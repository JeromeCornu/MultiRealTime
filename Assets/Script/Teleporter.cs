using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [SerializeField]
    private Teleporter linkedTeleporter;

    private void OnTriggerEnter(Collider other)
    {

            Teleport(other.transform);
   
    }

    private void Teleport(Transform player)
    {
        if(linkedTeleporter != null)
        {
            Debug.Log("Position avant téléportation : " + player.position);
            player.position = linkedTeleporter.transform.position + new Vector3(10.0f,0f,0f);
            Debug.Log("Position après téléportation : " + player.position);
        }
    }
}
