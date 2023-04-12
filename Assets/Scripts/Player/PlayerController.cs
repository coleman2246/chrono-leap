using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum MovementStates
{
    Accelerating,
    Decelerating,
    Stopped
}

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
    [SerializeField] float stepHeight = 0.3f;
    [SerializeField] Transform lowerStep;
    [SerializeField] Transform upperStep;

    int jumpChargesRemaining;

    private Camera cam;
    private Vector3 velocityVector;
    private Vector3 startPos;
    private float startJump = 0;
    private Transform meshTransform;
    private CapsuleCollider collider;
    private Rigidbody rb;
    private PlayerWorldInteractions playerWorld;

    private Vector3 negativeVel;
    private MovementStates[] moveStates = {MovementStates.Stopped,MovementStates.Stopped};
    private float[] velAtStart = {0,0};
    public float moveTime = 0;
    public RigidbodyConstraints startingConstraints; 

    void Start()
    {
        cam = GetComponentInChildren<Camera>();
        collider = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        playerWorld = GetComponent<PlayerWorldInteractions>();

        if(rb != null)
        {
            startingConstraints = rb.constraints;
        }

        meshTransform = transform.Find("Mesh");

        jumpChargesRemaining = maxJumpCharges;
        velocityVector = new Vector3(0,0,0);
        startPos = transform.position;


        negativeVel = new Vector3(0,0,0);

        upperStep.position = new Vector3(lowerStep.position.x, lowerStep.position.y + stepHeight, lowerStep.position.z);
        

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


    void HandleStopped(int direction, float movement)
    {
        
        if(movement != 0)
        {
            moveStates[direction] = MovementStates.Accelerating;

        }


    }

    float HandleAcceleration(int direction, float inputState)
    {
        if(inputState == 0)
        {
            moveStates[direction]  = MovementStates.Decelerating;
        }

        return accel * Mathf.Sign(inputState);
    }

    float HandleDeceleration(int direction, float inputState, float velocity, int directionMove)
    {

        if(velAtStart[direction] == 0)
        {
            velAtStart[direction] = directionMove * velocity;
        }
    
        if(inputState != 0 && directionMove == 1)
        {
            moveStates[direction] = MovementStates.Accelerating;
            velAtStart[direction] = 0f;
            
        }

        if(Mathf.Sign(directionMove) != Mathf.Sign(velAtStart[direction]))
        {
            moveStates[direction] = MovementStates.Stopped;
            velAtStart[direction] = 0f;
        }
        

        return -velAtStart[direction] / timeToStop;
    }

    void HandleMovement(int direction)
    {
        float inputState = 0;
        Vector3 movementDirection;

        Vector3 input = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        if(direction == 0)
        {
            inputState = Input.GetAxis("Horizontal"); 
            movementDirection = cam.transform.right;
        }
        else
        {
            inputState = Input.GetAxis("Vertical"); 
            movementDirection = cam.transform.forward;
        }

        if(inputState != 0)
        {
            inputState = Mathf.Sign(inputState);
        }

        Debug.Log(inputState);

        movementDirection.y = 0;

        Vector3 currentVel = Vector3.Project(input,movementDirection.normalized);
        


        float newAccel = 0;
        switch (moveStates[direction])
        {
            case MovementStates.Accelerating:
                newAccel = HandleAcceleration(direction,inputState);
                break;
            case MovementStates.Decelerating:
                float c_d_i = Vector3.Dot(currentVel.normalized, movementDirection.normalized);

                int directionMove = 1;

                if(c_d_i <= 0)
                {
                    directionMove = -1;
                }
                
                newAccel = HandleDeceleration(direction,inputState,currentVel.magnitude,directionMove);
                break;
            // stopping is default state
            default:
                HandleStopped(direction, inputState);
                break;

        }
        

        
        Vector3 clamped = Vector3.ClampMagnitude(rb.velocity + (newAccel * movementDirection.normalized * Time.deltaTime),maxSpeed);
        clamped.y = rb.velocity.y;
        rb.velocity = clamped;

        //rb.velocity
        //Vector3 movementDirection = (horizontalMovement * right + verticalMovement * forward).normalized;
        /*
        if(direction == 0)
        {
            rb.velocity = new Vector3(newVelocity, rb.velocity.y, rb.velocity.z);
        }
        else
        {
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, newVelocity);
        }

        Vector3 forward = cam.transform.forward;
        Vector3 right = cam.transform.right;

        // dont want movement in y dim
        forward.y = 0f; 
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        if(Mathf.Sign(rb.velocity.z) != verticalMovement && Mathf.Abs(rb.velocity.z) < 0.5)
        {

        }



        if(horizontalMovement == 0 && verticalMovement == 0)
        {
            return;
        }




        Vector3 newVelocity = rb.velocity + movementDirection * accel * Time.deltaTime;
        

        if(Mathf.Abs(rb.velocity.x) > maxSpeed)
        {
            newVelocity.x = Mathf.Sign(rb.velocity.x) * maxSpeed;
        }

        if(Mathf.Abs(rb.velocity.z) > maxSpeed)
        {
            newVelocity.z = Mathf.Sign(rb.velocity.z) * maxSpeed;
        }

        Debug.Log(movementDirection);


        PrepareForMovement();
        rb.velocity =  newVelocity;
        */

        PrepareForMovement();
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

    void HandleStep()
    {
        // used here https://www.youtube.com/watch?v=DrFk5Q_IwG0
        // as starting point 
        
        float [] adjustmentsX = {-0.55f,0, 0.55f};
        float [] adjustmentsZ = {-0.55f,0, 0.55f};
        float [] anglesAdjust = {-45,0,45};

        float rotationY = cam.transform.rotation.eulerAngles.y;

        Vector3 rot = lowerStep.rotation.eulerAngles;
        rot.y = rotationY;

        lowerStep.rotation = Quaternion.Euler(rot);
        upperStep.rotation = Quaternion.Euler(rot);
        
        for(int i = 0; i < adjustmentsX.Length; i++)
        {
            for(int j = 0; j < adjustmentsZ.Length; j++)
            {
                // only want movement in one axis
                if(adjustmentsX[i] == adjustmentsZ[j])
                {
                    continue;
                }

                Vector3 adjustment = new Vector3(adjustmentsX[i], 0,adjustmentsZ[j]);

                Vector3 forward = lowerStep.transform.forward; // or your vector you want to rotate
                
                //Vector3 forward = lowerStep.forward; // or your vector you want to rotate
            
                for(int k = 0; k < anglesAdjust.Length; k++)
                {

                    float angle = anglesAdjust[k];


                    Vector3 axis = Vector3.up; // y-axis

                    Quaternion rotation = Quaternion.AngleAxis(angle, axis);
                    Vector3 rotatedAngle = rotation * forward;

                    RaycastHit lowerContact;

                    if(!Physics.Raycast(lowerStep.transform.position + adjustment, rotatedAngle, out lowerContact, 0.2f))
                    {
                        continue;
                        // switch to continue
                    }

                    RaycastHit upperContact;
                    if(Physics.Raycast(upperStep.transform.position + adjustment, rotatedAngle, out upperContact, 0.2f))
                    {
                        continue;
                        // switch to continue
                        //
                    }

                    rb.position -= new Vector3(0f,-stepHeight,0);
                    return;
                }

            }
            
        }
        



    }

    void Update()
    {
        if(playerWorld.isPaused)
        {
            Cursor.lockState = CursorLockMode.None;
            return;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }


        HandleJump();
        
        HandleMovement(0);
        HandleMovement(1);

        HandleAngle();

        

    }
}
