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

    private Vector3 positionFinale = new Vector3(0.0f, 8.0f, 0.0f);


    private void Update()
    {
        if (CheckAllEnemyDied() && isDoorAlreadyOpened)
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
        Debug.Log("caca");
        float elapsedTime = 0f;
        float lerpSpeed = 1.0f;
        while (elapsedTime < 1f)
        {
            transform.position = Vector3.Lerp(transform.position, positionFinale, lerpSpeed);
            elapsedTime += Time.deltaTime * lerpSpeed;
            yield return null;
        }
        isDoorAlreadyOpened = false;
        Destroy(gameObject);
    }

}
