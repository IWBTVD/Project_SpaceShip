using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MoveAction : BaseAction
{
    public event EventHandler OnStartMoving;
    public event EventHandler OnStopMoving;


    [SerializeField] private float maxMoveDistance = 5f;
    private Vector3 destination = new Vector3(0f,0f,0f);
    public Transform actualVisualTransform;
     
    private float moveDuration = 2f;
    private bool isMoving;

    private void Start() 
    {
        
    }
    private void Update()
    {
        if (!isActive) return;
    }

    public override string GetActionName()
    {
        return "Move";
    }

    public override void TakeAction(Vector3 worldPosition, Action onActionComplete)
    {
        
    }


    // 적절한지 판단 여기에 구에 닿는지 닿으면 false 안닿으면 true를 반환하게 짜기
    // 일단 무시
    public override bool IsValidActionPosition(Vector3 worldPosition)
    {
        return true;
    }

    public override int GetActionPointCost()
    {
        //default action cost
        return 1;
    }

    private IEnumerator MoveTowardsPosition(Vector3 targetPosition, System.Action onActionComplete)
    {
        float moveSpeed = 5f; // You can adjust the speed here

        while (transform.position != targetPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        ActionComplete();
        onActionComplete?.Invoke();
    }
    
}