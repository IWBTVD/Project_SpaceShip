using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public static PhotonManager Instance { get; private set; }


    /// <summary>
    /// 나
    /// </summary>
    public Player myPlayer { get; private set; }
    /// <summary>
    /// 상대방
    /// </summary>
    public Player enemyPlayer { get; private set; }


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("On Connected To Master");
        PhotonNetwork.JoinRandomOrCreateRoom();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("On Joined Lobby");

    }

    public override void OnJoinedRoom()
    {
        Debug.Log("On Joined Room");
        myPlayer = PhotonNetwork.LocalPlayer;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log("New player has entered this room");

        //접속한 플레이어가 내가 조종하는 플레이어가 아니면(당연하겠지만)
        //적 플레이어로 등록
        if (!newPlayer.IsLocal)
            enemyPlayer = newPlayer;
    }
}
