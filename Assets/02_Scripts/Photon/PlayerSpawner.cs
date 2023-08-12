using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    private GameObject spawnedPlayerPrefab;

    // Start is called before the first frame update
    void Start()
    {
        //spawnedPlayerPrefab = PhotonNetwork.Instantiate("OVRPlayerController", transform.position, transform.rotation);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        // PhotonNetwork.Destroy(spawnedPlayerPrefab);
    }

    public void SpawnPlayer()
    {
        spawnedPlayerPrefab = PhotonNetwork.Instantiate("InteractionRigOVR-FullSynthetic", transform.position, transform.rotation);
    }
}
