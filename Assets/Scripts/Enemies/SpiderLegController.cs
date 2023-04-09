using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public enum LegTypes
{
    FrontLeft,
    FrontRight,
    BackLeft,
    BackRight
}

public enum LegState
{
    Moving,
    Maintain
}


public class Leg
{
    public Transform hintTransform;
    public Transform targetTransform;
    public Transform movingParent;

    public Vector3 footTarget;
    public Vector3 prevMovingParentPos;

    public LegState state;
    public float timeMoving;
    public Vector3 directionOfMovement;
    public Vector3 parentVel;
    public float lerp;
    public float targetToParentDist;

    public Vector3 initialOffset;
    public bool doneAnim;


    public float stepHeight;
    public float stepDistance;

    public float currentAcceptableDistance;
    public LegTypes type;
    public float forwardLegAngle;
    public float rightLegAngle;
    public Vector3 extendDirForward;
    public Vector3 extendDirRight;
    public float speed;

    public Leg(Transform moveParent, LegTypes legType, float stepHeight, float stepDistance, int isFront, int isRight, float speed)
    {
        bool hintFound = false;
        bool targetFound = false;

        targetTransform = null;
        hintTransform = null;


        type = legType;

        stepHeight = stepHeight;
        stepDistance = stepDistance;
        speed = speed;

        foreach (Transform child in moveParent)
        {

            if (child.name.Contains("hint"))
            {
                hintTransform = child;
                hintFound = true;
            }

            if (child.name.Contains("target"))
            {
                targetTransform = child;
                targetFound = true;
            }

            if(targetFound && hintFound)
            {
                break;
            }
        }

        movingParent = moveParent;
        footTarget = movingParent.position;
        prevMovingParentPos = movingParent.position;
        state = LegState.Maintain;
        timeMoving = 0;
        directionOfMovement = new Vector3(0,0,0);
        parentVel = new Vector3(0,0,0);
        lerp = 0;


        initialOffset = hintTransform.position - movingParent.position;
        targetToParentDist = 0;
        doneAnim = false;
        currentAcceptableDistance = stepDistance;
        forwardLegAngle = 90;

        extendDirForward = isFront * targetTransform.forward;
        extendDirRight = isRight * targetTransform.right;

    }

    public void UpdateParentInfo()
    {
        directionOfMovement = (prevMovingParentPos - movingParent.position + initialOffset).normalized;
        parentVel = (prevMovingParentPos - movingParent.position + initialOffset)/Time.deltaTime;

        prevMovingParentPos = movingParent.position;

        targetToParentDist = Vector3.Distance(GetParentFloorPos(), targetTransform.position);

        forwardLegAngle = Vector3.Angle(targetTransform.position-movingParent.position, extendDirForward);
        rightLegAngle = Vector3.Angle(targetTransform.position-movingParent.position, extendDirRight);
            

        //Debug.Log($"forwrad {type.ToString()} {forwardLegAngle}");
        //Debug.Log($"right {type.ToString()} {rightLegAngle}");

    }

    public void SetFootPos(Vector3 pos)
    {
        hintTransform.position = pos;
        targetTransform.position = pos;
    }

    public Vector3 GetParentFloorPos()
    {
        Vector3 pos = default(Vector3);

        Ray ray = new Ray(movingParent.position, Vector3.down);
        Debug.DrawRay(ray.origin,ray.direction, Color.red);

        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, 2))
        {
            pos =  hit.point;
            pos.y += .04f;
        }

        return pos;
    }

    public void PerformAction()
    {
        if(state == LegState.Maintain)
        {
            SetFootPos(footTarget);
        }
        else
        {
            AnimateMovement(footTarget);

        }

        UpdateParentInfo();

    }

    void AnimateMovement(Vector3 newPos)
    {

        if(lerp < 1f)
        {
            Vector3 currentPos = Vector3.Lerp(targetTransform.position, newPos, lerp);
            currentPos.y += Mathf.Sign(lerp * Mathf.PI) * stepHeight; 

            SetFootPos(currentPos);
            // basically integrating to get displacement

            //float speed = parentVel.magnitude; //4 legs each should be moving at 1/4

            lerp += Time.deltaTime * 6;

            /*
            if(Vector3.Distance(transform.parent.position, currentPos) > stepDistance)
            {
                Debug.Log("exti early");
                //break;
            }
            */

            timeMoving += Time.deltaTime;
        }
        else
        {
            doneAnim = true;
        }

    }

}

public class SpiderLegController : MonoBehaviour
{
    [SerializeField] private Transform frontLeft;
    [SerializeField] private Transform frontRight;
    [SerializeField] private Transform backLeft;
    [SerializeField] private Transform backRight;

    [SerializeField] private float stepHeight = 0.2f;
    [SerializeField] private float stepDistance = 0.3f;

    private Dictionary<LegTypes,Leg> legs = new Dictionary<LegTypes, Leg>();
    private Dictionary<LegTypes,LegTypes[]> antagonistLegs = new Dictionary<LegTypes, LegTypes[]>();
    NavMeshAgent agent;



    void Start()
    {
        agent = transform.parent.GetComponent<NavMeshAgent>();

        legs[LegTypes.FrontRight] = new Leg(frontRight,LegTypes.FrontRight, stepHeight, stepDistance, 1,1,agent.speed);
        legs[LegTypes.FrontLeft] = new Leg(frontLeft,LegTypes.FrontLeft, stepHeight, stepDistance, 1,-1, agent.speed);
        legs[LegTypes.BackRight] = new Leg(backRight,LegTypes.BackRight, stepHeight, stepDistance,-1,1, agent.speed);
        legs[LegTypes.BackLeft] = new Leg(backLeft,LegTypes.BackLeft, stepHeight, stepDistance, -1,-1, agent.speed);

        LegTypes[] frontRightAnatagonists = {LegTypes.BackRight, LegTypes.FrontLeft};
        LegTypes[] frontLeftAnatagonists = {LegTypes.BackLeft, LegTypes.FrontRight};
        LegTypes[] backRightAnatagonists = {LegTypes.FrontRight, LegTypes.BackLeft};
        LegTypes[] backLeftAnatagonists = {LegTypes.FrontLeft, LegTypes.BackRight};

        antagonistLegs[LegTypes.FrontRight] = frontRightAnatagonists;
        antagonistLegs[LegTypes.FrontLeft] = frontLeftAnatagonists;
        antagonistLegs[LegTypes.BackRight] = backRightAnatagonists;
        antagonistLegs[LegTypes.BackLeft] = backLeftAnatagonists;
        
    }


    void Update()
    {
        //Debug.Log(legs[LegTypes.FrontRight].parentVel);
        //Leg currentLeg = legs[LegTypes.FrontRight];

        foreach(KeyValuePair<LegTypes,Leg> kvp in legs)
        {
            Leg currentLeg = kvp.Value;
            LegTypes key = kvp.Key;

            if(key != LegTypes.BackRight && key != LegTypes.FrontRight)
            {

                //continue;
            }
            
            if(currentLeg.doneAnim)
            {

                currentLeg.doneAnim = false; 
                currentLeg.state = LegState.Maintain;
                currentLeg.timeMoving = 0;
            }


            bool badLegAngleForward = currentLeg.forwardLegAngle < 30;
            bool badLegAngleRight = currentLeg.rightLegAngle > 100;
            bool tooFarAway = currentLeg.targetToParentDist > stepDistance;

            //Debug.Log(currentLeg.doneAnim);

            if( (tooFarAway || badLegAngleForward || badLegAngleRight) && currentLeg.state != LegState.Moving)
            {
                
                bool moveLeg = true;

                foreach(LegTypes antagonistLegKey in antagonistLegs[key])
                {
                    Leg antLeg = legs[antagonistLegKey];

                    //Debug.Log(antLeg.timeMoving);

                    if(antLeg.state == LegState.Moving && antLeg.lerp < 0.1)
                    {
                        moveLeg = false;
                        break;
                    }

                }

                if(moveLeg)
                {
                    currentLeg.state = LegState.Moving;
                    currentLeg.lerp = 0;

                    currentLeg.timeMoving = 0;
                    Vector3 newFootTarget = currentLeg.GetParentFloorPos();

                    newFootTarget += -currentLeg.extendDirForward *3* currentLeg.stepDistance;
                    if(badLegAngleForward)
                    {
                        newFootTarget += -currentLeg.extendDirForward *30* currentLeg.stepDistance;
                    }
                    
                    if(badLegAngleRight)
                    {
                        newFootTarget += -currentLeg.extendDirRight * 30*currentLeg.stepDistance;
                    }

                    currentLeg.footTarget =  newFootTarget;
                }

            }

            

            

            currentLeg.PerformAction();

        }
                
    }


    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position

        foreach(KeyValuePair<LegTypes,Leg> kvp in legs)
        {
            Leg currentLeg = kvp.Value;
            LegTypes key = kvp.Key;


            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(currentLeg.footTarget, .05f);
        }
    }

}
