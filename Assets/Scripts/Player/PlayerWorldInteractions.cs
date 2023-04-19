using UnityEngine.SceneManagement;
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
 
    public float pickUpDistance = 3f;
    public float interactionDistance = 3f;
    public float interactionSphereRadius = 1f;
    public bool isDead = false;
    public bool isPaused = false;


    private Ray ray;
    private Rigidbody objectRb;
    private RigidbodyConstraints startConstraints;
    private FixedJoint joint;
    private Camera cam;
    private GameObject itemCarrying;
    private CapsuleCollider col;
    private PlayerHoldingItemStates holdingState = PlayerHoldingItemStates.Free;
    private PlayerUIController uiController;

    void Start()
    {
        cam = GetComponentInChildren<Camera>();
        ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

        col = GetComponentInChildren<CapsuleCollider>();
        itemCarrying = null;

        uiController = GetComponentInChildren<PlayerUIController>();

    }


    void HandlePickUp()
    {
        if (!Input.GetButtonDown("Fire3"))
        {
            return;
        }

        RaycastHit hit;
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
        if (!Input.GetButtonDown("Fire3"))
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

    public void HandleObjectInteraction()
    {

        RaycastHit hit;
        if (!Physics.SphereCast(ray, interactionSphereRadius, out hit, interactionDistance))
        {
            return;
        }

        Collider col = hit.collider;
        Rigidbody rb = hit.rigidbody;

        GameObject obj = null;

        if(col != null)
        {
            obj = col.gameObject;
        }

        if(rb != null)
        {
            obj = rb.gameObject;
        }

        if(obj == null)
        {
            return;
        }

        PlayerInteractableObject interactionObject = obj.GetComponent<PlayerInteractableObject>();

        if(interactionObject == null)
        {
            return;
        }

        //Debug.Log(interactionObject.GetInteractionMessage());
        uiController.SetInteractMessage(interactionObject.GetInteractionMessage());

        if(!Input.GetButtonDown("Interact"))
        {
            return;
        }

        interactionObject.Interact();
    }

    void HandlePause()
    {
        if(!Input.GetButtonDown("Cancel"))
        {
            return;
        }

        if(isPaused)
        {
            UnPause();
        }
        else
        {
            Pause();
        }
        
    }

    public void UnPause()
    {
        uiController.UnPause();
        Time.timeScale = 1; 
        isPaused = false;
    }

    public void Pause()
    {
        uiController.Pause();
        Time.timeScale = 0; 
        isPaused = true;
    }

    public void GameOverMenu(string msg)
    {
        uiController.GameOverMenu(msg);
        Time.timeScale = 0; 
        isPaused = true;
    }

    void Update()
    {

        ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));

        if(PlayerHoldingItemStates.Free == holdingState)
        {
            HandlePickUp();
        }
        else
        {
            HandleDrop();
        }

        HandleObjectInteraction();

        HandlePause();
    }


    public void KillPlayer()
    {
        // pause game time
        Debug.Log("Player Killed");
        //GameOverMenu("You Have Died");
        // show ui
        // restart scene
        // quit to main menu 
    }

    public void LevelEnd()
    {
        Debug.Log("Level Ended");
        GameOverMenu("Level Beaten");
        ProgressManager.UnlockNextLevel(SceneManager.GetActiveScene().name);
        // pause level
        // show ui

    }

    public void RestartLevel()
    {

        Time.timeScale = 1; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);

    }

    public void Quit()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("PickLevel", LoadSceneMode.Single);
    }
}
