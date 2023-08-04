using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AttackAction : BaseAction
{
    public event EventHandler OnStartAttacking;
    public event EventHandler OnStopAttacking;

    [SerializeField] private float maxDamage = 5f;
    private Vector3 destination;
    private bool isAttacking;


    public override string GetActionName()
    {
        return "Attack";
    }

    protected override void TakeAction(Vector3 worldPosition, Action onActionComplete)
    {
        throw new NotImplementedException();
    }

}
