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

        SetRequiredVelocity();

    }

    public override void TimeStepCallback()
    {
        MovingObjectTracking currentState = new MovingObjectTracking(movingState,timeInState);
        platformStateTracker.AddLast((object)currentState);
    }

    Vector3 CalculateDistanceVector()
    {
        Vector3 currentPos = transform.position;
        Vector3 targetVector = GetTargetVector();
        return transform.position - targetVector;
    }

    Vector3 GetTargetVector()
    {
        Vector3 targetVector = new Vector3(0,0,0);

        if(movingState == MovingObjectState.MovingToEnd)
        {
            targetVector = endTransform.position;

        }
        else if(movingState == MovingObjectState.MovingToStart)
        {
            targetVector = startVector;
        }

        return targetVector;
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
                Vector3 distanceVector = CalculateDistanceVector();
                requiredVelocity = -distanceVector / periodTime;
            }
            else
            {
                requiredVelocity = new Vector3(0,0,0);
            }

        }
        else
        {
            Vector3 distanceVector = CalculateDistanceVector();
            requiredVelocity = -distanceVector / periodTime;
        }
    }



    public override void PauseableUpdate()
    {
        if(prevTrigger != triggered)
        {
            prevTrigger = triggered;
            SetRequiredVelocity();
        }

        Vector3 distanceVector = CalculateDistanceVector();

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

        /*
        if(activateOnTrigger)
        {
            if(!triggered)
            {
                SetVelocity(Vector3.zero);
                setVel = false;
                return;
            }
            else if(triggered && !setVel)
            {

                SetRequiredVelocity();
            }
        }

        SetVelocity(requiredVelocity);

        if(!setVel)
        {
            SetRequiredVelocity();
        }

        if(setVel)
        {
            if (GetVelocity().magnitude < .1)
            {
                SetRequiredVelocity();
            }

        }

        Vector3 distanceVector = CalculateDistanceVector();

        if(distanceVector.magnitude < acceptableDistance)
        {
            if(!oneWay)
            {
                InvertMovingState();
                setVel = false;
                timeInState = 0;
            }
            else
            {
                ClearVelocity();
                setVel = false;
            }

            triggered = false;
        }
        */
    }

    public override void RewindEndCallback()
    {
        MovingObjectTracking newState = (MovingObjectTracking) platformStateTracker.Last.Value;
        movingState = newState.movingState;
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
        float copy = periodTime;
        periodTime -= timeInState;
        SetRequiredVelocity();
        periodTime = copy;
        
        
    }
    
}
