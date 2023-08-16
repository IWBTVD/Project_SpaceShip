using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Target : MonoBehaviour
{
    private PhotonView pv;
    private SpaceShipManager spaceShipManager;


    private void Start() {
        spaceShipManager = SpaceShipManager.Instance;
        pv = GetComponent<PhotonView>();
    }

    public void SetTarget(){
        Vector3 position = transform.position;
        gameObject.tag = "Enemy";
        if(position != null) pv.RPC(nameof(SetTargetRPC), RpcTarget.All, position);
    }

    public void ChangeTagToEnemy(){
        gameObject.tag = "Enemy";
    }

    [PunRPC]
    void SetTargetRPC(Vector3 position){
        spaceShipManager.RegisterTarget(position);
    }

}