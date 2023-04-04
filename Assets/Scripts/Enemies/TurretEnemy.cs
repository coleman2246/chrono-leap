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
    [SerializeField] private float triggerRpm = 3;
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

        // dont want any rotation in z dimension
        // makes the head look gooofy
        Vector3 eulerRot = rotationToApply.eulerAngles;
        eulerRot.z = headTransform.rotation.eulerAngles.z; 

        headTransform.rotation = Quaternion.Euler(eulerRot);

    }

    public bool isAtTarget(float customTol = default(float))
    {
        float tol = angleTolerance;

        if(customTol != default(float))
         {
             tol = customTol;
         }

        return Quaternion.Angle(headTransform.rotation, anglesLookup[rotState]) < tol;
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


    public override void TriggeredCallback(bool inFov)
    {
        Quaternion target = GetQuaternionToPlayer();

        rotState = TurretRotState.RotToEnemy;
        anglesLookup[rotState] = target;

        if(isAtTarget() && inFov)
        {
            TransitionToNextState();
        }
        else
        {

            RotationStep(triggerRpm);
        }


    }

    public override void AttackingCallback(bool inFov)
    {
        if(timeInState == 0)
        {
            rotState = TurretRotState.AttackEnemy;
        }

        float targetError = 0.5f;

        if(timeSinceLastAttack  > maxAttackInterval && isAtTarget(targetError) && inFov)
        {
            Vector3 endPoint = bulletSpawnLocation.position + headTransform.forward * distance;         
            FireBullet(endPoint);
        }
        else
        {
            Quaternion target = GetQuaternionToPlayer();
            anglesLookup[rotState] = target;

            timeSinceLastAttack += Time.deltaTime;
            
            if(!isAtTarget(targetError))
            {
                RotationStep(attackRpm);
            }
        }
        
    }


    public override void EnemyRewindCallback()
    {
        
        if(turretStateTracker.Last != null)
        {
            TurretRotState newState = (TurretRotState) turretStateTracker.Last.Value;
            rotState = newState;
            
        }
    }

}
