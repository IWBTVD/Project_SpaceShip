using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class TestCode : MonoBehaviourPun
{
    MoveAction moveAction;
    Vector3 testVector = new Vector3(-0.258f, 1.024f, 1.3f);
    private PhotonView pv;
    // Start is called before the first frame update
    void Start()
    {
        moveAction = GetComponent<MoveAction>();
        pv = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            pv.RPC(nameof(MovePRCTest), RpcTarget.All);
        }
    }

    [PunRPC]
    private void MovePRCTest()
    {
        moveAction.StartMoveAction(testVector);
    }
}
