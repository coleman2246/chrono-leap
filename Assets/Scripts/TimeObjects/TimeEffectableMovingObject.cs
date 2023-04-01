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

public class TimeEffectableMovingObject : TimeEffectedObject
{
    public bool oneWay = false;
    public bool activateOnTrigger = false;
    public bool useSpeed = false;
    public float speed = 1f;

    [SerializeField] private Vector3 endPosition = default(Vector3); 
    [SerializeField] private float periodTime = 1f;
    [SerializeField] private float acceptableDistance = 0.1f;
    [SerializeField] public bool triggered = false;

    private MovingObjectState movingState = MovingObjectState.MovingToEnd;
    private LinkedList<object> platformStateTracker = new LinkedList<object>();


    private Vector3 velocity;
    private bool nonRigid;
    private bool setVel = false;
    private float timeInState = 0;
    private Vector3 requiredVelocity; 
    private bool prevTrigger = false;
    private bool endPositionSet = false;

    private Dictionary<MovingObjectState, Vector3> velocityLookUp = new Dictionary<MovingObjectState, Vector3>()
    {
        {MovingObjectState.MovingToEnd, new Vector3(0,0,0)},
        {MovingObjectState.MovingToStart, new Vector3(0,0,0)}
    };

    private Dictionary<MovingObjectState, Vector3> velocityLookOrig = new Dictionary<MovingObjectState, Vector3>()
    {
        {MovingObjectState.MovingToEnd, new Vector3(0,0,0)},
        {MovingObjectState.MovingToStart, new Vector3(0,0,0)}
    };



    private Dictionary<MovingObjectState, Vector3> posLookUp = new Dictionary<MovingObjectState, Vector3>()
    {
        {MovingObjectState.MovingToStart, new Vector3(0,0,0)},
        {MovingObjectState.MovingToEnd, new Vector3(0,0,0)}
    };


    private Dictionary<MovingObjectState, Vector3> unitVector = new Dictionary<MovingObjectState, Vector3>()
    {
        {MovingObjectState.MovingToStart, new Vector3(0,0,0)},
        {MovingObjectState.MovingToEnd, new Vector3(0,0,0)}
    };

    public void TriggerMovement()
    {
        triggered = true;
    }

    public Vector3 GetTargetDistance()
    {
        return transform.position - posLookUp[movingState];
    }

    public override void PauseableStart()
    {
        RegisterTracker(ref platformStateTracker);
        nonRigid = rb == null;

        if(nonRigid)
        {
            velocity = new Vector3(0,0,0);
        }

        if(endPosition != default(Vector3))
        {
            endPositionSet = true;
            PopulateLookups();

            SetRequiredVelocity();

        }

        MoveableObjectStart();
    }

    public void SetEndPos(Vector3 newVector)
    {
        endPosition = newVector;
        PopulateLookups();
        SetRequiredVelocity();
        endPositionSet = true;
    }

    public override void TimeStepCallback()
    {
        MovingObjectTracking currentState = new MovingObjectTracking(movingState,timeInState);
        platformStateTracker.AddLast((object)currentState);
    }

    void PopulateLookups()
    {
        Vector3 forwardVel = new Vector3();

        unitVector[MovingObjectState.MovingToEnd] = (transform.position - endPosition).normalized;
        unitVector[MovingObjectState.MovingToStart] = (endPosition - transform.position).normalized;

        posLookUp[MovingObjectState.MovingToStart] = transform.position;
        posLookUp[MovingObjectState.MovingToEnd] = endPosition;


        if(useSpeed)
        {

            forwardVel = unitVector[MovingObjectState.MovingToEnd]  * speed;
        }
        else
        {
            forwardVel = (transform.position - endPosition) / periodTime;

                        
            
        }
        
        velocityLookUp[MovingObjectState.MovingToEnd] = -1f * forwardVel;
        velocityLookUp[MovingObjectState.MovingToStart] = forwardVel;


        velocityLookOrig[MovingObjectState.MovingToEnd] = -1f * forwardVel;
        velocityLookOrig[MovingObjectState.MovingToStart] = forwardVel;


    }

   
    public void InvertMovingState()
    {

        if(movingState == MovingObjectState.MovingToEnd)
        {
            movingState = MovingObjectState.MovingToStart;

        }
        else if(movingState == MovingObjectState.MovingToStart)
        {
            movingState = MovingObjectState.MovingToEnd;
        }


        velocityLookUp[MovingObjectState.MovingToStart] = velocityLookOrig[MovingObjectState.MovingToStart]; 

        velocityLookUp[MovingObjectState.MovingToEnd] = velocityLookOrig[MovingObjectState.MovingToEnd]; 

        
        //velocityLookUp[movingState] = -1f * (transform.position - posLookUp[movingState]) / periodTime;



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
        if(rb == null)
        {

            StepVelocity();
        }
    }

    public void SetRequiredVelocity()
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

    public void RecalculateVel()
    {
        float time = periodTime - timeInState;

        if(time <= 0)
        {
            time = periodTime;
        }

        velocityLookUp[movingState] = -(transform.position - posLookUp[movingState]) / (time);

        SetRequiredVelocity();


    }

    public void EnsureOnTrack()
    {
        Vector3 distanceVector = GetTargetDistance();

        // off track by more than 10cm
        if(Vector3.Distance(distanceVector.normalized, unitVector[movingState]) > 0.1)
        {
            RecalculateVel();
        }
    }



    public override void PauseableUpdate()
    {
        if(!endPositionSet)
        {
            return;
        }

        if(prevTrigger != triggered)
        {
            prevTrigger = triggered;
            SetRequiredVelocity();
        }

        Vector3 distanceVector = transform.position - posLookUp[movingState];


        if(distanceVector.magnitude < acceptableDistance)
        {
            AtTargetCallback();

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

        EnsureOnTrack();

        SetVelocity(requiredVelocity);
        timeInState += Time.deltaTime;
    }

    public override void RewindEndCallback()
    {
        if(platformStateTracker.Last != null)
        {
            MovingObjectTracking newState = (MovingObjectTracking) platformStateTracker.Last.Value;
            movingState = newState.movingState;

        }
        SetRequiredVelocity();
    }

    void UnPauseCallback()
    {
        SetRequiredVelocity();
    }


    void OnDrawGizmosSelected()
    {
        // Draw a semitransparent red cube at the transforms position
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(posLookUp[movingState], new Vector3(1, 1, 1));
    }

    public virtual void AtTargetCallback(){}
    public virtual void MoveableObjectStart(){}
    
}
