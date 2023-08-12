using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public static PhotonManager Instance { get; private set; }

    [SerializeField] private Transform[] enemySpawnPoints;
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
        SpawnPlayer();

    }

    public void SpawnPlayer()
    {
        spawnedPlayerPrefab = PhotonNetwork.Instantiate("InteractionRigOVR-FullSynthetic", transform.position, transform.rotation);
    }
}
