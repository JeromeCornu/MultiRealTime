using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

public class FragGrenade : MonoBehaviour
{
    public float timeBeforeExplosion = 02f;
    public GameObject explosionEffect;
    public GameObject fragmentPrefab;
    public int numberOfFragments = 5;
    public float fragmentSpawnRadius = 2f;
    public float damage = 20f;
    public float explosionRadius = 5f;

    void Start()
    {
        Invoke("ExplodeGrenade", timeBeforeExplosion);
    }

    void ExplodeGrenade()
    {
        Instantiate(explosionEffect, transform.position, transform.rotation);

        // Spawn fragments
        for (int i = 0; i < numberOfFragments; i++)
        {
            Vector3 randomDirection = Random.onUnitSphere * fragmentSpawnRadius;
            randomDirection.y = 0f; // Keep the fragments on the same level as the grenade
            Vector3 fragmentPosition = transform.position + randomDirection;
            Quaternion fragmentRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            Instantiate(fragmentPrefab, fragmentPosition, fragmentRotation);
        }

        ApplyDamage();
        gameObject.SetActive(false);
    }

    void ApplyDamage()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (var collider in colliders)
        {
            Damageable damageable = collider.GetComponent<Damageable>();
            if (damageable)
            {
                damageable.InflictDamage(damage, false, null);
            }
        }
    }
}
