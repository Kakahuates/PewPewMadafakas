using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerMoveTest : MonoBehaviour
{
    [Header("Cameras")]//camara
    private new Transform camera;
    [SerializeField] private CinemachineVirtualCamera cm;
    
    [Header("Movement")]
    private Vector2 horizontalMovement;
    private Vector3 direction;
    private Vector3 slowDown; //ref smoothdamp
    private Rigidbody rb;
    [SerializeField] private float speed;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float desacelerationSpeed;
    [SerializeField] private float rotSpeed;

    [Header("Ground Detection")] 
    private bool isGrounded;
    private Ray ray;
    private RaycastHit hit;
    [SerializeField] private float rayLength;
    [SerializeField] private LayerMask groundLayer;
    
    
    
    void Start()
    {
        camera = Camera.main.transform;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        InputMovement();
        
    }

    private void FixedUpdate()
    {
        OnGroud();
        Movement();
        RotationNoAiming();
    }

    private void InputMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        direction = new Vector3(horizontal, 0, vertical);
        direction = direction.normalized;
    }

    private void OnGroud()
    {
        ray.origin = transform.position;
        ray.direction = -transform.up;

        if (Physics.Raycast(ray, out hit, rayLength, groundLayer))
        {
            isGrounded = true;
            Debug.DrawRay(ray.origin, ray.direction * rayLength, Color.red);
        }
        else
        {
            isGrounded = false;
            Debug.DrawRay(ray.origin, ray.direction * rayLength, Color.blue);
        }
    }

    private void Movement()
    {
        horizontalMovement = new Vector2(rb.velocity.x, rb.velocity.z);
        if (horizontalMovement.magnitude > maxSpeed)
        {
            horizontalMovement = horizontalMovement.normalized;
            horizontalMovement *= maxSpeed;
        }

        rb.velocity = new Vector3(horizontalMovement.x, rb.velocity.y, horizontalMovement.y);

        if (isGrounded)
        {
            rb.AddForce(new Vector3(direction.x * speed * 10 * Time.deltaTime, rb.velocity.y,  direction.z * speed * 10 * Time.deltaTime));
        }
        else
        {
            rb.AddForce(new Vector3(direction.x * speed * 10/2 * Time.deltaTime, rb.velocity.y,  direction.z * speed * 10 * Time.deltaTime));
        }

        if (isGrounded)
        {
            rb.velocity = Vector3.SmoothDamp(rb.velocity, new Vector3(0, rb.velocity.y, 0), ref slowDown,
                desacelerationSpeed);
        }
    }

    private void RotationNoAiming()
    {
        Quaternion targetRot = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * rotSpeed );
    }
}
