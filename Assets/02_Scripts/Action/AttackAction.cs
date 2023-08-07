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
    [SerializeField] private MissileLauncher missileLauncher;
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
        ActionStart(onActionComplete);
    }

    // 적절한지 판단 여기에 구에 닿는지 닿으면 false 안닿으면 true를 반환하게 짜기
    //일단 무시
    public override bool IsValidActionPosition(Vector3 worldPosition)
    {
        return true;
    }

    public void StartAttakAction(Transform Enemy)
    {
        missileLauncher.target = Enemy;
        Debug.Log("StartAttakAction 안의 Enemy임 " + Enemy);
        missileLauncher.MissileLaunch();
        TakeAction(Enemy.position, WhenTurnEnd);
    }

    private void WhenTurnEnd(){
        worldBoardManager.nextTurn();
    }

}
