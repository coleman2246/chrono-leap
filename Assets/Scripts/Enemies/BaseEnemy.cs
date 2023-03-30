using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEnemy : TimeEffectedObject
{
    [SerializeField] private Transform headTransform;
    [SerializeField] private int fov = 135;
    [SerializeField] private int distance = 25;

    private Transform player;
    
    public override void PauseableStart()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }


    public override void PauseableUpdate()
    {

        Vector3 directionToPlayer = player.position - transform.position;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        Debug.Log(headTransform.forward);

        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToPlayer, out hit, distance))
        {

                if (hit.collider.gameObject == player.gameObject)
                {
                    Debug.Log("Player is in view");
                }
        }

 


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
 
}
