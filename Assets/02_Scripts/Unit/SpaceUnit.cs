using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SpaceUnit : MonoBehaviour
{
    private const int ACTION_POINT_MAX = 2;
    //[Header("필수 할당요소")]
    //[SerializeField, Tooltip("우주선 비주얼")] private Transform spaceshipVisual;
    //[SerializeField, Tooltip("ActualVisual에서 사용할 마테리얼")] private Material spaceshipMaterial;
    //
    //private Transform actualVisual;

    private static EventHandler OnAnyActionPointsChanged;

    private MoveAction moveAction;
    private AttackAction attackAction;
    private Queue<BaseAction> baseActionQueue = new Queue<BaseAction>();

    // private Oculus.Interaction.PointableUnityEventWrapper eventWrapper;


    private Vector3 targetPosition;
    private GameObject targetObject;

    public DrawVirtualBottomLine drawVirtualBottomLine;

    private int actionPoints;
    public void Awake()
    {
        moveAction = GetComponent<MoveAction>();
        attackAction = GetComponent<AttackAction>();
        baseActionQueue.Enqueue(moveAction);
        baseActionQueue.Enqueue(attackAction);
        // eventWrapper  = GetComponent<Oculus.Interaction.PointableUnityEventWrapper>();
        // eventWrapper.WhenUnselect.RemoveAllListeners();

        actionPoints = ACTION_POINT_MAX;
    }

    public void Move()
    {
        targetPosition = drawVirtualBottomLine.GetEndPoint();
        moveAction.StartMoveActionFromPosition(targetPosition);
        Debug.Log("이동한다~!!!@!@");

        actionPoints = actionPoints -1;

        RemoveActionQueue();
    }

    public void Attack()
    {
        
        actionPoints = actionPoints -1;
    }
    

    public String GetActionNames()
    {
        if(actionPoints > 0)
        {
            string ActionNames = "";

            foreach (BaseAction actionMessage in baseActionQueue)
            {
                ActionNames += actionMessage.GetActionName() + " | ";
            }

            return ActionNames;
        }
        else
        {
            Debug.Log("Queue is empty!");

            return "";
        }
    }

    private void RemoveActionQueue(){
        baseActionQueue.Dequeue();
    }
}
