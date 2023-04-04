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
   public EnemyState state;
   public float timeInState;
   public float timeSincePlayerSeen;
   public float timeSinceLastAttack;

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
    [SerializeField] protected Transform headTransform;
    [SerializeField] private int fov = 135;
    [SerializeField] protected int distance = 25;
    [SerializeField] private float forgetTime = 5f;
    [SerializeField] protected float maxAttackInterval = .5f;
    [SerializeField] private EnemyState state = EnemyState.Patrol;
    [SerializeField] protected Transform bulletSpawnLocation;
    [SerializeField] protected GameObject bulletPrefab;
    
    [SerializeField] private AudioClip[] transStateSounds = new AudioClip[System.Enum.GetNames(typeof(EnemyState)).Length];
    [SerializeField] private AudioClip fireSound;

    protected Transform player;
    private float timeSincePlayerSeen = 0f;
    private LinkedList<object> baseEnemyStateTracker = new LinkedList<object>();
    private Dictionary<EnemyState, Action<bool>> stateFunctionMap = new Dictionary<EnemyState, Action<bool>>();
    protected float timeSinceLastAttack = 0f;
    protected float timeInState = 0f;
    protected AudioSource audioSource;
    private bool changedStates = false;

    public override void PauseableStart()
    {

        stateFunctionMap[EnemyState.Patrol] = Patrol;
        stateFunctionMap[EnemyState.Triggered] =  Triggered;
        stateFunctionMap[EnemyState.Attacking] =  Attacking;
        stateFunctionMap[EnemyState.Dead] =  Dead;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        audioSource = gameObject.AddComponent<AudioSource>();


        RegisterTracker(ref baseEnemyStateTracker);
        EnemyStartCallback();

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
        EnemyTimeStepCallback();

    }
    
    public override void RewindEndCallback()
    {
        if(baseEnemyStateTracker.Last != null)
        {
            EnemyStateTracking newState = (EnemyStateTracking) baseEnemyStateTracker.Last.Value;
            state = newState.state;
            timeInState = newState.timeInState;
            timeSincePlayerSeen = newState.timeSincePlayerSeen;
            timeSinceLastAttack = newState.timeSinceLastAttack;
        }

        EnemyRewindCallback();

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

        /*
        if(!playerInFov)
        {
            return;
        }
        */

        TriggeredCallback(playerInFov);

    }

    public void Attacking(bool playerInFov)
    {
        if(timeSincePlayerSeen > forgetTime)
        {
            SwitchState(EnemyState.Patrol);
            return;
        }

        AttackingCallback(playerInFov);
        
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

        // want everything that is not enemy

        if(Physics.Raycast(headTransform.position, directionToPlayer.normalized * directionToPlayer.magnitude, out hit, distance, ~LayerMask.GetMask("Enemy")))
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
    
    public Quaternion GetQuaternionToPlayer()
    {
        Vector3 directionToPlayer = player.position - headTransform.position;
        Quaternion rotationToPlayer = Quaternion.FromToRotation(headTransform.forward, directionToPlayer);
        float angleToPlayerY = rotationToPlayer.eulerAngles.y;
        float angleToPlayerX = rotationToPlayer.eulerAngles.x;

        // dont want any rotation in z dimension
        Quaternion rot =  Quaternion.Euler(angleToPlayerX, angleToPlayerY, 0) * headTransform.rotation;
        return rot;
    }

    public override void PauseableUpdate()
    {
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

        EnemyUpdateCallback();
        if(changedStates)
        {
            timeInState = 0;
            changedStates = false;
        }
        else
        {
            timeInState += Time.deltaTime;
        }

    }



    public void SwitchState(EnemyState state)
    {
        EnemyState copy = this.state;
        this.state = state;
        TransitionStateCallback(copy,state);

        AudioClip clip = transStateSounds[(int)state];
        if(clip != null)
        {
            audioSource.clip = clip;
            audioSource.loop = false;
            audioSource.Play();
            
        }

        changedStates = true;

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

    protected void FireBullet(Vector3 endPoint)
    {
        // need to add 180 or they spawn backwards
        GameObject bulletGameObj = Instantiate(bulletPrefab,
                bulletSpawnLocation.position,
                bulletSpawnLocation.rotation * Quaternion.Euler(0,180,0)
        );


        TimeEffectableBullet timeBullet = bulletGameObj.GetComponent<TimeEffectableBullet>();

        timeBullet.oneWay = true;
        timeBullet.useSpeed = true;
        timeBullet.speed = 6f;

        if(fireSound != null)
        {
            audioSource.clip = fireSound;
            audioSource.loop = false;
            audioSource.Play();
        }
        
        timeBullet.SetEndPos(endPoint);

        timeSinceLastAttack = 0;

    }

    public virtual void EnemyStartCallback(){}
    public virtual void EnemyUpdateCallback(){}
    public virtual void PatrolCallback(){}
    public virtual void TriggeredCallback(bool inFov){}
    public virtual void AttackingCallback(bool inFov){}
    public virtual void DeadCallback(){}
    public virtual void EnemyRewindCallback(){}
    public virtual void EnemyTimeStepCallback(){}
    public virtual void TransitionStateCallback(EnemyState start, EnemyState end){}

     
}
