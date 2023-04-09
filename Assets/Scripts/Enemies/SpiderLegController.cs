using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Threading.Tasks;
using System.Collections.Generic;

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

    public LegState state;

    public Vector3 legStuckPos;
    public float stuckDistance;


    public Leg(Transform moveParent)
    {
        bool hintFound = false;
        bool targetFound = false;

        movingParent = moveParent;

        targetTransform = null;
        hintTransform = null;

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
        
        state = LegState.Maintain;
        legStuckPos = GetParentFloorPos();;
        stuckDistance = 0;
    }

    public void UpdateParentInfo()
    {
        stuckDistance = Vector3.Distance(legStuckPos,GetParentFloorPos());
        Debug.Log(stuckDistance);

    }

    public void MaintainAction()
    {
        SetFootPos(legStuckPos);
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

    public async void AnimateMovement(Vector3 newPos, float time, float stepHeight)
    {
        float lerp = 0;

        while(lerp < time)
        {
            Vector3 currentPos = Vector3.Lerp(targetTransform.position, newPos, 1/time * lerp);
            currentPos.y += Mathf.Sign(1/time * lerp * Mathf.PI) * stepHeight; 

            SetFootPos(currentPos);

            lerp += Time.deltaTime;

            await Task.Yield();

        }

        legStuckPos = newPos;
        state = LegState.Maintain;

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
    [SerializeField] private float stepTime = 0.3f;

    private Dictionary<LegTypes,Leg> legs = new Dictionary<LegTypes, Leg>();



    void Start()
    {

        legs[LegTypes.FrontRight] = new Leg(frontRight);
        legs[LegTypes.FrontLeft] = new Leg(frontLeft);
        legs[LegTypes.BackRight] = new Leg(backRight);
        legs[LegTypes.BackLeft] = new Leg(backLeft);
        
    }




    void Update()
    {


        // wanted to use a max heap, but not built in
        // and dont want to implement my own

        List<Leg> sortedLegs  = new List<Leg>();

        bool anyMoving = false;

        foreach(KeyValuePair<LegTypes,Leg> kvp in legs)
        {
            Leg currentLeg = kvp.Value;

            currentLeg.UpdateParentInfo();

            if(currentLeg.state == LegState.Maintain)
            {
                currentLeg.MaintainAction();
            }
            else
            {
                anyMoving = true;
            }

            sortedLegs.Add(currentLeg);
        }

        // only want one leg moving
        if(anyMoving)
        {
            return;
        }
        

        sortedLegs.Sort((p1, p2) => p2.stuckDistance.CompareTo(p1.stuckDistance));

        Leg highestPriorityLeg = sortedLegs[0];

        if(highestPriorityLeg.stuckDistance > stepDistance && highestPriorityLeg.state != LegState.Moving)
        {
            highestPriorityLeg.AnimateMovement(highestPriorityLeg.GetParentFloorPos(), stepTime, stepHeight);
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
            Gizmos.DrawSphere(currentLeg.legStuckPos, .1f);
        }
    }

}

