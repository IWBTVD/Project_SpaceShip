using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public interface ITarget
{
    public void SetTarget();

    public void ChangeTagToEnemy();
    void SetTargetRPC(Vector3 position);

}