using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public struct RigidbodyData
{
    public Vector3 angularVelocity;
    public Vector3 velocity;
    public bool isKinematic;

    public RigidbodyData(Rigidbody rb)
    {
        angularVelocity = new Vector3(rb.angularVelocity.x, rb.angularVelocity.y, rb.angularVelocity.z);
        velocity = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z);
        isKinematic = rb.isKinematic;
    }

    public void CopyToRigidBody(Rigidbody rb)
    {
        rb.angularVelocity = angularVelocity;
        rb.velocity = velocity;
        rb.isKinematic = isKinematic;
    }


}


public struct TransformData
{
    Vector3 position;
    Quaternion rotation;
    List<TransformData> children;

    public TransformData(Transform transform)
    {
        position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        rotation = new Quaternion(transform.rotation.x, transform.rotation.y,
                transform.rotation.z, transform.rotation.w);

        children = new List<TransformData>();

        foreach(Transform child in transform)
        {
            children.Add(new TransformData(child));
        }

    }

    

    public void CopyToTransform(Transform transform)
    {
        transform.position = position;
        transform.rotation = rotation;


        foreach(Transform child in transform)
        {
            if(children.Count > 0)
            {
                TransformData dataChild = children[0];
                dataChild.CopyToTransform(child);
                children.RemoveAt(0);


            }

        }


    }
}

public struct CommonTracking
{
    public TransformData tfData;
    public RigidbodyData? rbData;


    public CommonTracking(Transform newTransform, Rigidbody newRb)
    {
        this.tfData = new TransformData(newTransform);

        this.rbData = null;

        if(newRb != null)
        {
            this.rbData = new RigidbodyData(newRb);
        }
    }
}


public class TimeEffectedObject : MonoBehaviour
{
    /*
     * This is the base class for all time objects
     * The assumptions made of these objects are:
     *  - They will not change in structure (some number of children and parents)
     *  - They all have a transform (pretty sure all unity objects have transforms)
     *  - They may or may not have a rigidbody
     *  - All other things that need to be tracked in time need be setup in child class \
     *  using the RegisterTracker function
     *
     * */

    [SerializeField] public bool isPaused = false;
    [SerializeField] private bool pauseTest = false;
    [SerializeField] private bool rewindTest = false;
    [SerializeField] private float rewindLength = 5f; // in secs
    [SerializeField] private float rewindFrequency = 10f; // in hz
    [SerializeField] private float playBackSpeed = .5f; // in secs

    public Shader outLine;
    
    private LinkedList<object> commonTracking = new LinkedList<object>();
    private List<LinkedList<object>> timeTracker = new List<LinkedList<object>>();

    private CommonTracking pauseSaveState;
    private PlayerTimeAbilities playerTime;

    protected Rigidbody rb;
    private GameObject hourglassPrefab;
    private GameObject spawnedHourglass;

    void Start()
    {
        gameObject.tag = "TimeEffectedObject";
        rb = GetComponent<Rigidbody>();

        PauseableStart();

        InvokeRepeating("NewTimeStep",0, 1f/rewindFrequency);

        playerTime = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerTimeAbilities>();
        hourglassPrefab = Resources.Load ("Prefabs/Hourglass") as GameObject;


    }

    public void RegisterTracker(ref LinkedList<object> tracker)
    {
        timeTracker.Add(tracker);
    }

    void Update()
    {
        if(pauseTest)
        {
            Pause();
        }
        else
        {
            //UnPause();
        }

        if(rewindTest)
        {
            Rewind();
            rewindTest = false;
        }

        if(!isPaused)
        {
            PauseableUpdate();
        }
    }

    void FixedUpdate()
    {
        if(!isPaused)
        {
            PauseableFixedUpdate();
        }
    }


    void NewTimeStep()
    {
        if(!isPaused)
        {
            CommonTracking newTimeStep = new CommonTracking(transform,rb);
            commonTracking.AddLast((object)newTimeStep);
            TimeStepCallback();
            TrimTimeSteps();
        }

    }

    void TrimTimeSteps()
    {
        int desiredLength = Mathf.CeilToInt(rewindLength * rewindFrequency);

       foreach(LinkedList<object> currTracker in timeTracker)
       {
            while(currTracker.Count > desiredLength)
            {
                currTracker.RemoveFirst();

            }
       }

        while(commonTracking.Count > desiredLength)
        {
            commonTracking.RemoveFirst();

        }
        

    }

    public void Pause()
    {

        if(isPaused)
        {
            return;
        }

        pauseSaveState = new CommonTracking(transform,rb);
        isPaused = true;


        // fix this later
        /*
        Vector3 spawnLocation = transform.position;
        Quaternion quaternion = Quaternion.identity;


        Transform[] childTransforms = GetComponentsInChildren<Transform>();

        // Find the highest point in the child objects
        float highestPoint = float.MinValue;
        foreach(Transform childTransform in childTransforms)
        {
            if (childTransform.position.y > highestPoint)
            {
                highestPoint = childTransform.position.y;
            }
        }

        spawnLocation.y = highestPoint + 0.1f;


        spawnedHourglass = Instantiate(hourglassPrefab,
                spawnLocation,
                quaternion,
                transform
        );

        spawnedHourglass.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        */


        ClearRigidBody(); 
        PauseCallback();

    }

    public void ClearRigidBody()
    {

        return;

        if(rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = new Vector3(0,0,0);
            rb.angularVelocity = new Vector3(0,0,0);
        }

    }

    public void UnPause()
    {
        if(!isPaused)
        {
            return;
        }

        UnPauseCallback();


        if(rb != null)
        {
            pauseSaveState.rbData?.CopyToRigidBody(rb);
        }


        isPaused = false;
        
        if(spawnedHourglass != null)
        {
            Destroy(spawnedHourglass);
        }

    }

    public void Rewind()
    {
        if(isPaused)
        {
            playerTime.RemoveObject(this);
        }

        isPaused = true;
        RewindStartCallback();
        AnimateRewind();
    }

    public async void AnimateRewind()
    {
        int waitTime = Mathf.CeilToInt( (this.playBackSpeed / commonTracking.Count) ); // units are ms

        List<bool> doneAll = new List<bool>(new bool[timeTracker.Count+1]);
        bool done = false;

        while(!done)
        {
            for(int i = 0; i < timeTracker.Count; i++)
            {
                LinkedList<object> currTracker = timeTracker[i];

                if(currTracker.Count > 2)
                {
                    AnimateRewindCallback();
                    currTracker.RemoveLast();
                }
                else
                {
                    doneAll[i] = true;
                }
            }


            if(commonTracking.Count > 2)
            {
                CommonTracking current = (CommonTracking)commonTracking.Last.Value;
                current.tfData.CopyToTransform(transform);
                commonTracking.RemoveLast();
            }
            else 
            {
                doneAll[doneAll.Count-1] = true;
            }

            await Task.Delay(waitTime);

            done = true;

            int index  = 0;
            foreach(bool currCheck in doneAll)
            {
                done = done && currCheck; 
            }
        }

        if(commonTracking.Count > 0)
        {

            CommonTracking current = (CommonTracking)commonTracking.Last.Value;
            current.rbData?.CopyToRigidBody(rb);
            commonTracking.RemoveLast();
        }


        RewindEndCallback();
        

        for(int i = 0; i < timeTracker.Count; i++)
        {
            LinkedList<object> currTracker = timeTracker[i];

            if(currTracker.Count > 0)
            {
                currTracker.RemoveLast();
            }
        }

        isPaused = false;
    }

    void OnDestroy()
    {
        OnDestroyCallback();
        if(isPaused)
        {
            playerTime.RemoveObject(this);
        }

    }


    // all children class should use these instead of gameObject defaults 
    public virtual void PauseableUpdate(){}
    public virtual void PauseableFixedUpdate(){}
    public virtual void PauseableStart(){}


    // children should should implement these if they wish to use a callback when paused/rewinded
    public virtual void PauseCallback(){}
    public virtual void UnPauseCallback(){}
    public virtual void AnimateRewindCallback(){}
    public virtual void RewindStartCallback(){}
    public virtual void RewindEndCallback(){}

    public virtual void TimeStepCallback(){}
    public virtual void OnDestroyCallback(){}


}
