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
        SpawnPlayer();
        
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log("New player has entered this room");

    }

    public void SpawnPlayer()
    {
        ExitGames.Client.Photon.Hashtable teamCustomProperty = new();
        if (PhotonNetwork.LocalPlayer == PhotonNetwork.MasterClient)
        {
            spawnedPlayerPrefab = PhotonNetwork.Instantiate("RedCrew", transform.position, transform.rotation);

            teamCustomProperty.Add("team", "Red");
            PhotonNetwork.LocalPlayer.SetCustomProperties(teamCustomProperty);

            spawnedPlayerPrefab.GetComponent<NetworkedPlayer>().characterTurn = WorldBoardManager.Turn.Red;
        }
        else
        {
            spawnedPlayerPrefab = PhotonNetwork.Instantiate("BlueCrew", transform.position, transform.rotation);

            teamCustomProperty.Add("team", "Blue");
            PhotonNetwork.LocalPlayer.SetCustomProperties(teamCustomProperty);

            spawnedPlayerPrefab.GetComponent<NetworkedPlayer>().characterTurn = WorldBoardManager.Turn.Blue;
        }
    }
}
