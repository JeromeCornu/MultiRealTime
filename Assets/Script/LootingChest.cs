using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Gameplay;
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
        rotationFinale = Quaternion.Euler(90f, 0f, 0f);
    }

    private void Update()
    {

        if (Input.GetKeyDown(keyToInteract) && isNearOfChest && canDrop && objectToDrop != null)
        {
            DropObject();
        }

    }


    private void DropObject()
    {
        canDrop = false;
        StartCoroutine(LerpRotation());
        Instantiate(objectToDrop, pointToDrop.transform.position, pointToDrop.transform.rotation);
    }


    IEnumerator LerpRotation()
    {
        float elapsedTime = 0f;
        float lerpPercentage = 0f;

        while (elapsedTime < 1f)
        {
            animatePivot.rotation = Quaternion.Lerp(rotationInitiale, rotationFinale, lerpPercentage);

            lerpPercentage = elapsedTime / 1f;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        animatePivot.rotation = rotationFinale;
    }


    public void OnTriggerEnter(Collider other)
    {
         isNearOfChest = true;
        print("entering");
    }

    public void OnTriggerExit(Collider other)
    {
        isNearOfChest = false;
    }
}
