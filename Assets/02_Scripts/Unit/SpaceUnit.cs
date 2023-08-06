using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Oculus.Interaction;
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

    private Oculus.Interaction.PointableUnityEventWrapper eventWrapper;
    private SpaceShipPlaceToBeacon spaceShipPlaceToBeacon;
    private Grabbable grabbable;
    private Vector3 targetPosition;
    private GameObject targetObject;
    private WorldBoardManager.GameStage currentStage;
    private int actionPoints;

    public DrawVirtualBottomLine drawVirtualBottomLine;

    
    public void Awake()
    {
        moveAction = GetComponent<MoveAction>();
        attackAction = GetComponent<AttackAction>();
        

        actionPoints = ACTION_POINT_MAX;
    }

    private void Start() 
    {
        worldBoardManager = WorldBoardManager.Instance;
        worldBoardManager.OnCurrentStageChanged += OnCurrentStageChangedHandler;
        eventWrapper  = GetComponent<Oculus.Interaction.PointableUnityEventWrapper>();
        spaceShipPlaceToBeacon = GetComponent<SpaceShipPlaceToBeacon>();
        grabbable = GetComponent<Grabbable>();
        drawVirtualBottomLine = GetComponent<DrawVirtualBottomLine>();

        Debug.Log(eventWrapper);
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event to avoid memory leaks
        if (worldBoardManager != null)
        {
            worldBoardManager.OnCurrentStageChanged -= OnCurrentStageChangedHandler;
        }
    }


    public void Move()
    {
        targetPosition = drawVirtualBottomLine.GetEndPoint();
        moveAction.StartMoveAction(targetPosition);
        Debug.Log("이동한다~!!!@!@");

        actionPoints -= 1;

    }

    public void Attack()
    {
        if(targetObject != null){
            if(OVRInput.GetDown(OVRInput.Button.One)){
                attackAction.StartAttakAction(targetObject);
                Debug.Log("공격한다!!");

                actionPoints -= 1;
            }
        }
        else{
            Debug.Log("적 없음");
        }
    }

    // 상태가 변할때마다 이벤트에 넣을거 추가
    private void OnCurrentStageChangedHandler(WorldBoardManager.GameStage stage)
    {
        Debug.Log("Current stage changed: " + stage.ToString());
        
        switch (stage)
        {
            case WorldBoardManager.GameStage.Standby:

                break;

            case WorldBoardManager.GameStage.UnitSetting:
                grabbable.isGrabbed = false;

                eventWrapper.WhenUnselect.AddListener(spaceShipPlaceToBeacon.MoveToCenter);
                break;

            case WorldBoardManager.GameStage.GamePlaying:
                eventWrapper.WhenUnselect.RemoveListener(spaceShipPlaceToBeacon.MoveToCenter);
                
                eventWrapper.WhenSelect.AddListener(drawVirtualBottomLine.ActivateLineObject);
                eventWrapper.WhenUnselect.AddListener(drawVirtualBottomLine.DeactivateLineObject);
                eventWrapper.WhenUnselect.AddListener(Move);

                break;

            case WorldBoardManager.GameStage.EndofGame:

                break;

            case WorldBoardManager.GameStage.Reseting:

                break;
        }

    }

}
