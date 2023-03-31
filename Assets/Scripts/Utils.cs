using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour
{
    // checks if parent or siblings contains a specified component
    // returns default if not
    // generic return type would not allow me to return null :( 
    public static T GetComponentInParentOrSibling<T>(GameObject gameObject) 
    {
        if (gameObject == null)
        {
            return default(T);
        }

        T component = gameObject.GetComponent<T>();

        if(component != null)
        {
            return component;
        }

        if (gameObject.transform.parent == null)
        {
            return default(T);
        }

        foreach (Transform sibling in gameObject.transform.parent.transform)
        {
            if (sibling.gameObject == gameObject)
            {
                continue;
            }

            component = sibling.gameObject.GetComponent<T>();
            if (component != null)
            {
                return component;
            }
        }

        component = GetComponentInParentOrSibling<T>(gameObject.transform.parent.gameObject);

        if (component != null)
        {
            return component;
        }

        return default(T);
    }
    
}
