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
    [SerializeField] int maxJumpCharges = 2;
    [SerializeField] bool isOnFloor = false;

    int jumpChargesRemaining;

    private Camera cam;
    private CharacterController con;
    private Vector3 velocityVector;
    private float startJump = 0;


    void Start()
    {
        cam = GetComponentInChildren<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
        con = GetComponent<CharacterController>();

        jumpChargesRemaining = maxJumpCharges;


        velocityVector = new Vector3(0,0,0);

        
    }

    void HandleAngle()
    {
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");


        Vector3 angle = cam.transform.rotation.eulerAngles;

        if(Mathf.Abs(mouseY) > 0)
        {
            
            float dAngle = -mouseY* yCamSmoothness;



            if(dAngle + angle.x > 330 || dAngle + angle.x < 30)
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

    }

    void HandleMovement()
    {
        float horizontalMovement = Input.GetAxisRaw("Horizontal");
        float verticalMovement = Input.GetAxisRaw("Vertical");

        if(horizontalMovement == 0 && verticalMovement == 0)
        {
            return;
        }

        Vector3 transformVel = Vector3.zero;

        if(Mathf.Abs(verticalMovement) > 0 )
        {
            Vector3 cameraForward = cam.transform.forward;
            Vector3 rbForwardTransformed = transform.InverseTransformDirection(cameraForward) * Mathf.Sign(verticalMovement);
            transformVel += rbForwardTransformed;

        }


        if(Mathf.Abs(horizontalMovement) > 0 )
        {
            Vector3 camRight = cam.transform.right;
            Vector3 rbRightTransformed = transform.InverseTransformDirection(camRight) * Mathf.Sign(horizontalMovement);
            transformVel += rbRightTransformed;
        }

        float currentSpeed = con.velocity.magnitude;

        float speed = Mathf.Min(currentSpeed + (Mathf.Pow(2f,accel)* Time.fixedDeltaTime), maxSpeed);
        
        con.Move((transformVel*speed*Time.deltaTime) );
    }

    async void HandleGravity()
    {


        Vector3 movementVector = new Vector3(0,0,0);
        movementVector.y = velocityVector.y * Time.deltaTime + .5f * Physics.gravity.y * Mathf.Pow(Time.deltaTime,2);

        con.Move(movementVector);

        if(!isOnFloor)
        {
            velocityVector.y += Physics.gravity.y * Time.deltaTime;
        }

        /*
        Vector3 prevPos = transform.position;
        Vector3 vel = Vector3.zero;
        float timeFalling = 0;

        while(true)
        {

            Vector3 gravityComp = new Vector3(0,0,0);

            //vel = (transform.position - prevPos) / Time.deltaTime;

            if(!isOnFloor)
            {
                timeFalling += Time.deltaTime;

                gravityComp = Mathf.Pow(2,timeFalling)* Physics.gravity;
                //gravityComp.y = -Mathf.Pow(2,gravityComp.y);
            }
            else
            {
                timeFalling = 0;
                await Task.Yield();
                continue;
                //vel = Vector3.zero;
            }

            //vel.y -= Mathf.Pow(2f,timeFalling*gravityComp.y);
            con.Move(gravityComp * Time.deltaTime);

            //prevPos = transform.position;
            await Task.Yield();

        }
        */

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
        //velocityVector.y += v0;
        velocityVector.y = v0;
        startJump = Time.time;

        


        /*
        float apexTime = Mathf.Sqrt((2f * jumpHeight) / Physics.gravity.y);
        float v0 = (jumpHeight / apexTime)  - (  (Physics.gravity.y * apexTime) / 2f);
        con.Move(
        //con.velocity += Vector3(0,v0,0);
        jumpChargesRemaining -= 1;
        */
    }


    /*
    async void PerformJump()
    {

        
        float currentTime = 0;


        Vector3 vel = Vector3.zero;
                /*
        while(currentTime < apexTime)
        {

            float dd = currentTime * v0 + 0.5f * Physics.gravity.y * Mathf.Pow(currentTime,2f);

            
            Vector3 movementVector = new Vector3(0,dd - currentHeight, 0);

            con.Move(movementVector);

            currentTime += Time.deltaTime;

            currentHeight = dd;
            await Task.Yield();
        }

        Debug.Log(transform.position.y - startHeight);


    }

    */

    void FixedUpdate()
    {
        isOnFloor = Physics.Raycast(transform.position, -Vector3.up, con.bounds.extents.y + 0.05f);
        HandleGravity();
    }

    void Update()
    {

        HandleJump();
        HandleMovement();
        HandleAngle();
    }
}
