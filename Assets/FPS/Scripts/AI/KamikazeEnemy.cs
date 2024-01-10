using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.AI
{
    [RequireComponent(typeof(EnemyController))]
    public class KamikazeEnemy : MonoBehaviour
    {
        public enum AIState
        {
            Approach,
            Explode,
        }

        public AIState KamikazeAiState
        {
            get { return m_KamikazeAiState; }
            set { m_KamikazeAiState = value; }
        }

        private AIState m_KamikazeAiState;

        public Animator Animator;

        [Header("Sound")] public AudioClip MovementSound;
        public MinMaxFloat PitchDistortionMovementSpeed;

        [Header("Parameters")]
        public float ExplosionRadius = 2f;
        public float ExplosionDamage = 30f;
        public float ExplosionForce = 500f;

        EnemyController m_EnemyController;
        AudioSource m_AudioSource;

        const string k_AnimMoveSpeedParameter = "MoveSpeed";
        const string k_AnimExplodeParameter = "Explode";

        void Start()
        {
            m_EnemyController = GetComponent<EnemyController>();
            DebugUtility.HandleErrorIfNullGetComponent<EnemyController, KamikazeEnemy>(m_EnemyController, this,
                gameObject);

            // Start approaching
            KamikazeAiState = AIState.Approach;

            // Add an audio source
            m_AudioSource = GetComponent<AudioSource>();
            DebugUtility.HandleErrorIfNullGetComponent<AudioSource, EnemyMobile>(m_AudioSource, this, gameObject);
            m_AudioSource.clip = MovementSound;
        }


        void Update()
        {
            UpdateAiStateTransitions();
            UpdateCurrentAiState();

            float moveSpeed = m_EnemyController.NavMeshAgent.velocity.magnitude;

            // Update animator speed parameter
            Animator.SetFloat(k_AnimMoveSpeedParameter, moveSpeed);

            // changing the pitch of the movement sound depending on the movement speed
            m_AudioSource.pitch = Mathf.Lerp(PitchDistortionMovementSpeed.Min, PitchDistortionMovementSpeed.Max,
                moveSpeed / m_EnemyController.NavMeshAgent.speed);

            // Play the audio only if in the approach state
            if (KamikazeAiState == AIState.Approach && !m_AudioSource.isPlaying)
            {
                m_AudioSource.Play();
            }
        }

        void UpdateAiStateTransitions()
        {
            // Handle transitions 
            switch (KamikazeAiState)
            {
                case AIState.Approach:
                    // Transition to explode when close to the player
                    if (Vector3.Distance(transform.position, m_EnemyController.KnownDetectedTarget.transform.position) <= ExplosionRadius)
                    {
                        Debug.Log("Transition to Explode");
                        KamikazeAiState = AIState.Explode;
                        m_EnemyController.SetNavDestination(transform.position); // Stop moving
                    }
                    break;
            }
        }

        void UpdateCurrentAiState()
        {
            // Handle logic 
            switch (KamikazeAiState)
            {
                case AIState.Approach:
                    m_EnemyController.SetNavDestination(m_EnemyController.KnownDetectedTarget.transform.position);
                    m_EnemyController.OrientTowards(m_EnemyController.KnownDetectedTarget.transform.position);
                    m_EnemyController.OrientWeaponsTowards(m_EnemyController.KnownDetectedTarget.transform.position);
                    break;
                case AIState.Explode:
                    Explode();
                    break;
            }
        }

        void Explode()
        {
            // Damage and push nearby objects
            Collider[] colliders = Physics.OverlapSphere(transform.position, ExplosionRadius);
            foreach (Collider hitCollider in colliders)
            {
                // Ignore the kamikaze enemy itself
                if (hitCollider.gameObject == gameObject)
                    continue;

                // Damage health component if exists
                Health health = hitCollider.GetComponent<Health>();
                if (health)
                {
                    // Pass null as damage source since it's a kamikaze explosion
                    health.TakeDamage(ExplosionDamage, null);
                }

                // Apply force to Rigidbody if exists
                Rigidbody rb = hitCollider.GetComponent<Rigidbody>();
                if (rb)
                {
                    rb.AddExplosionForce(ExplosionForce, transform.position, ExplosionRadius);
                }
            }

            // Destroy the kamikaze enemy
            Destroy(gameObject);
        }
    }
}
