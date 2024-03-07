using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("VFX")]
    [SerializeField] private GameObject hitPrefab;
    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint contact = collision.contacts[0]; //donde golpea la bala, el primer contacto
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, contact.normal); //rotar nuestro objeto deacuerdo a la normal del lugar donde golpea
        Vector3 positionContact = contact.point; //la posicion del punto de contacto
        
        if (hitPrefab != null)
        {
            GameObject hitVfx = Instantiate(hitPrefab, positionContact, rotation);
            ParticleSystem psHit = hitVfx.GetComponent<ParticleSystem>();
            if (psHit != null)
            {
                Destroy(hitVfx, psHit.main.duration);
            }
            else
            {
                ParticleSystem psChild = hitVfx.transform.GetChild(0).GetComponent<ParticleSystem>();
                Destroy(hitVfx, psChild.main.duration);
            }
            
        }
        
        Destroy(gameObject);
    }
    
}
