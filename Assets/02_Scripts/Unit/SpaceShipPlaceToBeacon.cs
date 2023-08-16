using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpaceShipPlaceToBeacon : MonoBehaviourPun , IPunObservable
{
    Transform beaconTransform;
    Beacon spaceshipScript;

    public GameObject origin;

    private void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.CompareTag("Beacon"))
        {
            Debug.Log("Beacon Enter detected: 1");
            
            // 비콘과 닿을때 비콘 정보와 스크립트를 읽어오기
            beaconTransform = other.transform;
            spaceshipScript = beaconTransform.GetComponent<Beacon>();
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.CompareTag("Beacon"))
        {
            Debug.Log("Beacon Exit detected: 1");
            
            // 비콘과 빠질때 비콘 정보와 스크립트를 읽어오기
            beaconTransform = null;
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

        if (beaconTransform != null)
        {
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
        transform.position = beaconTransform.transform.position;
        origin.transform.position = beaconTransform.transform.position;
        // 방향 이동
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        origin.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        spaceshipScript.isUnit = true;

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(beaconTransform);
        }

        else
        {
           beaconTransform = (Transform)stream.ReceiveNext();
        }
    }
}

  
