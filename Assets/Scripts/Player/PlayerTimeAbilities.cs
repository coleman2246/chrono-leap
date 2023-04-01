using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTimeAbilities : MonoBehaviour
{
    [SerializeField] private int maxStoppedObjects = 3;
    [SerializeField] private int numberOfStoppedObjects = 0;
    [SerializeField] private int maxTimeStopCharges = 3;
    [SerializeField] private float timestopRechargeTime = 3;
    [SerializeField] private float freezeCooldown = 10f;
    [SerializeField] private float currentCooldown = 0f;
    [SerializeField] private float radius = 30f;

    LinkedList<TimeEffectedObject> stoppedObjects;

    public int remainingTimeStopCharges = 3;
    private Camera cam;

    void Start()
    {
        stoppedObjects = new LinkedList<TimeEffectedObject>();
        cam = GetComponentInChildren<Camera>();
    }

    void Update()
    {
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
        currentCooldown -= Time.deltaTime;

        if(currentCooldown < 0)
        {
            currentCooldown = freezeCooldown;
            SetTimeStopCharges(remainingTimeStopCharges += 1);
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
            if(currCollider.transform.parent == null)
            {

                timeObject = currCollider.transform.GetComponent<TimeEffectedObject>();
            }
            else
            {
                timeObject = currCollider.transform.parent.GetComponent<TimeEffectedObject>();
            }


            if(timeObject == null)
            {
                continue;
            }

            timeObject.Rewind();

        }


    }


    void PauseNewObject(TimeEffectedObject timeObject)
    {
        numberOfStoppedObjects = stoppedObjects.Count;

        while(stoppedObjects.Count >= maxStoppedObjects)
        {
            stoppedObjects.First.Value.UnPause();
            stoppedObjects.RemoveFirst();
        }

        timeObject.Pause();
        stoppedObjects.AddLast(timeObject);
    }

    void UnPauseNewObject(TimeEffectedObject timeObject)
    {

        int targetId = timeObject.GetInstanceID();

        foreach(TimeEffectedObject currObject in stoppedObjects)
        {
            if(currObject.GetInstanceID() == targetId)
            {
                currObject.UnPause();
                stoppedObjects.Remove(currObject);
                numberOfStoppedObjects -= 1;
                return;
            }

        }
    }
}
