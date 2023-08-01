using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    GameObject BeaconObject;
    
    private bool isUnit;
    private void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.CompareTag("Beacon"))
        {
            Debug.Log("Beacon Enter detected: 1");
            // Move the OriginObject (ship) to the center of the touched beacon
            BeaconObject = other.gameObject;

        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.CompareTag("Beacon"))
        {
            Debug.Log("Beacon Exit detected: 1");
            // Move the OriginObject (ship) to the center of the touched beacon
            BeaconObject = null;
        }
    }

    public void MoveToCenter()
{
    if (isUnit)
    {
        Debug.LogError("Unit is assigned!");
        return;
    }

    if (BeaconObject != null)
    {
        transform.position = BeaconObject.transform.position;
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }
    else
    {
        Debug.LogError("BeaconObject is not assigned!");
    }
}
}
