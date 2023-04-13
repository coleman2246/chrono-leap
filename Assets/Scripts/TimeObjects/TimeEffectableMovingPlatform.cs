using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeEffectableMovingPlatform : TimeEffectableMovingObject
{
    void OnTriggerStay(Collider collision)
    {
        HandlePlayerCollision(collision.gameObject);
    }

    void HandlePlayerCollision(GameObject collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {

            Vector3 collisionNormal = (collision.transform.position - transform.position).normalized;
            if(collisionNormal.y > .5)
            {
                Rigidbody playerRb = collision.transform.GetComponent<Rigidbody>();
                PlayerController player = collision.transform.GetComponent<PlayerController>();

                float diff = collision.transform.position.y - transform.position.y;

                if( Time.time - player.moveTime > 0.1f)
                {

                    playerRb.isKinematic = true;
                    playerRb.useGravity = false;
                }
                else if(diff < 1)
                {
                    Vector3 newPos = collision.transform.position;
                    newPos.y += 1-diff;
                    collision.transform.position = newPos;
                }

                //rb.mass = rb.mass - playerRb.mass;
                collision.transform.SetParent(transform);

            }

        }

    }


    void OnCollisionExit(Collision other)
    {

        if (other.gameObject.CompareTag("Player"))
        {
            other.transform.SetParent(null);

            PlayerController player = other.transform.GetComponent<PlayerController>();
            Rigidbody playerRb = other.transform.GetComponent<Rigidbody>();

            playerRb.isKinematic = false;
            //rb.mass = rb.mass + playerRb.mass;
            //playerRb.isKinematic = false;
            //playerRb.constraints = player.startingConstraints; 

            playerRb.useGravity = true;
        }
    }
    /*
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

            //playerRb.useGravity = true;
        }
    }
    */

    void OnCollisionEnter(Collision collision)
    {
        
        HandlePlayerCollision(collision.gameObject);


        TimeEffectableMovingPlatform other = collision.gameObject.GetComponent<TimeEffectableMovingPlatform>();

        Rigidbody otherRb = collision.gameObject.GetComponent<Rigidbody>();


        if ( otherRb == null || other == null)
        {
            return;
        }
        
        if(isPaused)
        {
            return;
        }

        if(other.isPaused)
        {
            InvertMovingState();
            SetRequiredVelocity();
        }


        foreach (ContactPoint contact in collision.contacts)
        {

            Vector3 velocity = other.GetVelocity();
            Vector3 normal = contact.normal;


            if (Vector3.Dot(velocity, normal) > 0)
            {
                other.InvertMovingState();
                other.SetRequiredVelocity();
            }
            break;
        }
    }

}
