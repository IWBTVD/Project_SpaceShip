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

    public void setTarget(){
        Vector3 position = transform.position;
        if(position != null) pv.RPC(nameof(setTargetRPC), RpcTarget.All, position);
    }

    [PunRPC]
    void setTargetRPC(Vector3 position){
        spaceShipManager.RegisterTarget(position);
    }


}