using System;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class FragGrenadePickup : Pickup
    {
        protected override void OnPicked(PlayerCharacterController player)
        {
            if (player)
            {
                player.AddOneGrenade();
                PlayPickupFeedback();
                Destroy(gameObject);
            }
        }
    }
}