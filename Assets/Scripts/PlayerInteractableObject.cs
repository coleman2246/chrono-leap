using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractableObject : MonoBehaviour
{
    [SerializeField] private bool customMessage = false; 
    [SerializeField] private string message = "Press Button {0} To Interact";

    public bool hasMessage = true; 

    public bool isInteractable = true; 
    private string buttonChar = "E";


    public void SetCustomMessage(string message)
    {
        message = message;
        customMessage = true;
    }

    public void Interact()
    {
        PreInteractCallback();

        if(isInteractable)
        {
            InteractChild();
        }

        PostInteractCallback();
    }

    public void SetButton(string button)
    {
        buttonChar = button.ToUpper();
    }

    public string GetInteractionMessage()
    {
        if(!isInteractable)
        {
            return "";
        }

        if(!hasMessage)
        {
            return "";
        }

        if(customMessage)
        {
            return message;
        }

        return string.Format(message,buttonChar);

    }

    public virtual void PreInteractCallback(){}
    public virtual void InteractChild(){}
    public virtual void PostInteractCallback(){}

}
