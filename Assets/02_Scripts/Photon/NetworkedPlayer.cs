using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;

public class NetworkedPlayer : MonoBehaviourPun, IPunObservable
{
    struct SyncData //Vector3 와 쿼터니언값을 갖는 구조체
    {
        public Vector3 pos;
        public Quaternion rot;
    };

    public GameObject ovrCameraRig;
    public GameObject otherModel;

    public OVRPlayerController ovrPlayerController;
    public OVRSceneSampleController ovrSceneSampleController;

    //내 몸
    public Transform[] myBody;
    //다른 사람 몸
    public Transform[] otherBody;

    private Vector3 pos;
    private SyncData[] syncData;

    private void Awake()
    {
        ovrPlayerController = GetComponentInChildren<OVRPlayerController>();
        ovrSceneSampleController = GetComponentInChildren<OVRSceneSampleController>();

        //ovrPlayerController.gameObject.SetActive(photonView.IsMine);
        ovrCameraRig.SetActive(photonView.IsMine);
        otherModel.SetActive(!photonView.IsMine);

        if(photonView.IsMine)
        {
            ovrPlayerController.enabled = true;
            ovrSceneSampleController.enabled = true;
        }

        if(!photonView.IsMine)
            syncData = new SyncData[myBody.Length];
    }

    private void Update()
    {
        //내꺼가 아니면
        if(!photonView.IsMine)
        {
            //타 플레이어 캐릭터의 위치값
            transform.position = Vector3.Lerp(transform.position, pos, 0.2f);
            //타 플레이어 캐릭터의 머리, 양손의 위치와 회전값
            for(int i = 0; i < otherBody.Length; i++)
            {
                otherBody[i].position = Vector3.Lerp(otherBody[i].position, syncData[i].pos, 0.1f);
                otherBody[i].rotation = Quaternion.Lerp(otherBody[i].rotation, syncData[i].rot, 0.1f);
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //내가 써야되는 것들(내컴퓨터에서 내것)
        if(stream.IsWriting)
        {
            stream.SendNext(transform.position);
            for(int i = 0; i < myBody.Length; i++)
            {
                stream.SendNext(myBody[i].position);
                stream.SendNext(myBody[i].rotation);
            }
        }
        //내가 받아야되는 것들(내컴퓨터에서 다른 플레이어 것)
        if(stream.IsReading)
        {
            pos = (Vector3)stream.ReceiveNext();
            for (int i = 0; i < otherBody.Length; i++)
            {
                syncData[i].pos = (Vector3)stream.ReceiveNext();
                syncData[i].rot = (Quaternion)stream.ReceiveNext();
            }

        }
    }
}