using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeEffectableBullet : TimeEffectableMovingObject
{


 void OnTriggerStay(Collider collision)
    {

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerWorldInteractions player = collision.transform.GetComponent<PlayerWorldInteractions>();
            player.KillPlayer();
        }
        else
        {
            Destroy(gameObject);
        }

    }


    public override void AtTargetCallback()
    {
        Destroy(gameObject);

    }

}
