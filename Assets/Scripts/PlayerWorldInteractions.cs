using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerHoldingItemStates
{
    Free,
    HoldingItem,
}

public class PlayerWorldInteractions : MonoBehaviour
{
 
    public KeyCode pickUpKey = KeyCode.E;
    public float pickUpDistance = 3f;

    private Rigidbody objectRb;
    private RigidbodyConstraints startConstraints;
    private FixedJoint joint;
    private Camera cam;
    private GameObject itemCarrying;
    private CapsuleCollider col;
    private PlayerHoldingItemStates holdingState = PlayerHoldingItemStates.Free;


    void Start()
    {
        cam = GetComponentInChildren<Camera>();
        col = GetComponentInChildren<CapsuleCollider>();
        itemCarrying = null;
    }


    void HandlePickUp()
    {
        if (!Input.GetKeyDown(pickUpKey))
        {
            return;
        }

        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
        if (Physics.Raycast(ray, out hit,pickUpDistance))
        {
            objectRb = hit.rigidbody;

            if(objectRb == null)
            {
                return;
            }

            PlayerMoveAbleObject obj = hit.rigidbody.GetComponent<PlayerMoveAbleObject>();
            if(obj == null)
            {
                objectRb = null;
                return;
            }

            itemCarrying = objectRb.gameObject;
            objectRb.isKinematic = true;

        }
        

        if(itemCarrying == null)
        {
            return;
        }

        itemCarrying.transform.SetParent(cam.transform);
        startConstraints = objectRb.constraints;
        objectRb.constraints = RigidbodyConstraints.FreezeRotation;
    
        holdingState =  PlayerHoldingItemStates.HoldingItem;



    }

    void HandleDrop()
    {
        if (!Input.GetKeyDown(pickUpKey))
        {
            return;
        }

        itemCarrying.transform.SetParent(null);
        holdingState =  PlayerHoldingItemStates.Free;
        objectRb.isKinematic = false;
        objectRb.constraints = startConstraints;
        itemCarrying = null;
        objectRb = null;

    }

    void Update()
    {
        if(PlayerHoldingItemStates.Free == holdingState)
        {
            HandlePickUp();
        }
        else
        {
            Debug.Log(itemCarrying.transform.position);
            HandleDrop();
        }
    }
}
