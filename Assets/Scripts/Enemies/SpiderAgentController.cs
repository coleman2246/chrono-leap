using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class SpiderAgentController : MonoBehaviour
{
    [SerializeField] private Transform target;

    private NavMeshAgent agent;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.updateRotation = true;
        
    }

    void Update()
    {
        agent.SetDestination(target.position);
    }
}
