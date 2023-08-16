using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MoveAction : BaseAction
{
    public event EventHandler OnStartMoving; // 만약 애니메이션등 움직일동안 실행할 이벤트를 담을 변수
    public event EventHandler OnStopMoving;

    [SerializeField] private float maxMoveDistance = 5f;
    private Vector3 destination;
    public Transform actualVisualTransform;
    
    private float moveDuration = 2f;

    // 비행선 움직이는 중인지 아닌지
    private bool isMoving;

    //잡으면 이동 준비
    //놓으면 이동
    
    private void Start() 
    {
        // actualVisualTransform = transform.Find("ActualVisual")?.gameObject.transform;
    }

    public override string GetActionName()
    {
        return "Move";
    }

    // 2
    protected override void TakeAction(Vector3 worldPosition, Action onActionComplete)
    {
        ActionStart(onActionComplete);
        Debug.Log("TakeAction active");
        StartCoroutine(MoveTowardsDestination(worldPosition));
        ActionComplete();
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

    //worldPosition 값 받고  1 유효성 체크후 TakeAction
    public void StartMoveAction(Vector3 worldPosition)
    {
        if (IsValidActionPosition(worldPosition))
        {
            Debug.Log("StartMoveActionFromPosition active");
            TakeAction(worldPosition, Kong);
            OnStartMoving?.Invoke(this, EventArgs.Empty); // 1 용도 우주선이 이동 중임을 알림 
        }
    }


    // 실제로 이동하는 함수 3
    private IEnumerator MoveTowardsDestination(Vector3 worldPosition)
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
        
        ActionComplete();
    }

    private void Kong()// 공
    {
        isMoving = false;
        // SpaceShipManager.Instance.ProtectiveshipAttack(gameObject);
        Debug.Log("이동 종료");
    }
    
}