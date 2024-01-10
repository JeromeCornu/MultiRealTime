using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

public class SpawnBoss : MonoBehaviour
{
    [SerializeField]
    List<GameObject> playerList;
    [SerializeField]
    GameObject bossToSpawn;
    [SerializeField]
    Transform positionToSpawnBoss;


    private int numberOfMaxPlayer = 0;
    private int numberOfPlayerInZone = 0;

    private bool bossAsSpawned;

    void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
      
            playerList.Add(player);
        }
        numberOfMaxPlayer = playerList.Count;
    }

    private void Update()
    {
        if(!bossAsSpawned && numberOfPlayerInZone == numberOfMaxPlayer)
        {
            bossAsSpawned = true;
            Instantiate(bossToSpawn, positionToSpawnBoss);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            numberOfPlayerInZone += 1;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        numberOfPlayerInZone -= 1;
    }

}
