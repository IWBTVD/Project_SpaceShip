using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpaceShipPlaceToBeacon : MonoBehaviourPun
{
    Transform beaconTransform;
    Beacon beacon;

    public GameObject origin;
    private SpaceUnit spaceUnit;

    private Vector3 moveToCenterPosition;

    private void Start()
    {
        spaceUnit = GetComponent<SpaceUnit>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Beacon"))
        {
            Debug.Log("Beacon Enter detected: 1");

            // 비콘과 닿을때 비콘 정보와 스크립트를 읽어오기
            beaconTransform = other.transform;
            beacon = beaconTransform.GetComponent<Beacon>();
            spaceUnit.myShipIndex = beacon.myIndex;
            MoveToCenter();
            spaceUnit.myShipIndex.DebugLog();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Beacon"))
        {
            Debug.Log("Beacon Exit detected: 1");

            // 비콘과 빠질때 비콘 정보와 스크립트를 초기화
            // 값 리셋
            beaconTransform = null;
            beacon = null;
            spaceUnit.myShipIndex = new ShipIndex();
            spaceUnit.myShipIndex.DebugLog();
        }
    }


    public void MoveToCenter()
    {
        if (beacon != null && beacon.isUnit == true)
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

            beacon.isUnit = true;
        }
        else
        {
            Debug.Log("beaconTransform is not assigned!");
            return;
        }
    }

}


