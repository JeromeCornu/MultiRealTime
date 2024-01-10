using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> ennemyInZone;


    private bool isDoorAlreadyOpened;

    private Vector3 positionInitiale;
    private Vector3 positionFinale;

    private void Start()
    {
        positionInitiale = transform.position;
        positionFinale = transform.position;
        positionFinale.y += 8.0f;
    }

    private void Update()
    {
        if (CheckAllEnemyDied() && !isDoorAlreadyOpened)
        {
            StartCoroutine(OpenDoor());
        }
    }


    private bool CheckAllEnemyDied()
    {
        ennemyInZone.RemoveAll(item => item == null);

        if (ennemyInZone.Count > 0)
        {
            return false;
        }
        return true;

    }

    IEnumerator OpenDoor()
    {
        float elapsedTime = 0f;
        float lerpDuration = 5.0f;
        while (elapsedTime < lerpDuration)
        {
            float t = elapsedTime / lerpDuration;
            transform.position = Vector3.Lerp(positionInitiale, positionFinale, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = positionFinale;
        isDoorAlreadyOpened = true;
        Destroy(gameObject);
    }

}
