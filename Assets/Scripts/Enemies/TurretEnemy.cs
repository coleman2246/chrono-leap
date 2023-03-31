using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurretRotState
{
    RotatingCCWX,
    RotatingCWX,
    RotToEnemy,
    AttackEnemy
}

public class TurretEnemy : BaseEnemy
{

    [SerializeField] private float maxYRotation = 180f;
    [SerializeField] private float maxXRotation = 30f;
    [SerializeField] private float rpm = 2;
    [SerializeField] private float attackRpm = 4;
    [SerializeField] private float angleTolerance = 2f;
    [SerializeField] private Vector3 targetAngle;
    [SerializeField] private TurretRotState rotState = TurretRotState.RotatingCCWX;

    private Dictionary<TurretRotState, Quaternion> anglesLookup = new Dictionary<TurretRotState, Quaternion>();

    private float rpmToDegSec = 6f;
    LinkedList<object> turretStateTracker = new LinkedList<object>();

    public override void EnemyStartCallback()
    {
        Quaternion CCWTarget = Quaternion.Euler(0, -maxYRotation / 2f, 0) * headTransform.rotation;
        Quaternion CWTarget = Quaternion.Euler(0, maxYRotation / 2f, 0) * headTransform.rotation;

        anglesLookup[TurretRotState.RotatingCCWX] = CCWTarget;
        anglesLookup[TurretRotState.RotatingCWX] = CWTarget;

        RegisterTracker(ref turretStateTracker);
    }

    public override void EnemyTimeStepCallback()
    {
        turretStateTracker.AddLast((object)rotState);
    }

    
    public void RotationStep(float targetRpm)
    {
        Quaternion rotationToApply = Quaternion.RotateTowards(headTransform.rotation, anglesLookup[rotState], targetRpm * rpmToDegSec * Time.deltaTime);

        headTransform.rotation = rotationToApply;
    }

    public bool isAtTarget()
    {
        return Quaternion.Angle(headTransform.rotation, anglesLookup[rotState]) < angleTolerance;
    }

    public void ChangeRotState()
    {
        if (rotState == TurretRotState.RotatingCWX)
        {
            rotState = TurretRotState.RotatingCCWX;
        }
        else if (rotState == TurretRotState.RotatingCCWX)
        {
            rotState = TurretRotState.RotatingCWX;
        }
        else
        {
            // do something later
            return;
        }
    }

    public override void PatrolCallback()
    {
        if(timeInState == 0)
        {
            rotState = TurretRotState.RotatingCCWX;
        }

        if (isAtTarget())
        {
            ChangeRotState();
        }

        RotationStep(rpm);
    }


    public override void TriggeredCallback()
    {
        if(timeInState == 0)
        {
            Quaternion target = Quaternion.FromToRotation(headTransform.forward, directionToPlayer);
            Debug.Log(target);

            rotState = TurretRotState.RotToEnemy;
            anglesLookup[rotState] = target;
        }

        if(isAtTarget())
        {
            Debug.Log("Pointed toward player");
        }

        RotationStep(attackRpm);

    }


    public override void EnemyRewindCallback()
    {
        
        Debug.Log(turretStateTracker.Count);
        if(turretStateTracker.Last != null)
        {
            TurretRotState newState = (TurretRotState) turretStateTracker.Last.Value;
            rotState = newState;
            
        }
    }

}
