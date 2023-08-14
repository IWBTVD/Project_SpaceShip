using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Target : MonoBehaviour
{
    private PhotonView pv;
    private SpaceShipManager spaceShipManager;

    private Vector3 target = new Vector3(4.5f,0.7f,-4.5f);

    private void Start() {
        spaceShipManager = SpaceShipManager.Instance;
        pv = GetComponent<PhotonView>();
    }

    public void setTarget(){
        // Vector3 position = transform.position;
        Vector3 position = target;
        if(position != null) pv.RPC(nameof(setTargetRPC), RpcTarget.All, position);
    }

    [PunRPC]
    void setTargetRPC(Vector3 position){
        spaceShipManager.RegisterTarget(position);
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.N)){
            setTarget();
        }
    }

}