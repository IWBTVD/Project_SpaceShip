using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SpaceUnit : MonoBehaviour
{
    private WorldBoardManager worldBoardManager;
    private const int ACTION_POINT_MAX = 2;
    //[Header("필수 할당요소")]
    //[SerializeField, Tooltip("우주선 비주얼")] private Transform spaceshipVisual;
    //[SerializeField, Tooltip("ActualVisual에서 사용할 마테리얼")] private Material spaceshipMaterial;
    //
    //private Transform actualVisual;

    private static EventHandler OnAnyActionPointsChanged;

    private MoveAction moveAction;
    private AttackAction attackAction;

    //list로 만들어야함
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
        // eventWrapper  = GetComponent<Oculus.Interaction.PointableUnityEventWrapper>();
        // eventWrapper.WhenUnselect.RemoveAllListeners();

        actionPoints = ACTION_POINT_MAX;
    }

    private void Start() 
    {
        worldBoardManager = WorldBoardManager.Instance;
    }

    public void Move()
    {
        targetPosition = drawVirtualBottomLine.GetEndPoint();
        moveAction.StartMoveActionFromPosition(targetPosition);
        Debug.Log("이동한다~!!!@!@");

        actionPoints -= 1;

    }

    public void Attack()
    {
        
        actionPoints -= 1;
    }
    

}
