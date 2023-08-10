using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpaceShipPlaceToBeacon : MonoBehaviourPun, IPunObservable
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
            photonView.RPC(nameof(ReceiveTransformData), RpcTarget.All, beaconTransform.position);
            spaceshipScript = beaconTransform.GetComponent<Beacon>();

            MoveToCenterRPC();
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.CompareTag("Beacon"))
        {
            Debug.Log("Beacon Exit detected: 1");
            
            // 비콘과 빠질때 비콘 정보와 스크립트를 초기화
            photonView.RPC(nameof(ClearBeaconData), RpcTarget.All);
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
            moveToCenterPosition = beaconTransform.transform.position;
            // 우주선을 이동하고 isUnit 플래그를 모든 클라이언트에 설정하기 위해 RPC 호출
            photonView.RPC(nameof(MoveToCenterRPC), RpcTarget.All);
        }
        else
        {
            Debug.Log("beaconTransform is not assigned!");
            return;
        }
    }


    [PunRPC]
    private void MoveToCenterRPC()
    {

        // 위치 이동
        transform.position = beaconTransform.position;
        origin.transform.position = beaconTransform.position;
        // 방향 이동
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        origin.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        spaceshipScript.isUnit = true;

    }

    [PunRPC]
    private void ReceiveTransformData(Vector3 position)
    {
        if (beaconTransform != null)
        {
            beaconTransform.position = position;
        }
    }

    [PunRPC]
    private void ClearBeaconData()
    {
        // 값 리셋
        beaconTransform = null;
        spaceshipScript = null;

    }

     public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            
        }
        else
        {
           
        }
    }
}

  
