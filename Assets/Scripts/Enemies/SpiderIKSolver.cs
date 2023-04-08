using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class SpiderIKSolver : MonoBehaviour
{
    Vector3 targetPos = default(Vector3);

    Transform hintTransform;
    float stepDistance = 0.4f; // distace before a step will be taken
    bool takingStep = false;
    float stepHeight = 0.2f;
    float strideLength = 0.4f;
    Vector3 initialOffset;
    
    void Start()
    {
        bool foundHint = false;

        foreach (Transform sibling in transform.parent)
        {

            if (sibling.name.Contains("hint"))
            {
                hintTransform = sibling;
                break;
            }
        }
        
        initialOffset = transform.position - transform.parent.position;

        Vector3 startPos = transform.position;
        Ray ray = new Ray(startPos, Vector3.down);

        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, 10))
        {
            targetPos = hit.point;
        }

        targetPos.x += Random.Range(-0.2f, 0.20f);
        targetPos.z += Random.Range(-0.2f, 0.20f);


    }

    void TakeStep()
    {
        float distance = Vector3.Distance(transform.parent.position, targetPos);

        if(distance <  stepDistance)
        {
            return;
        }

        if(!takingStep)
        {
            Vector3 offset = transform.parent.position;
            offset += initialOffset;

            Ray ray = new Ray(offset, Vector3.down);

            Debug.DrawRay(ray.origin,ray.direction, Color.red);

            RaycastHit hit;

            if(Physics.Raycast(ray, out hit, 2))
            {

                takingStep = true;
                Vector3 point = hit.point;


                UpdateFootPos(point);
            }


        }

    }


    async void UpdateFootPos(Vector3 newPos)
    {
        float lerp = 0;
        Vector3 currentPos = new Vector3(0,0,0);

        

        Vector3 dir = (transform.position - newPos).normalized * stepDistance;

        float speed = 4f;
        //newPos += dir;


        while(lerp < stepDistance)
        {
            currentPos = Vector3.Lerp(targetPos, newPos, lerp);
            currentPos.y += Mathf.Sign(lerp * Mathf.PI) * stepHeight; 

            SetPos(currentPos);
            // basically integrating to get displacement


            lerp += Time.deltaTime * speed;

            Debug.Log(speed);
            //wait till next frame
            await Task.Yield();

            if(Vector3.Distance(transform.parent.position, currentPos) > stepDistance)
            {
                Debug.Log("exti early");
                //break;
            }
        }

        targetPos = newPos;

        takingStep = false;
    }

    void SetPos(Vector3 pos)
    {
        if(Vector3.Distance(transform.position,pos) > 0.01)
        {
            pos.x += Random.Range(-0.03f, 0.03f);
            pos.z += Random.Range(-0.03f, 0.03f);


            //rb.isKinematic = true;
            transform.position = pos;
            hintTransform.position = pos;
            //rb.isKinematic = false;
        }
    }

    void Update()
    {
        
        TakeStep();

        if(!takingStep)
        {
            SetPos(targetPos);
        }
    }


    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(targetPos, .05f);
    }
}
