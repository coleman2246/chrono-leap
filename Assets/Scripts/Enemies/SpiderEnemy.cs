using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class SpiderEnemy : TurretEnemy
{
    private NavMeshAgent agent;
    [SerializeField] private List<Transform> targets;
    
    private Transform targetTransform;
    private NavMeshPath path;
    int targetIndex = 0;
    
    public override void EnemyStartCallback()
    {
        base.EnemyStartCallback();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;

        path = new NavMeshPath();

        targetTransform = targets[targetIndex];
    }

    public override void EnemyUpdateCallback()
    {
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

    public override void PatrolCallback()
    {
        base.PatrolCallback();

        agent.isStopped = false;
        UpdateTargetPos();

        agent.SetDestination(targetTransform.position);
    }

    public override void TriggeredCallback(bool inFov)
    {
        base.TriggeredCallback(inFov);
        agent.isStopped = true;

    }

    public void UpdateTargetPos()
    {

        float distance = Vector3.Distance(targetTransform.position,transform.position);


        bool pathExists = true;

        if(agent.CalculatePath(targetTransform.position,path))
        {
            pathExists = path.status == NavMeshPathStatus.PathComplete;
        }
        else
        {
            pathExists = false;
        }


        if(distance < 1f || !pathExists)
        {
            IncrementTarget();
        }

    }

    public void IncrementTarget()
    {
        if(targetIndex +1 <= targets.Count -1)
        {
            targetIndex += 1;
        }
        else
        {
            targetIndex = 0;
        }

        targetTransform = targets[targetIndex];

    }




    


}
