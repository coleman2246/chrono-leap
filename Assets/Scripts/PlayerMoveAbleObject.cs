using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveAbleObject : TimeEffectedObject
{
    private Rigidbody objectRb;

    public override void PauseableStart()
    {
        Rigidbody attachedRb = GetComponent<Rigidbody>();
        if(attachedRb == null)
        {

            objectRb = gameObject.AddComponent<Rigidbody>();
        }
        else
        {
            objectRb = attachedRb;
        }

        objectRb.freezeRotation = true;
    }

    public override void PauseCallback()
    {
        objectRb.isKinematic = true;
    }

    public override void UnPauseCallback()
    {
        objectRb.isKinematic = false;
    }
    
}
