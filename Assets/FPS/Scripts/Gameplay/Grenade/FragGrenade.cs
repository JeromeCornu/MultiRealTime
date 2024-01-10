using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

public class FragGrenade : MonoBehaviour
{
    public float timeBeforeExplosion = 3f;
    public GameObject explosionEffect;
    public float damage = 50f;
    public float explosionRadius = 5f;

    void Start()
    {
        Invoke("ExplodeGrenade", timeBeforeExplosion);
    }

    void ExplodeGrenade()
    {
        Instantiate(explosionEffect, transform.position, transform.rotation);

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
