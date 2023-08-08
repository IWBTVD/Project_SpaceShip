using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Oculus.Interaction;
public class SpaceUnit : MonoBehaviour
{
    private WorldBoardManager worldBoardManager;
    private TargetManager targetManager;
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
    private InteractableUnityEventWrapper interactableUnityEventWrapper;
    private SpaceShipPlaceToBeacon spaceShipPlaceToBeacon;
    private Vector3 targetPosition;
    private Transform target;
    private WorldBoardManager.GameStage currentStage;
    private int actionPoints;

    public DrawVirtualBottomLine drawVirtualBottomLine;

    private Transform firstTransform;


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
        
        targetManager = TargetManager.Instance;
        targetManager.OnAttackEvent += OnTargetSelected;
        
        eventWrapper  = GetComponent<Oculus.Interaction.PointableUnityEventWrapper>();
        
        interactableUnityEventWrapper = GetComponent<InteractableUnityEventWrapper>();
        
        spaceShipPlaceToBeacon = GetComponent<SpaceShipPlaceToBeacon>();

        firstTransform = transform;
       
        
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event to avoid memory leaks
        if (worldBoardManager != null)
        {
            worldBoardManager.OnCurrentStageChanged -= OnCurrentStageChangedHandler;
        }

        if(targetManager != null)
        {
            targetManager.OnAttackEvent -= OnTargetSelected;
        }
        

    }

    private void Move()
    {
        if(actionPoints == 2){
    
            targetPosition = drawVirtualBottomLine.GetEndPoint();
            moveAction.StartMoveAction(targetPosition);
            Debug.Log("이동한다!!");
            
            actionPoints -= 1;
        }else{
            return;
        }
    }

    private void OnTargetSelected(Transform target)
    {
        this.target = target;
        Attack();
    }
    

    private void Attack()
    {
        if(target != null && actionPoints == 1){
           
            attackAction.StartAttakAction(target);
            Debug.Log("공격한다!!");
            target.tag = "Enemy";
            actionPoints -= 1;
            
        }
        else{
            if(actionPoints == 0){
                Debug.Log("액션 끝남");
            }
            else{
                Debug.Log("적 없음");
            } 
        }
    }

    private void RevertToDefaultPosition()
    {
        transform.position = firstTransform.position;

        Debug.Log(firstTransform.position +" || " + transform.position);
    }

    // 상태가 변할때마다 이벤트에 넣을거 추가
    private void OnCurrentStageChangedHandler(WorldBoardManager.GameStage stage)
    {
        Debug.Log("Current stage changed: " + stage.ToString());
        
        switch (stage)
        {
            case WorldBoardManager.GameStage.Standby:
                eventWrapper.WhenUnselect.AddListener(RevertToDefaultPosition);
                break;

            case WorldBoardManager.GameStage.UnitSetting:
                eventWrapper.WhenUnselect.RemoveAllListeners();
                //비콘에 배치 하는 함수 추가
                eventWrapper.WhenUnselect.AddListener(spaceShipPlaceToBeacon.MoveToCenter);
                break;

            case WorldBoardManager.GameStage.GamePlaying:
                //비콘에 배치하는 함수 제거
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



    // private void Update() {
    //     if(Input.GetKeyDown(KeyCode.K)){
    //         OnTargetSelected(target);
    //     }
    // }
}
