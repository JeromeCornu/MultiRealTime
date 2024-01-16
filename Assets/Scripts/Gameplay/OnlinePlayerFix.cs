using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using Unity.Netcode;
using UnityEngine;

public struct InputPayload
{
    public int tick;
    public Vector3 inputVector;
}

public struct StatePayload
{
    public int tick;
    public Vector3 position;
}

public class OnlinePlayerFix : NetworkBehaviour
{
    private NetworkTimer _timer;

    private const float _serverTickRate = 30f;
    private const int BUFFER_SIZE = 1024;

    private StatePayload[] stateBuffer;
    private InputPayload[] inputBuffer;
    private StatePayload latestServerState;
    private StatePayload lastProcessedState;
    [SerializeField] private Camera _camera;

    // Server
    private StatePayload[] stateBufferServer;
    Queue<InputPayload> inputQueue;

    private Vector3 move;

    private void Awake()
    {
    }

    private void Start()
    {
        if(!IsOwner)
        {
            _camera.enabled = false;
        }
        if(IsServer)
        {
            stateBufferServer = new StatePayload[BUFFER_SIZE];
            inputQueue = new Queue<InputPayload>();
        }
        _timer = new NetworkTimer(_serverTickRate);
        stateBuffer = new StatePayload[BUFFER_SIZE];
        inputBuffer = new InputPayload[BUFFER_SIZE];
    }

    private void Update()
    {
        _timer.Update(Time.deltaTime);
        if (_timer.ShouldTick())
        {
            HandleTick();
        }
        if (!IsOwner) { return; }

        move = Vector3.zero;
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        move = new Vector3(horizontalInput, 0, verticalInput);
    }

    void HandleServerReconciliation()
    {
        lastProcessedState = latestServerState;

        int serverStateBufferIndex = latestServerState.tick % BUFFER_SIZE;
        float positionError = Vector3.Distance(latestServerState.position, stateBuffer[serverStateBufferIndex].position);

        if(positionError < 0.1f)
        {
            Debug.Log("we have to reconcile");

            transform.position = latestServerState.position;

            stateBuffer[serverStateBufferIndex] = latestServerState;

            int tickToProcess = latestServerState.tick + 1;
            while (tickToProcess < _timer.CurrentTick) 
            {

                int bufferIndex = tickToProcess % BUFFER_SIZE;
                StatePayload statePayload = ProcessMovement(inputBuffer[bufferIndex]);
                stateBuffer[bufferIndex] = statePayload;

                tickToProcess++;

            }

        }

    }

    public void HandleClientTick()
    {
        if (!IsClient || !IsOwner) return;

        int bufferIndex = _timer.CurrentTick % BUFFER_SIZE;

        // Add payload to inputBuffer
        InputPayload inputPayload = new InputPayload();
        inputPayload.tick = _timer.CurrentTick;
        inputPayload.inputVector = move;

        inputBuffer[bufferIndex] = inputPayload;


        stateBuffer[bufferIndex] = ProcessMovement(inputPayload);

        StartCoroutine(SendToServer(inputPayload));
    }

    public void HandleTick()
    {
        HandleClientTick();
        if(IsServer)
        {

            if (!latestServerState.Equals(default(StatePayload)) && lastProcessedState.Equals(default(StatePayload)) || !latestServerState.Equals(lastProcessedState))
            {
                HandleServerReconciliation();
            }


            int bufferIndex = -1;
            while (inputQueue.Count > 0) 
            {
                InputPayload inputPayload = inputQueue.Dequeue();

                bufferIndex = inputPayload.tick % BUFFER_SIZE;
                
                StatePayload statePayload = ProcessMovement(inputPayload);
                stateBufferServer[bufferIndex] = statePayload;

            }
            if(bufferIndex != -1)
            {
                StartCoroutine(SendToClient(stateBufferServer[bufferIndex]));
            }
        }
    }

    public void OnServerMovementState(StatePayload serverState)
    {
        latestServerState = serverState;
    }

    IEnumerator SendToServer(InputPayload inputPayload)
    {
        yield return new WaitForSeconds(0.02f);
        OnClientInput(inputPayload);
        
    }

    IEnumerator SendToClient(StatePayload statePayload)
    {
        yield return new WaitForSeconds(0.02f);

        OnServerMovementState(statePayload);
    }

    public void OnClientInput(InputPayload inputPayload) 
    {
        inputQueue.Enqueue(inputPayload);
    }

    StatePayload ProcessMovement(InputPayload input)
    {
        transform.position += input.inputVector * 5f * _timer.MinTimeBetweenTicks;

        return new StatePayload()
        {
            tick = input.tick,
            position = transform.position,
        };
    }
}
