using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeEffectableMovingPlatform : TimeEffectableMovingObject
{
    void OnTriggerStay(Collider collision)
    {

        if (collision.gameObject.CompareTag("Player"))
        {
            Vector3 collisionNormal = (collision.transform.position - transform.position).normalized;
            Debug.Log(collisionNormal);
            if(collisionNormal.y > .5)
            {
                Rigidbody playerRb = collision.transform.GetComponent<Rigidbody>();
                PlayerController player = collision.transform.GetComponent<PlayerController>();

                if( Time.time - player.moveTime > 0.1f)
                {
                     playerRb.constraints |= RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;

                    playerRb.isKinematic = true;
                }

                //rb.mass = rb.mass - playerRb.mass;
                collision.transform.SetParent(transform);

            }

        }

    }


    void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);

            PlayerController player = collision.transform.GetComponent<PlayerController>();
            Rigidbody playerRb = collision.transform.GetComponent<Rigidbody>();
            //rb.mass = rb.mass + playerRb.mass;
            playerRb.isKinematic = false;
            playerRb.constraints = player.startingConstraints; 
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        TimeEffectableMovingObject other = collision.gameObject.GetComponent<TimeEffectableMovingObject>();
        Rigidbody otherRb = collision.gameObject.GetComponent<Rigidbody>();


        if ( otherRb == null || other == null)
        {
            return;
        }


         foreach (ContactPoint contact in collision.contacts)
        {

            Vector3 velocity = otherRb.velocity;
            Vector3 normal = contact.normal;

            Debug.Log(Vector3.Dot(velocity, normal));

            // if there is a collision the angle between the
            // normal and the 
            if (Vector3.Dot(velocity, normal) > 0)
            {
                other.InvertMovingState();
                other.SetRequiredVelocity();
            }
            break;
        }
    }

}
