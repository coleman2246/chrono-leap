using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTimeAbilities : MonoBehaviour
{
    [SerializeField] public int maxStoppedObjects = 3;
    [SerializeField] public int numberOfStoppedObjects = 0;
    [SerializeField] public int maxTimeStopCharges = 3;
    [SerializeField] public float timestopRechargeTime = 3;
    [SerializeField] private float freezeCooldown = 10f;
    [SerializeField] private float currentCooldown = 0f;
    [SerializeField] private float radius = 30f;
    [SerializeField] private AudioClip timeInteractionSound;

    LinkedList<TimeEffectedObject> stoppedObjects;

    public int remainingTimeStopCharges = 3;
    private Camera cam;
    private PlayerWorldInteractions playerWorld;
    private AudioSource audioSource;

    void Start()
    {
        stoppedObjects = new LinkedList<TimeEffectedObject>();
        cam = GetComponentInChildren<Camera>();
        playerWorld = GetComponent<PlayerWorldInteractions>();
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if(playerWorld.isPaused)
        {
            return;
        }

        if(playerWorld.holdingState != PlayerHoldingItemStates.Free)
        {
            return;
        }

        HandleTimePause();
        HandleTimeUnPause();
        HandleRewind();
    }




    void SetTimeStopCharges(int i)
    {
        int setVal = i;
        if(i > maxTimeStopCharges)
        {
            setVal = maxTimeStopCharges;
        }
        remainingTimeStopCharges = setVal;
    }

    TimeEffectedObject GetRayCastTimeEffectedObject()
    {
        TimeEffectedObject timeObject = null;

        // assume center of screen
        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));

        RaycastHit hit;


        if (!Physics.Raycast(ray, out hit,Mathf.Infinity, ~LayerMask.GetMask("Player")))
        {
            return timeObject;
        }

        Debug.Log("Hit object: " + hit.transform.name);

        TimeEffectedObject foundObj = Utils.GetComponentInParentOrSibling<TimeEffectedObject>(hit.transform.gameObject);

        if(foundObj == default(TimeEffectedObject))
        {
            return timeObject;
        }

        return foundObj;
    }


    void HandleTimeUnPause()
    {
        if(!Input.GetButtonDown("Fire2"))
        {
            return;
        }

        TimeEffectedObject timeObject = GetRayCastTimeEffectedObject();

        if(timeObject == null)
        {
            return;
        }

        if(!timeObject.isPaused)
        {
            return;
        }

        UnPauseNewObject(timeObject);

    }

    void HandleTimePause()
    {
        if(remainingTimeStopCharges < maxTimeStopCharges)
        {

            currentCooldown -= Time.deltaTime;
        }


        if(currentCooldown < 0)
        {
            currentCooldown = freezeCooldown;
            SetTimeStopCharges(remainingTimeStopCharges+1);
        }

        if(remainingTimeStopCharges < 1)
        {
            return;
        }



        if(!Input.GetButtonDown("Fire1"))
        {
            return;
        }

        
        TimeEffectedObject timeObject = GetRayCastTimeEffectedObject();

        if(timeObject == null)
        {
            return;
        }

        if(timeObject.isPaused)
        {
            return;
        }

        remainingTimeStopCharges -= 1;

        PauseNewObject(timeObject);
        PlayTimeInteractionSound();

    }

    void PlayTimeInteractionSound()
    {
        if(timeInteractionSound != null)
        {
            audioSource.clip = timeInteractionSound;
            audioSource.loop = false;
            audioSource.Play();
        }
    }

    void HandleRewind()
    {
        if(!Input.GetButtonDown("RewindButton"))
        {
            return;
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);

        foreach(Collider currCollider in hitColliders)
        {
            TimeEffectedObject timeObject;
            timeObject = Utils.GetComponentInParentOrSibling<TimeEffectedObject>(currCollider.gameObject);

            if(timeObject == default(TimeEffectedObject))
            {
                continue;
            }

            timeObject.Rewind();

        }


    }


    void PauseNewObject(TimeEffectedObject timeObject)
    {

        while(stoppedObjects.Count >= maxStoppedObjects)
        {
            stoppedObjects.First.Value.UnPause();
            stoppedObjects.RemoveFirst();
        }

        timeObject.Pause();
        stoppedObjects.AddLast(timeObject);

        numberOfStoppedObjects = stoppedObjects.Count;
    }

    public bool RemoveObject(TimeEffectedObject timeObject)
    {
        int targetId = timeObject.GetInstanceID();

        foreach(TimeEffectedObject currObject in stoppedObjects)
        {
            if(currObject.GetInstanceID() == targetId)
            {
                stoppedObjects.Remove(currObject);
                numberOfStoppedObjects -= 1;
                return true;
            }

        }

        return false;

    }

    void UnPauseNewObject(TimeEffectedObject timeObject)
    {
        bool removed = RemoveObject(timeObject);
        if(removed)
        {
            timeObject.UnPause();
            PlayTimeInteractionSound();
        }
    }
}
