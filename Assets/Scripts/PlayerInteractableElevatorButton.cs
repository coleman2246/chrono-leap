using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractableElevatorButton : PlayerInteractableObject
{
    public bool pressed = false;

    [SerializeField] private TimeEffectableMovingObject elevatorTimeObj;

    public void Start()
    {
        elevatorTimeObj.activateOnTrigger = true;
        elevatorTimeObj.oneWay = true;

    }
    
    public override void InteractChild()
    {
        elevatorTimeObj.TriggerMovement();
    }
}
