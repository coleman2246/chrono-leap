using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RotatingItem : MonoBehaviour
{
    [SerializeField] private float rpm;
    [SerializeField] bool x;
    [SerializeField] bool y;
    [SerializeField] bool z;


    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float angle = 6f * rpm * Time.deltaTime;


        

        Vector3 direction = new Vector3(0, 0, 0);

        if (x)
        {
            direction += Vector3.left;
        }

        if (y)
        {
            direction += Vector3.up;
        }

        if (z)
        {
            direction += Vector3.forward;
        }

        transform.rotation *= Quaternion.Euler(direction * angle);
    } 
}
