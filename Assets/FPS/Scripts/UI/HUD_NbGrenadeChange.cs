using System;
using TMPro;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class HUD_NbGrenadeChange : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI grenadeNumber;

        private PlayerCharacterController playerCharacterController;

        private int currentGrenades = 0;

        private void Start()
        {
            playerCharacterController = FindObjectOfType<PlayerCharacterController>();

            if (playerCharacterController == null)
            {
                Debug.LogError("Didnt find any playerCharacterController in HUD_NbGrenadeChange");
            }
        }

        void Update()
        {
            if (currentGrenades != playerCharacterController.GetCurrentGrenades())
            {
                int currentGrenades = playerCharacterController.GetCurrentGrenades();
                grenadeNumber.SetText(currentGrenades.ToString());
            }
        }
    }
}
