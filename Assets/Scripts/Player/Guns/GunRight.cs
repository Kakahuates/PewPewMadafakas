using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunRight : MonoBehaviour
{
    [Header("Bullet")]
    [SerializeField] private Transform initialPos;
    [SerializeField] private Rigidbody bullet;

    [Header("Camera")] 
    [SerializeField] private PlayerMovement catBaseScript;
    private Vector3 direction;

    [Header("Vfx")] 
    [SerializeField] private Transform muzzlePos;
    [SerializeField] private GameObject muzzlePrefab;
    
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        direction = catBaseScript.camera.position - (initialPos.position + initialPos.forward);
        
    }

    private void FixedUpdate()
    {
        Debug.DrawRay(initialPos.position, direction + catBaseScript.camera.forward * catBaseScript.camDistanceZ, Color.red );
        
        Debug.DrawRay(catBaseScript.camera.position, catBaseScript.camera.forward * catBaseScript.camDistanceZ, Color.yellow);
        
    }

    public void CanShoot()
    {
        MuzzleVFX();
        Debug.Log("Disparo");
        Rigidbody bulletClone = Instantiate(bullet, initialPos.position, initialPos.rotation);
        bulletClone.transform.LookAt(initialPos.position + initialPos.forward + direction + catBaseScript.camera.forward * catBaseScript.camDistanceZ);
        bulletClone.AddForce(bulletClone.transform.forward * catBaseScript.shootForce, ForceMode.VelocityChange);
            
        Destroy(bulletClone.gameObject, 10.0f);
    }

    private void MuzzleVFX()
    {
        if (muzzlePrefab != null)
        {
            GameObject muzzleVfx = Instantiate(muzzlePrefab, muzzlePos.transform.position, Quaternion.identity);
            muzzleVfx.transform.forward = muzzlePos.transform.forward;

            ParticleSystem psMuzzle = muzzleVfx.GetComponent<ParticleSystem>();
            if (psMuzzle != null)
            {
                Destroy(muzzleVfx, psMuzzle.main.duration);
            }
            else
            {
                ParticleSystem psChild = muzzleVfx.transform.GetChild(0).GetComponent<ParticleSystem>();
                Destroy(muzzleVfx, psChild.main.duration);
            }
        }
    }
}
