using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public static PhotonManager Instance { get; private set; }

    private string gameVersion = "1";
    public PlayerSpawner playerSpawner;
    /// <summary>
    /// 나
    /// </summary>
    public Player myPlayer { get; private set; }
    /// <summary>
    /// 상대방
    /// </summary>
    public Player enemyPlayer { get; private set; }


    private GameObject spawnedPlayerPrefab;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 접속에 필요한 정보(게임 버전) 설정
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("On Connected To Master");
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 2 }, null);
    }
    public override void OnJoinedRoom()
    {
        playerSpawner.SpawnPlayer();
    }

}
