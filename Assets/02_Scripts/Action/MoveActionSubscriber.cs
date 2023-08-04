using UnityEngine;
using System;

public class MoveActionSubscriber : MonoBehaviour
{
    private bool isMoving = false;

    private void OnEnable()
    {
        BaseAction.OnAnyActionStarted += OnAnyActionStartedHandler;
    }

    private void OnDisable()
    {
        BaseAction.OnAnyActionStarted -= OnAnyActionStartedHandler;
    }

    private void OnAnyActionStartedHandler(object sender, EventArgs e)
    {
        BaseAction action = sender as BaseAction;
        if (action != null && action.GetActionName() == "Move")
        {
            MoveAction moveAction = action as MoveAction;
            if (moveAction != null)
            {
                MoveObject(moveAction);
            }
        }
    }

    private void MoveObject(MoveAction moveAction)
    {
        if (!isMoving)
        {
            isMoving = true;
            Vector3 targetPosition = transform.position + new Vector3(10f, 0f, 0f);
            moveAction.TakeAction(targetPosition, OnMoveActionComplete);
        }
    }

    private void OnMoveActionComplete()
    {
        isMoving = false;
        Debug.Log("Move action completed.");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 targetPosition = transform.position + new Vector3(10f, 0f, 0f);
            Debug.Log(targetPosition);
            StartMoveActionFromPosition(targetPosition);
        }
    }

    public void StartMoveActionFromPosition(Vector3 targetPosition)
    {
        if (!isMoving)
        {
            isMoving = true;
            MoveAction moveAction = new MoveAction();
            moveAction.TakeAction(targetPosition, OnMoveActionComplete);
        }
    }
}
