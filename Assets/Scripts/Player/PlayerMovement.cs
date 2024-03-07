using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float acceleration;
    [SerializeField] private float maxAcceleration;
    [SerializeField] private float desaccelerationSpeed;
    [SerializeField] private float turnSpeed;
    [SerializeField] private bool isMoving;
    
    private Vector3 direction;
    private Vector3 desiredRotation;
    public new Transform camera; 

    private float horizontal, vertical;
    private Vector2 horizontalMovement; //guardar las velocidades de X y Z
    private Vector3 slowdowm; //es el ref del Vector3.SmoothDamp
    private float currentVel;
    [SerializeField] private bool isGrounded;

    [Header("Raycast")] 
    [SerializeField] private float rayLength;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform raycastOrigin;
    private Ray ray;
    private RaycastHit hit;

    [Header("Cinemachine")] 
    [SerializeField] private CinemachineVirtualCamera cm;

    [Header("Jump")] 
    [SerializeField] private float jumpForce;
    [SerializeField] private float fallMultiplier;
    private bool jumpPressed;
    private float gravity;
    private bool isInAir;
    
    private Rigidbody rb;
    private Animator anim;

    [Header("Shoot")] 
    [SerializeField] private bool isAiming;
    [SerializeField] private float coolDownTimer;
    [SerializeField] private float coolDown;
    [SerializeField] private GunLeft gunLeftScript;
    [SerializeField] private GunRight gunRightScript;
    [SerializeField] private float leftGunDelay;
    [SerializeField] private float rotSensitivity;
    public float shootForce;
    public float camDistanceZ;
    
    
    private void Awake()
    {
        camera = Camera.main.transform;
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        gravity = Physics.gravity.y;
        
    }

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        
        InputMovement();
        InputJump();
        Aiming();
        InputShoot();
    }

    private void FixedUpdate()
    {
        IsGrounded();
        Movement();
        if (isAiming)
        {
            RotationAiming();
        }else if (direction.magnitude != 0f)
        {
            Rotation();
        }
        Jump();
        
        
        
    }

    #region Movement and Ground Detection

    private void InputMovement()
    {
        //Inputs
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        
        //Camera direction
        Vector3 camForward = camera.forward;
        Vector3 camRight = camera.right;
        camForward.y = 0;
        camRight.y = 0;
        
        //direction
        direction = camForward * vertical + camRight * horizontal;
        direction.Normalize();
    }

    private void IsGrounded()
    {
        ray.origin = transform.position + Vector3.up * 0.01f;
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
        //limito la velocidad del rb para que no se vaya cada vez mas rápido como loco en cada frame
        //registro la velocidad que lleva el rb
        horizontalMovement = new Vector2(rb.velocity.x, rb.velocity.z);
                
        //Si se pasa de la velocidad maxima la magnitud del vector, lo normalizo y luego lo multiplico por maxAcceleration
        if (horizontalMovement.magnitude > maxAcceleration)
        {
            horizontalMovement = horizontalMovement.normalized;
            horizontalMovement *= maxAcceleration;
        }
        //aplico la nueva velocidad, velocity es un Vector3
        rb.velocity = new Vector3(horizontalMovement.x, rb.velocity.y, horizontalMovement.y);
        
        //aplico fuerza con input y aceleración por el tiempo si estoy en el suelo y si no estoy en el suelo reduzco
        //la velocidad a la mitad
        if (isGrounded)
        {
            rb.AddForce(new Vector3(direction.x * acceleration * 10 * Time.deltaTime, rb.velocity.y, 
                direction.z * acceleration * 10 * Time.deltaTime));
            anim.SetBool("isRunning", true);
            
        }else {
            rb.AddForce(new Vector3(direction.x * acceleration * 10 / 2 * Time.deltaTime, rb.velocity.y, 
            direction.z * acceleration * 10 / 2 * Time.deltaTime));
            
        }
        
        //desacelero el rb al final del movimiento con smoothdamp cambiando la velocidad actual a la que quiero llegar
        if (isGrounded)
        {
            rb.velocity = Vector3.SmoothDamp(rb.velocity, new Vector3(0, rb.velocity.y, 0), ref slowdowm,
                desaccelerationSpeed);
        }
        
        Animations();
        
    }

    private void Aiming()
    {
        if (Input.GetMouseButton(1) && !isAiming)
        {
            Debug.Log("entra apuntar");
            isAiming = true;
            GameplayUI.gamePlayUI.CrosshairVisibility(true);
            cm.Priority = 21;
        }

        if (Input.GetMouseButtonUp(1) && isAiming)
        {
            isAiming = false;
            GameplayUI.gamePlayUI.CrosshairVisibility(false);
            cm.Priority = 9;
        }
    }

    private void Rotation()
    {
        //el parámetro MaxRadiansDelta es la velocidad de la rotacion, y se multiplica por Time.deltaTime
        //el parámetro maxMagnitudDelta normalmente se pone a valor 0;
        //rotamos el vector del vector actual al target
        
        desiredRotation = Vector3.RotateTowards(transform.forward, direction, Time.deltaTime * turnSpeed, 0);
        //Creo la rotación con Quaternions
        Quaternion lookToRotation = Quaternion.LookRotation(desiredRotation);
        //aplico la rotación con quaternions
        rb.MoveRotation(lookToRotation);

    }

    private void RotationAiming()
    {
        float targetAngle = camera.eulerAngles.y;
        Quaternion rotation = Quaternion.Euler(0, targetAngle, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, turnSpeed * Time.deltaTime * rotSensitivity);
    }

    #endregion

    #region Jump and Animations

    private void InputJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            jumpPressed = true;
            anim.SetBool("isJumping", jumpPressed);
        }
    }

    private void Jump()
    {
        if (jumpPressed)
        {
            jumpPressed = false;
            rb.AddRelativeForce(Vector3.up * (jumpForce), ForceMode.Impulse);
            
        }
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * (gravity * fallMultiplier * Time.deltaTime);
        }
    }

    void Animations()
    {
        if(isGrounded && horizontalMovement == Vector2.zero){
            anim.SetBool("isRunning", false);
        }
        
        if(isGrounded) anim.SetBool("isJumping", jumpPressed);
    }

    #endregion

    #region Shoot

    private void InputShoot()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Time.time > leftGunDelay)
            {
                gunLeftScript.CanShoot();
            }
            gunRightScript.CanShoot();
        }
        
        if (Input.GetMouseButton(0)) //mantiene presionado el boton
        {
            if (coolDownTimer < coolDown)
            {
                coolDownTimer += Time.deltaTime;
            }
            else
            {
                coolDownTimer = 0f;
                if (Time.time > leftGunDelay)
                {
                    gunLeftScript.CanShoot();
                }
                gunRightScript.CanShoot();
            }
        }
        

        if (Input.GetMouseButtonUp(0))// suelta el boton
        {
            coolDownTimer = 0f;
        }
    }

    #endregion
    
}
