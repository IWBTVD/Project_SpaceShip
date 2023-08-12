using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpaceShipPlaceToBeacon : MonoBehaviourPun
{
    Transform beaconTransform;
    Beacon spaceshipScript;

    public GameObject origin;


    private Vector3 moveToCenterPosition;
    private void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.CompareTag("Beacon"))
        {
            Debug.Log("Beacon Enter detected: 1");
            
            // 비콘과 닿을때 비콘 정보와 스크립트를 읽어오기
            beaconTransform = other.transform;
            spaceshipScript = beaconTransform.GetComponent<Beacon>();
            MoveToCenter();
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.CompareTag("Beacon"))
        {
            Debug.Log("Beacon Exit detected: 1");
            
            // 비콘과 빠질때 비콘 정보와 스크립트를 초기화
            // 값 리셋
            beaconTransform = null;
            spaceshipScript = null;
        }
    }

 
    public void MoveToCenter()
    {
        if (spaceshipScript != null &&spaceshipScript.isUnit == true)
        {
            Debug.Log("Unit is assigned!");
            return;
        }

        if (beaconTransform != null)
        {
            transform.position = beaconTransform.position;
            origin.transform.position = beaconTransform.position;
            // 방향 이동
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            origin.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            
            spaceshipScript.isUnit = true;
        }
        else
        {
            Debug.Log("beaconTransform is not assigned!");
            return;
        }
    }

}

  
