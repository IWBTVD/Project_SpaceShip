using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceShipPlaceToBeacon : MonoBehaviour
{
    GameObject BeaconObject;
    SpaceshipPresetBeacon spaceshipScript;

    public GameObject origin;
    private void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.CompareTag("Beacon"))
        {
            Debug.Log("Beacon Enter detected: 1");
            
            // 비콘과 닿을때 비콘 정보와 스크립트를 읽어오기
            BeaconObject = other.gameObject;
            spaceshipScript = BeaconObject.GetComponent<SpaceshipPresetBeacon>();
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.CompareTag("Beacon"))
        {
            Debug.Log("Beacon Exit detected: 1");
            
            // 비콘과 빠질때 비콘 정보와 스크립트를 읽어오기
            BeaconObject = null;
            spaceshipScript = null;
        }
    }

    public void MoveToCenter()
    {
        if (spaceshipScript.isUnit == true)
        {
            Debug.Log("Unit is assigned!");
            return;
        }

        if (BeaconObject != null)
        {
            //위치 이동
            transform.position = BeaconObject.transform.position;
            origin.transform.position = BeaconObject.transform.position;
            //방향 이동
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            origin.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            spaceshipScript.isUnit = true;
            
        }
        else
        {
            Debug.Log("BeaconObject is not assigned!");
            return;
        }
    }

}
