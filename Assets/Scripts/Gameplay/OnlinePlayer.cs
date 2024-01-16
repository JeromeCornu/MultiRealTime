using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Netcode;
using UnityEngine;

public abstract class Timer
{
    protected float initialTime;
    protected float Time { get; set; }
    public bool IsRunning { get; protected set; }

    public float Progress => Time / initialTime;

    //public Action OnTimerStart = delegate { };
    //public Action OnTimerStop = delegate { };

    protected Timer(float value)
    {
        initialTime = value;
        IsRunning = false;
    }

    public void Start()
    {
        Time = initialTime;
        if (!IsRunning)
        {
            IsRunning = true;
            //OnTimerStart.Invoke();
        }
    }

    public void Stop()
    {
        if (IsRunning)
        {
            IsRunning = false;
            //OnTimerStop.Invoke();
        }
    }

    public void Resume() => IsRunning = true;
    public void Pause() => IsRunning = false;

    public abstract void Tick(float deltaTime);
}
public class CountdownTimer : Timer
{
    public CountdownTimer(float value) : base(value) { }

    public override void Tick(float deltaTime)
    {
        if (IsRunning && Time > 0)
        {
            Time -= deltaTime;
        }

        if (IsRunning && Time <= 0)
        {
            Stop();
        }
    }

    public bool IsFinished => Time <= 0;

    public void Reset() => Time = initialTime;

    public void Reset(float newTime)
    {
        initialTime = newTime;
        Reset();
    }
}

public class OnlinePlayer : NetworkBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private float _mouseSensitivity;
    [SerializeField] private float _movementSpeed;

    private CharacterController _characterController;
    private float _verticalRotation = 0f;
    private float _horizontalRotation = 0f;

    Vector3 move;

    #region Online
    public struct InputPayload : INetworkSerializable
    {
        public int tick;
        public Vector3 inputVector;
        public float verticalView;
        public float horizontalView;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref inputVector);
        }
    }
    public struct StatePayload : INetworkSerializable
    {
        public int tick;
        public Vector3 position;
        public Quaternion rotation;
        public Quaternion cameraRotation;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref rotation);
        }
    }


    NetworkTimer _timer;
    const float _serverTickRate = 60f;
    const int _bufferSize = 1024;

    CircularBuffer<StatePayload> _clientStateBuffer;
    CircularBuffer<InputPayload> _clientInputBuffer;
    StatePayload _lastServerState;
    StatePayload lastProcessedState;
    

    CircularBuffer<StatePayload> _serverStateBuffer;
    Queue<InputPayload> _serverInputQueue;

    [SerializeField] float reconcialitionCountdown =10f;
    [SerializeField] float _reconciliationThreshold = 10f;

    CountdownTimer reconciliationCooldown;


    #endregion



    private void Awake()
    {
        _timer = new NetworkTimer(_serverTickRate);
        _clientStateBuffer = new CircularBuffer<StatePayload>(_bufferSize);
        _clientInputBuffer = new CircularBuffer<InputPayload>(_bufferSize);

        _serverStateBuffer = new CircularBuffer<StatePayload>(_bufferSize);
        _serverInputQueue = new Queue<InputPayload>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if(!IsOwner)
        {
            _camera.enabled = false;
            return;
        }

        _characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    // Update is called once per frame
    void Update()
    {
        _timer.Update(Time.deltaTime);

        if(!IsOwner ) { return; }

        move = Vector3.zero;
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        move.x = horizontalInput;
        move.z = verticalInput;


        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity;



        _verticalRotation += mouseY;
        _verticalRotation = Mathf.Clamp(_verticalRotation, -90f, 90f);

        _horizontalRotation = mouseX;

        
    }

    private void FixedUpdate()
    {
        while(_timer.ShouldTick())
        {
            HandleClientTick();
            HandleServerTick();
        }
    }

    void HandleServerTick()
    {
        if(!IsServer) { return; }
        var bufferIndex = -1;
        while(_serverInputQueue.Count > 0)
        {
            InputPayload inputPayload = _serverInputQueue.Dequeue();
            bufferIndex = inputPayload.tick % _bufferSize;

            StatePayload statePayload = ProcessMovement(inputPayload);
            _serverStateBuffer.Add(statePayload, bufferIndex);
        }

        if(bufferIndex == -1) { return; }
        SendToClientRpc(_serverStateBuffer.Get(bufferIndex));
    }


    [ClientRpc]
    void SendToClientRpc(StatePayload statePayload)
    {
        if (!IsOwner) return;
        _lastServerState = statePayload;
    }

    void HandleClientTick()
    {
        if(!IsClient || !IsOwner) return;

        var currentTick = _timer.CurrentTick;
        var bufferIndex = currentTick % _bufferSize;

        InputPayload inputPayload = new InputPayload()
        {
            tick = currentTick,
            inputVector = move,
            horizontalView = _horizontalRotation,
            verticalView = _verticalRotation,
        };
        _clientInputBuffer.Add(inputPayload, bufferIndex);
        SendToServerRpc(inputPayload);

        StatePayload statePayload = ProcessMovement(inputPayload);
        _clientStateBuffer.Add(statePayload, bufferIndex);


        HandleServerReconciliation();
    }

    bool ShouldReconcile()
    {
        bool isNewServerState = !_lastServerState.Equals(default);
        bool isLastStateUndefinedOrDifferent = lastProcessedState.Equals(default) || !lastProcessedState.Equals(_lastServerState); 

        return isNewServerState && isLastStateUndefinedOrDifferent;
    }

    void HandleServerReconciliation()
    {
        if (!ShouldReconcile()) return;
        float positionError;
        int bufferIndex;
        StatePayload rewindState = default;

        bufferIndex = _lastServerState.tick % _bufferSize;
        if (bufferIndex - 1 < 0) return;

        rewindState = IsHost ? _serverStateBuffer.Get(bufferIndex - 1) : _lastServerState; // Evite a l'host de s'update comme un client pour rien

        positionError = Vector3.Distance(rewindState.position, _clientStateBuffer.Get(bufferIndex).position);
        if(positionError > _reconciliationThreshold)
        {
            ReconcileState(rewindState);
        }

        lastProcessedState = _lastServerState;
    }

    void ReconcileState(StatePayload rewindState)
    {
        transform.position = rewindState.position;
        transform.rotation = rewindState.rotation;

        if (!rewindState.Equals(_lastServerState)) return;
        _clientStateBuffer.Add(rewindState, rewindState.tick);

        //replay all input front the rewind to 

        int tickToReplay = _lastServerState.tick;
        while(tickToReplay < _timer.CurrentTick)
        {
            int bufferIndex = tickToReplay % _bufferSize;
            StatePayload statePayload = ProcessMovement(_clientInputBuffer.Get(bufferIndex));
            _clientStateBuffer.Add(statePayload, bufferIndex);
            tickToReplay++;
        }
    }


    [ServerRpc]
    void SendToServerRpc(InputPayload input)
    {
        _serverInputQueue.Enqueue(input);
    }

    StatePayload ProcessMovement(InputPayload input)
    {
        Move(input.inputVector);
        RotateView(input.verticalView, input.horizontalView);

        return new StatePayload()
        {
            tick = input.tick,
            position = transform.position,
            rotation = transform.rotation,
            cameraRotation = _camera.transform.localRotation
        };
    }

    void RotateView(float verticalRotation, float horizontalRotation)
    {
        _camera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        transform.rotation *= Quaternion.Euler(0f, horizontalRotation, 0f);
        //transform.rotation = Quaternion.Lerp(transform.rotation, transform.rotation + Quaternion.Euler(0f, horizontalRotation, 0f), _timer.MinTimeBetweenTicks);
    }

    void Move(Vector3 inputVector)
    {
        //transform.position += new Vector3(inputVector.x, inputVector.y, 0);
        Vector3 movementDirection =
            (transform.TransformDirection(Vector3.forward) * inputVector.z) + (transform.TransformDirection(Vector3.right) * inputVector.x);

        _characterController.Move(movementDirection * _movementSpeed * _timer.MinTimeBetweenTicks);
    }
}
