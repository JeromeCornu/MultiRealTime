using System.Collections;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.FPS.Gameplay
{
    public class FragGrenadePickup : Pickup
    {
        [Header("Spawn Settings")]
        [Tooltip("Enable continuous spawning")]
        public bool SpawnInLoop;

        [Tooltip("Time interval between spawns (in seconds)")]
        public float spawnInterval = 10f;

        [SerializeField]
        private MeshRenderer rendererToHide;

        protected override void OnPicked(PlayerCharacterController player)
        {
            if (player)
            {
                player.AddOneGrenade();
                PlayPickupFeedback();

                if (SpawnInLoop)
                {
                    m_HasPlayedFeedback = false;
                    StartCoroutine(SpawnLoop());
                }
                else
                {
                    Destroy(gameObject);
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
