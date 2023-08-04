using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MoveAction : BaseAction
{
    public event EventHandler OnStartMoving;
    public event EventHandler OnStopMoving;

    [SerializeField] private float maxMoveDistance = 5f;
    private Vector3 destination;
    public Transform actualVisualTransform;
    
    private float moveDuration = 2f;
    private bool isMoving;

    //잡으면 이동 준비
    //놓으면 이동
    
    private void Start() 
    {
        // actualVisualTransform = transform.Find("ActualVisual")?.gameObject.transform;
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
        ActionStart(onActionComplete);
        Debug.Log("TakeAction active");
        StartCoroutine(MoveTowardsDestination(worldPosition));
    }

    // 적절한지 판단 여기에 구에 닿는지 닿으면 false 안닿으면 true를 반환하게 짜기
    //일단 무시
    public override bool IsValidActionPosition(Vector3 worldPosition)
    {
        return true;
    }

    public override int GetActionPointCost()
    {
        //default action cost
        return 1;
    }

    //worldPosition 값 받고 
    public void StartMoveActionFromPosition(Vector3 worldPosition)
    {
        if (IsValidActionPosition(worldPosition))
        {
            Debug.Log("StartMoveActionFromPosition active");
            TakeAction(worldPosition, onActionComplete);
            OnStartMoving?.Invoke(this, EventArgs.Empty);
          
        }
    }

    public IEnumerator  MoveTowardsDestination(Vector3 worldPosition)
    {
        isMoving = true;

        Vector3 startPosition = actualVisualTransform.position;
        float elapsedTime = 0f;
        Debug.Log("MoveTowardsDestination active");
        while (elapsedTime < moveDuration)
        {
            actualVisualTransform.position = Vector3.Lerp(startPosition, worldPosition, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        actualVisualTransform.position = worldPosition;
        transform.position = actualVisualTransform.position;
        isMoving = false;
        // ActionComplete();
    }

    
}