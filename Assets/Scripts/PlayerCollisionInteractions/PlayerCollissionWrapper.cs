using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// should work with both rb collision and colliders
public class PlayerCollissionWrapper : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerCollision(collision.gameObject);
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerCollision(other.gameObject);
        }
    }

    public virtual void PlayerCollision(GameObject player){}
}
