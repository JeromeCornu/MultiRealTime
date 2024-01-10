using System.Collections;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class HealthPickup : Pickup
    {
        [Header("Parameters")]
        [Tooltip("Amount of health to heal on pickup")]
        public float healAmount;

        [Header("Spawn Settings")]
        [Tooltip("Enable continuous spawning")]
        public bool SpawnInLoop;

        [Tooltip("Time interval between spawns (in seconds)")]
        public float spawnInterval = 10f;

        [SerializeField]
        private MeshRenderer rendererToHide;

        protected override void OnPicked(PlayerCharacterController player)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth && playerHealth.CanPickup())
            {
                playerHealth.Heal(healAmount);
                PlayPickupFeedback();

                if (!SpawnInLoop)
                {
                    Destroy(gameObject);
                }
                else
                {
                    StartCoroutine(SpawnLoop());
                    m_HasPlayedFeedback = false;
                }
            }
        }

        private IEnumerator SpawnLoop()
        {
            SetComponentsActive(false);

            yield return new WaitForSeconds(spawnInterval);

            SetComponentsActive(true);
        }

        private void SetComponentsActive(bool active)
        {
            Collider collider = GetComponent<Collider>();

            if (rendererToHide)
            {
                rendererToHide.enabled = active;
            }

            if (collider)
            {
                collider.enabled = active;

            }
        }
    }
}
