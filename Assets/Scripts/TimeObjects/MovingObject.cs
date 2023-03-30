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
    public bool oneWay = false;
    public bool activateOnTrigger = false;
 
    [SerializeField] private Transform endTransform;
    [SerializeField] private float periodTime = 1f;
    [SerializeField] private float acceptableDistance = 0.1f;
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


        velocityLookOrig[MovingObjectState.MovingToEnd] = -1f * forwardVel;
        velocityLookOrig[MovingObjectState.MovingToStart] = forwardVel;

        unitVector[MovingObjectState.MovingToEnd] = (transform.position - endTransform.position).normalized;
        unitVector[MovingObjectState.MovingToStart] = (endTransform.position - transform.position).normalized;



        posLookUp[MovingObjectState.MovingToStart] = transform.position;
        posLookUp[MovingObjectState.MovingToEnd] = endTransform.position;

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

        /*
        Debug.Log($"{distanceVector.normalized} {unitVector[movingState]} {transform.parent.name}");
        */

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


    void OnTriggerStay(Collider collision)
    {

        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody playerRb = collision.transform.GetComponent<Rigidbody>();
            PlayerController player = collision.transform.GetComponent<PlayerController>();

            if( Time.time - player.moveTime > 0.1f)
            {
                 playerRb.constraints |= RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;

                playerRb.isKinematic = true;
            }

            rb.mass = rb.mass - playerRb.mass;
            collision.transform.SetParent(transform);
        }

    }


    void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);

            PlayerController player = collision.transform.GetComponent<PlayerController>();
            Rigidbody playerRb = collision.transform.GetComponent<Rigidbody>();
            rb.mass = rb.mass + playerRb.mass;
            playerRb.isKinematic = false;
            playerRb.constraints = player.startingConstraints; 
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        MovingObject other = collision.gameObject.GetComponent<MovingObject>();
        Rigidbody otherRb = collision.gameObject.GetComponent<Rigidbody>();


        if ( otherRb == null || other == null)
        {
            return;
        }


         foreach (ContactPoint contact in collision.contacts)
        {

            Vector3 velocity = otherRb.velocity;
            Vector3 normal = contact.normal;

            Debug.Log(Vector3.Dot(velocity, normal));

            // if there is a collision the angle between the
            // normal and the 
            if (Vector3.Dot(velocity, normal) > 0)
            {
                other.InvertMovingState();
                other.SetRequiredVelocity();
            }
            break;
        }
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
    
}
