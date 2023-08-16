using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SetOrderButton : MonoBehaviourPun
{
    [SerializeField] Oculus.Interaction.InteractableUnityEventWrapper eventWrapper;

    private WorldBoardManager worldBoardManager;

    //0 red 1 blue
    public int teamColor;

    private void Start() {
        worldBoardManager = WorldBoardManager.Instance;
    }
    public void OnEnable()
    {
        //이벤트 구독
        eventWrapper.WhenSelect.AddListener(SendTeamColor);
    }

    public void OnDisable()
    {
        //disable될 때 이벤트 구독 해제
        eventWrapper.WhenSelect.RemoveListener(SendTeamColor);
    }

    public void OnDestroy()
    {
        //destroy될 때 이벤트 구독 해제
        eventWrapper.WhenSelect.RemoveListener(SendTeamColor);
    }

    public void SendTeamColor(){
        photonView.RPC(nameof(SendTeamColorRPC), RpcTarget.All);
    }


    [PunRPC]
    private void SendTeamColorRPC(){
        worldBoardManager.SetOrder(teamColor);

        gameObject.SetActive(false);
    }
}
