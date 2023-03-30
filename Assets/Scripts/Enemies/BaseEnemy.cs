using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyState
{
    Patrol,
    Triggered,
    Attacking,
    Dead
}

public struct EnemyStateTracking
{
    EnemyState state;
    float timeInState;
    float timeSincePlayerSeen;
    float timeSinceLastAttack;

    public EnemyStateTracking(EnemyState state, float timeInState, 
            float timeSincePlayerSeen, float timeSinceLastAttack)
    {
        this.state = state;
        this.timeInState = timeInState;
        this.timeSincePlayerSeen = timeSincePlayerSeen;
        this.timeSinceLastAttack = timeSinceLastAttack;
    }

}

public class BaseEnemy : TimeEffectedObject
{
    [SerializeField] private Transform headTransform;
    [SerializeField] private int fov = 135;
    [SerializeField] private int distance = 25;
    [SerializeField] private float forgetTime = 5f;
    [SerializeField] private float maxAttackInterval = .5f;
    [SerializeField] private EnemyState state = EnemyState.Patrol;

    private Transform player;
    private float timeSincePlayerSeen = 0f;
    private LinkedList<object> baseEnemyStateTracker = new LinkedList<object>();
    private Dictionary<EnemyState, Action<bool>> stateFunctionMap = new Dictionary<EnemyState, Action<bool>>();
    protected float timeSinceLastAttack = 0f;
    protected float timeInState = 0f;

    public override void PauseableStart()
    {

        stateFunctionMap[EnemyState.Patrol] = Patrol;
        stateFunctionMap[EnemyState.Triggered] =  Triggered;
        stateFunctionMap[EnemyState.Attacking] =  Attacking;
        stateFunctionMap[EnemyState.Dead] =  Dead;
        player = GameObject.FindGameObjectWithTag("Player").transform;


        RegisterTracker(ref baseEnemyStateTracker);

    }

    public override void TimeStepCallback()
    {
        EnemyStateTracking currentState = new EnemyStateTracking(
                state,
                timeInState,
                timeSincePlayerSeen,
                timeSinceLastAttack
        );
        baseEnemyStateTracker.AddLast((object)currentState);
    }


    public void Patrol(bool playerInFov)
    {
        if(playerInFov)
        {
            TransitionToNextState();
        }
        else
        {
            PatrolCallback();
        }
        
        
    }

    // this class can only be transitioned out of 
    // from the child class
    public void Triggered(bool playerInFov)
    {
        if(timeSincePlayerSeen > forgetTime)
        {
            TransitionToPrevState();
            return;
        }

        if(!playerInFov)
        {
            return;
        }

        TriggeredCallback();

    }

    public void Attacking(bool playerInFov)
    {
        if(timeSincePlayerSeen > forgetTime)
        {
            SwitchState(EnemyState.Patrol);
            return;
        }

        if(!playerInFov)
        {
            return;
        }
        
    }

    // this does nothing at the present time
    // as there is no way to transition out of this
    public void Dead(bool playerInFov)
    {
        DeadCallback();
    }


    public void KillEnemy()
    {
        SwitchState(EnemyState.Dead);
    }

       
    protected bool PlayerInFOV()
    {
        Vector3 directionToPlayer = player.position - headTransform.position ;
        float angleToPlayer = Vector3.Angle(headTransform.forward, directionToPlayer);

        
        //Debug.Log(angleToPlayer);
        if(angleToPlayer> fov/2f)
        {
            return false;
        }


        float angleToPlayerVertical = Vector3.Angle(headTransform.up, directionToPlayer);
        if(angleToPlayerVertical- 90 < -fov/2f || angleToPlayerVertical - 90 > fov/2f)
        {
            return false;
        }

        if(directionToPlayer.magnitude > distance)
        {
            return false;
        }

        RaycastHit hit;
        if(Physics.Raycast(headTransform.position, directionToPlayer.normalized * directionToPlayer.magnitude, out hit, distance, LayerMask.NameToLayer("Enemy")))
        {

                GameObject playerGameObj = null;

                if(hit.collider != null)
                {
                    playerGameObj = hit.collider.gameObject;
                }
                
                if(hit.rigidbody != null)
                {
                    playerGameObj = hit.rigidbody.gameObject;
                }

                if(playerGameObj == null)
                {
                    return false;
                }

                if (playerGameObj.CompareTag("Player"))
                {

                    Debug.DrawRay(headTransform.position, directionToPlayer.normalized * directionToPlayer.magnitude, Color.green, Mathf.Infinity);
                    return true;
                }
        }

        return false;

    }


    public override void PauseableUpdate()
    {
        timeInState += Time.deltaTime;
        bool inFov = PlayerInFOV();

        if(inFov)
        {
            timeSincePlayerSeen = 0;
        }
        else
        {
            timeSincePlayerSeen += Time.deltaTime;
        }


        stateFunctionMap[state](inFov);
    }



    public void SwitchState(EnemyState state)
    {
        EnemyState copy = this.state;
        this.state = state;
        TransitionStateCallback(copy,state);
        timeInState = 0;
    }

    public void TransitionToNextState()
    {
        if((int)state + 1 > System.Enum.GetNames(typeof(EnemyState)).Length)
        {
            return;
        }

        SwitchState(state+1);
    }

    public void TransitionToPrevState()
    {
        if((int)state - 1 < 0)
        {
            return;
        }
        

        SwitchState(state-1);
    }


    void OnDrawGizmosSelected()
    {

        Vector3 leftDir = Quaternion.Euler(0, -fov / 2, 0) * headTransform.forward;
        Vector3 rightDir = Quaternion.Euler(0, fov / 2, 0) * headTransform.forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(headTransform.position, leftDir * distance);
        Gizmos.DrawRay(headTransform.position, rightDir * distance);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(headTransform.position, headTransform.forward * distance);
    }

    public virtual void UpdateCallback(){}
    public virtual void PatrolCallback(){}
    public virtual void TriggeredCallback(){}
    public virtual void AttackingCallback(){}
    public virtual void DeadCallback(){}
    public virtual void TransitionStateCallback(EnemyState start, EnemyState end){}

     
}