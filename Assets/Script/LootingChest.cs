using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LootingChest : MonoBehaviour
{
    [SerializeField]
    private KeyCode keyToInteract;
    [SerializeField]
    private GameObject objectToDrop;
    [SerializeField]
    private Transform pointToDrop;
    [SerializeField]
    private bool canDrop = true;
    [SerializeField]
    private Transform animatePivot;

    private Quaternion rotationInitiale;
    private Quaternion rotationFinale;

    public bool isNearOfChest;

    private void Start()
    {
        rotationInitiale = transform.rotation;
    }

    private void Update()
    {

        if (Input.GetKeyDown(keyToInteract) && isNearOfChest && canDrop)
        {
            DropObject();
        }

    }


    private void DropObject()
    {
        canDrop = false;
        StartCoroutine(LerpRotation());
        Instantiate(objectToDrop, pointToDrop);
    }


    IEnumerator LerpRotation()
    {
        float elapsedTime = 0f;
        float lerpSpeed = 1.0f;
        while (elapsedTime < 1f)
        {
            animatePivot.rotation = Quaternion.Lerp(rotationInitiale, rotationFinale, lerpSpeed);
            elapsedTime += Time.deltaTime * lerpSpeed;
            yield return null;
        }

        
    }

    public void OnTriggerEnter(Collider other)
    {
        isNearOfChest = true;
    }

    public void OnTriggerExit(Collider other)
    {
        isNearOfChest = false;
    }
}
