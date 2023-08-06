using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AttackAction : BaseAction
{
    private WorldBoardManager worldBoardManager;
    public event EventHandler OnStartAttacking;
    public event EventHandler OnStopAttacking;

    [SerializeField] private float maxDamage = 5f;
    private Vector3 destination;
    private bool isAttacking;


    private void Start() {
        worldBoardManager = WorldBoardManager.Instance;
    }
    public override string GetActionName()
    {
        return "Attack";
    }

    protected override void TakeAction(Vector3 worldPosition, Action onActionComplete)
    {
        throw new NotImplementedException();
    }


    public void StartAttakAction(GameObject Enemy)
    {

    }

    private void WhenTurnEnd(){
        worldBoardManager.nextTurn();
    }

}
