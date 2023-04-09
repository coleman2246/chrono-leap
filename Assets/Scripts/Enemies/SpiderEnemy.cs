using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class SpiderEnemy : TurretEnemy
{
    private NavMeshAgent agent;
    [SerializeField] private Transform target;
    
    public override void EnemyStartCallback()
    {
        base.EnemyStartCallback();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;

    }

    public override void EnemyUpdateCallback()
    {
        agent.SetDestination(target.position);
    }

    public override void PauseCallback()
    {
        agent.isStopped = true;

    }

    public override void UnPauseCallback()
    {
        agent.isStopped = false;
    }

    public override void EnemyRewindCallback()
    {
        base.EnemyRewindCallback();

        agent.isStopped = false;
    }



    


}
