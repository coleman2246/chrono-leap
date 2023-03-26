using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MovingPlatformState
{
    MovingToEnd,
    MovingToStart
}

struct MovingPlatformTracking
{
    public MovingPlatformState movingState;
    public float timeInState;

    public MovingPlatformTracking(MovingPlatformState state, float time)
    {
        movingState = state;
        timeInState = time;
    }


}

public class MovingPlatform : TimeEffectedObject
{

    [SerializeField] private Transform endTransform;
    [SerializeField] private float periodTime = 1f;
    [SerializeField] private float acceptableDistance = 0.1f;


    private MovingPlatformState movingState = MovingPlatformState.MovingToEnd;
    private LinkedList<object> platformStateTracker = new LinkedList<object>();

    private Vector3 startVector;
    private Vector3 endVector;
    private float timeInState = 0;

    public override void PauseableStart()
    {
        startVector = transform.position;
        RegisterTracker(ref platformStateTracker);

    }

    public override void TimeStepCallback()
    {
        MovingPlatformTracking currentState = new MovingPlatformTracking(movingState,timeInState);
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

        if(movingState == MovingPlatformState.MovingToEnd)
        {
            targetVector = endTransform.position;
        }
        else if(movingState == MovingPlatformState.MovingToStart)
        {
            targetVector = startVector;
        }

        return transform.position - targetVector;
    }

    void InvertMovingState()
    {

        if(movingState == MovingPlatformState.MovingToEnd)
        {
            movingState = MovingPlatformState.MovingToStart;
        }
        else if(movingState == MovingPlatformState.MovingToStart)
        {
            movingState = MovingPlatformState.MovingToEnd;
        }

    }



    public override void PauseableUpdate()
    {

        Vector3 distanceVector = GetTargetVector();

        if(distanceVector.magnitude < acceptableDistance)
        {
            InvertMovingState();
            timeInState = 0;
            return;
        }



        float timeRemaining = periodTime - timeInState;
        if(timeRemaining < 0)
        {
            timeRemaining = 0.1f;
        }

        Vector3 requiredVelocity = -distanceVector / timeRemaining;
        rb.velocity = requiredVelocity;



        timeInState += Time.deltaTime;

    }

    public override void RewindEndCallback()
    {
        MovingPlatformTracking newState = (MovingPlatformTracking) platformStateTracker.Last.Value;
        movingState = newState.movingState;
        timeInState = newState.timeInState;
    }

    
}
