using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeEffectableMovingCrusher : TimeEffectableMovingObject
{
    

    void OnTriggerStay(Collider collision)
    {

        if (collision.gameObject.CompareTag("Player"))
        {
            Vector3 collisionNormal = (collision.transform.position - transform.position).normalized;
            Debug.Log(collisionNormal);
            // most of the collision is happening on the bottom
            if(collisionNormal.y < .5)
            {
                PlayerWorldInteractions player = collision.transform.GetComponent<PlayerWorldInteractions>();
                player.KillPlayer();
                
            }

        }

    }
}
