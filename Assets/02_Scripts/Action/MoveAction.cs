using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MoveAction : BaseAction
{
    public event EventHandler OnStartMoving;
    public event EventHandler OnStopMoving;

    [SerializeField] private float maxMoveDistance = 5f;

    private void Update()
    {
        if (!isActive) return;
    }

    public override string GetActionName()
    {
        throw new NotImplementedException();
    }

    public override void TakeAction(Vector3 worldPosition, Action onActionComplete)
    {
        throw new NotImplementedException();
    }
}
