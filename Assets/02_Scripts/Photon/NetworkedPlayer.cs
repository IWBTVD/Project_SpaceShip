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
    public OVRPlayerController ovrPlayerController;
    public OVRSceneSampleController ovrSceneSampleController;
    private WorldBoardManager worldBoardManager;

    // Grabber 껏다 켰다 하기 위한 변수값
    public CustomOVRGrabber ovrGrabber; 


    //내 몸
    public Transform[] myBody;

    private Vector3 pos;
    private SyncData[] syncData;

    private void Awake()
    {
        ovrPlayerController = GetComponentInChildren<OVRPlayerController>();
        ovrSceneSampleController = GetComponentInChildren<OVRSceneSampleController>();

        //ovrPlayerController.gameObject.SetActive(photonView.IsMine);
        ovrCameraRig.SetActive(photonView.IsMine);

        if(photonView.IsMine)
        {
            ovrPlayerController.enabled = true;
            ovrSceneSampleController.enabled = true;
        }

        if(!photonView.IsMine)
            syncData = new SyncData[myBody.Length];
    }

    private void Start(){
        worldBoardManager = WorldBoardManager.Instance;
        worldBoardManager.OnCurrentTurnChanged += onCurrentTurnChagedHandler;
    }

    private void Destroy(){
        worldBoardManager.OnCurrentTurnChanged -= onCurrentTurnChagedHandler;
    }

    private void onCurrentTurnChagedHandler(WorldBoardManager.Turn turn){
        Debug.Log("NetworkedPlater Current turn changed: " + turn.ToString());
        
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //내가 써야되는 것들(내컴퓨터에서 내것)
        if(stream.IsWriting)
        {

        }
        //내가 받아야되는 것들(내컴퓨터에서 다른 플레이어 것)
        if(stream.IsReading)
        {

        }
    }
}