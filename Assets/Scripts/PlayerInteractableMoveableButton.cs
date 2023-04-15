using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractableMoveableButton : PlayerInteractableObject
{
    public bool pressed = false;

    [SerializeField] private List<TimeEffectableMovingObject> moveableTimeObj;

    public override void InteractStartCallback()
    {

        foreach(TimeEffectableMovingObject obj in moveableTimeObj)
        {
            obj.activateOnTrigger = true;
        }

    }
    
    public override void InteractChild()
    {
        foreach(TimeEffectableMovingObject obj in moveableTimeObj)
        {
            obj.TriggerMovement();
        }
    }
}
