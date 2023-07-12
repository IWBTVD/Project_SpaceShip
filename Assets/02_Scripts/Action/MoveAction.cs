using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MoveAction : BaseAction
{
    public event EventHandler OnStartMoving;
    public event EventHandler OnStopMoving;

    [SerializeField] private float maxMoveDistance = 5f;

    //잡으면 이동 준비
    //놓으면 이동

    private void Update()
    {
        if (!isActive) return;
    }

    public override string GetActionName()
    {
        return "이동";
    }

    public override void TakeAction(Vector3 worldPosition, Action onActionComplete)
    {
        throw new NotImplementedException();
    }
}
