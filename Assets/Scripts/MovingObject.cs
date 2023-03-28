using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MovingObjectState
{
    MovingToEnd,
    MovingToStart
}

struct MovingObjectTracking
{
    public MovingObjectState movingState;

    public MovingObjectTracking(MovingObjectState state, float time)
    {
        movingState = state;
    }
}

public class MovingObject : TimeEffectedObject
{

    [SerializeField] private Transform endTransform;
    [SerializeField] private float periodTime = 1f;
    [SerializeField] private float acceptableDistance = 0.1f;
    [SerializeField] private bool activateOnTrigger = false;
    [SerializeField] private bool oneWay = false;
    [SerializeField] public bool triggered = false;
    [SerializeField] private Vector3 velocity;

    private MovingObjectState movingState = MovingObjectState.MovingToEnd;
    private LinkedList<object> platformStateTracker = new LinkedList<object>();

    private Vector3 startVector;
    private Vector3 endVector;

    private bool nonRigid;
    private bool setVel = false;
    private float timeInState = 0;
    private Vector3 requiredVelocity; 
    private bool prevTrigger = false;
    private Dictionary<MovingObjectState, Vector3> velocityLookUp = new Dictionary<MovingObjectState, Vector3>();
    private Dictionary<MovingObjectState, Vector3> posLookUp = new Dictionary<MovingObjectState, Vector3>();

    public void TriggerMovement()
    {
        triggered = true;
    }

    public override void PauseableStart()
    {
        startVector = transform.position;
        RegisterTracker(ref platformStateTracker);
        nonRigid = rb == null;

        if(nonRigid)
        {
            velocity = new Vector3(0,0,0);
        }

        PopulateLookups();

        SetRequiredVelocity();

    }

    public override void TimeStepCallback()
    {
        MovingObjectTracking currentState = new MovingObjectTracking(movingState,timeInState);
        platformStateTracker.AddLast((object)currentState);
    }

    void PopulateLookups()
    {
        Vector3 forwardVel = new Vector3();

        forwardVel = (transform.position - endTransform.position) / periodTime;

        velocityLookUp[MovingObjectState.MovingToEnd] = -1f * forwardVel;
        velocityLookUp[MovingObjectState.MovingToStart] = forwardVel;

        posLookUp[MovingObjectState.MovingToStart] = transform.position;
        posLookUp[MovingObjectState.MovingToEnd] = endTransform.position;

    }

   
    void InvertMovingState()
    {

        if(movingState == MovingObjectState.MovingToEnd)
        {
            movingState = MovingObjectState.MovingToStart;

        }
        else if(movingState == MovingObjectState.MovingToStart)
        {
            movingState = MovingObjectState.MovingToEnd;
        }
        timeInState = 0;
    }

    void SetVelocity(Vector3 vec)
    {
        if(nonRigid)
        {
            velocity = vec;

        }
        else
        {
            rb.velocity = vec;
        }
    }

    void ClearVelocity()
    {
        if(nonRigid)
        {
            velocity = Vector3.zero;

        }
        else
        {
            rb.velocity = Vector3.zero;
        }
    }

    public Vector3 GetVelocity()
    {
        if(nonRigid)
        {
            return velocity;
        }
        else
        {
            return rb.velocity;
        }
    }

    void StepVelocity()
    {
        transform.position += velocity * Time.deltaTime;
    }

    public override void PauseableFixedUpdate()
    {
        StepVelocity();
    }

    void SetRequiredVelocity()
    {
        if(activateOnTrigger)
        {
            if(triggered)
            {
                requiredVelocity = velocityLookUp[movingState];
            }
            else
            {
                requiredVelocity = new Vector3(0,0,0);
            }

        }
        else
        {

            requiredVelocity = velocityLookUp[movingState];
        }

    }



    public override void PauseableUpdate()
    {
        if(prevTrigger != triggered)
        {
            prevTrigger = triggered;
            SetRequiredVelocity();
        }

        Vector3 distanceVector = transform.position - posLookUp[movingState];

        if(distanceVector.magnitude < acceptableDistance)
        {
            if(!oneWay)
            {
                InvertMovingState();
                SetRequiredVelocity();
            }
            else
            {
                requiredVelocity = new Vector3(0,0,0);
                ClearVelocity();
            }

            if(triggered)
            {
                triggered = false;
            }

        }

        SetVelocity(requiredVelocity);
        timeInState += Time.deltaTime;
    }

    public override void RewindEndCallback()
    {
        MovingObjectTracking newState = (MovingObjectTracking) platformStateTracker.Last.Value;
        movingState = newState.movingState;
        SetRequiredVelocity();
    }


    void OnTriggerStay(Collider collision)
    {

        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }

    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Rigidbody>() && collision.gameObject.GetComponent<TimeEffectedObject>())
        {
            InvertMovingState();
            SetRequiredVelocity();
        }
    }

    void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }

    void UnPauseCallback()
    {
        SetRequiredVelocity();
    }
    
}
