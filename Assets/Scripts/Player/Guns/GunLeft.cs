using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunLeft : MonoBehaviour
{
    [Header("Bullet")]
    [SerializeField] private Transform initialPos;
    [SerializeField] private Rigidbody bullet;

    private Ray ray;
    private RaycastHit hit;
    private float rayDistance;

    [Header("Camera")] 
    [SerializeField] private PlayerMovement catBaseScript;
    private Camera cam;
    private Vector3 direction;
    [SerializeField] private LayerMask aimLayerMask = new LayerMask();
    
    
    [Header("Vfx")] 
    [SerializeField] private Transform muzzlePos;
    [SerializeField] private GameObject muzzlePrefab;
    
    void Awake()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
       
        direction = catBaseScript.camera.position - (initialPos.position + initialPos.forward);
        
    }

    private void FixedUpdate()
    {
        RaycastCam();
        
        
        
        
    }

    private void RaycastCam()
    {
        
        
        Vector3 rayCamOrigin = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f));
        ray = new Ray(rayCamOrigin, cam.transform.forward);
        
        if (Physics.Raycast(ray, out hit, 999f, aimLayerMask))
        {
            rayDistance = hit.distance;
            Debug.Log(rayDistance);
            Debug.DrawLine(initialPos.position, hit.point, Color.red);
            Debug.DrawRay(cam.transform.position, cam.transform.forward * rayDistance, Color.yellow);
        }
        else
        {
            //Debug.DrawRay(initialPos.position, initialPos.forward * 100,Color.magenta);
        }
    }

    public void CanShoot()
    {
        MuzzleVFX();
        Debug.Log("Disparo");
        Rigidbody bulletClone = Instantiate(bullet, initialPos.position, initialPos.rotation);
        bulletClone.transform.LookAt(initialPos.position + initialPos.forward + direction + catBaseScript.camera.forward * rayDistance);
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
