using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
[SerializeField] private float maxSpeed = 4f;

    [SerializeField] private float accel = 5f;
    [SerializeField] float yCamSmoothness = 1f;
    [SerializeField] float xCamSmoothness = 1f;
    [SerializeField] float jumpHeight = 2f;
    [SerializeField] float timeToStop = .25f;
    [SerializeField] int maxJumpCharges = 2;
    [SerializeField] bool isOnFloor = false;

    int jumpChargesRemaining;

    private Camera cam;
    private Vector3 velocityVector;
    private Vector3 startPos;
    private float startJump = 0;
    private Transform meshTransform;
    private CapsuleCollider collider;
    private Rigidbody rb;

    private Vector3 negativeVel;
    private bool[] isDecel = {false,false};
    public float moveTime = 0;
    public RigidbodyConstraints startingConstraints; 

    void Start()
    {
        cam = GetComponentInChildren<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
        collider = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();

        if(rb != null)
        {
            startingConstraints = rb.constraints;
        }

        meshTransform = transform.Find("Mesh");

        jumpChargesRemaining = maxJumpCharges;
        velocityVector = new Vector3(0,0,0);
        startPos = transform.position;

        negativeVel = new Vector3(0,0,0);
        

    }


    void HandleAngle()
    {
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");


        Vector3 angle = cam.transform.rotation.eulerAngles;

        if(Mathf.Abs(mouseY) > 0)
        {
            
            float dAngle = -mouseY* yCamSmoothness;



            if(dAngle + angle.x > 300 || dAngle + angle.x < 60)
            {
                angle.x += dAngle;
            }
        }


        if(Mathf.Abs(mouseX) > 0)
        {

            float dAngle = mouseX* xCamSmoothness;
            angle.y += dAngle;
        }

        cam.transform.rotation = Quaternion.Euler(angle);
        angle.x = 0;
        angle.z = 0;
        meshTransform.transform.rotation = Quaternion.Euler(angle);;

    }

    void HandleMovement()
    {
        float horizontalMovement = Input.GetAxisRaw("Horizontal");
        float verticalMovement = Input.GetAxisRaw("Vertical");

        
        Vector3 forward = cam.transform.forward;
        Vector3 right = cam.transform.right;

        // dont want movement in y dim
        forward.y = 0f; 
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        /*
        if(horizontalMovement == 0 && Mathf.Abs(rb.velocity.x) > 1)
        {
            isDecel[0] = true;
            negativeVel.x  = rb.velocity.x / timeToStop * Time.deltaTime;
        }
        else
        {
            isDecel[0] = false;
            negativeVel.x = 0;
        }

        if(isDecel[0])
        {
            Vector3 copy = rb.velocity;
            copy.x -= negativeVel.x;
            rb.velocity = copy;
        }

        if(verticalMovement == 0 && Mathf.Abs(rb.velocity.z) > 1)
        {
            isDecel[0] = true;
            negativeVel.z  = rb.velocity.z / timeToStop * Time.deltaTime;
        }
        else
        {
            isDecel[1] = false;
            negativeVel.z = 0;
        }

        if(isDecel[1])
        {
            Vector3 copy = rb.velocity;
            copy.z -= negativeVel.z;
            rb.velocity = copy;
        }
        */

        if(horizontalMovement == 0 && verticalMovement == 0)
        {
            return;
        }




        Vector3 movementDirection = (horizontalMovement * right + verticalMovement * forward).normalized;

        Vector3 newVelocity = rb.velocity + movementDirection * accel * Time.deltaTime;
        


        if(Mathf.Abs(rb.velocity.x) > maxSpeed)
        {
            newVelocity.x = Mathf.Sign(rb.velocity.x) * maxSpeed;
        }

        if(Mathf.Abs(rb.velocity.z) > maxSpeed)
        {
            newVelocity.z = Mathf.Sign(rb.velocity.z) * maxSpeed;
        }


        PrepareForMovement();
        rb.velocity =  newVelocity;

    }

    void PrepareForMovement()
    {

        rb.isKinematic = false;
        moveTime = Time.time;
        rb.constraints = startingConstraints; 
    }

    void HandleJump()
    {
        if(isOnFloor && Time.time - startJump > 0.1f)
        {
            jumpChargesRemaining = maxJumpCharges;
        }

        if(!Input.GetButtonDown("Jump"))
        {
            return;
        }

        if(!isOnFloor && jumpChargesRemaining < 1)
        {
            return;
        }
        
        jumpChargesRemaining -= 1;

        float startHeight =  transform.position.y;
        float targetHeight = startHeight + jumpHeight;


        float apexTime = Mathf.Sqrt((2f * jumpHeight) / -Physics.gravity.y);
        float distanceRemaining = targetHeight - transform.position.y;

        float v0 = (distanceRemaining / apexTime)  - (  (Physics.gravity.y * apexTime) / 2f);
        rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y + v0, rb.velocity.z);

        startJump = Time.time;

        PrepareForMovement();
    }
     
    void FixedUpdate()
    {
        isOnFloor = Physics.Raycast(transform.position, -Vector3.up, collider.bounds.extents.y + 0.1f);
        
    }

    void Update()
    {
        HandleJump();
        HandleMovement();
        HandleAngle();
    }
}
