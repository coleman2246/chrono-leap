using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingPlatform : TimeEffectedObject
{

    [SerializeField] private bool xAxis = false;
    [SerializeField] private bool yAxis = false;
    [SerializeField] private bool zAxis = false;
    [SerializeField] private float secsPerRotation = 5;

    public override void PauseableStart()
    {

    }

    void ApplyRotation(float angle, Vector3 dir)
    {
        transform.rotation *= Quaternion.Euler(dir * angle);
    }



    public override void PauseableUpdate()
    {
        float angle =  360 / secsPerRotation * Time.deltaTime;

        if(xAxis)
        {
            ApplyRotation(angle,Vector3.left);
        }

        if(yAxis)
        {
            ApplyRotation(angle,Vector3.up);
        }

        if(zAxis)
        {
            ApplyRotation(angle,Vector3.forward);
        }


    }



}
