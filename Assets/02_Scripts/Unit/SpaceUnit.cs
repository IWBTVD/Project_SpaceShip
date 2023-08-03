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


    private BaseAction[] baseActionArray;

    private Vector3 targetPosition = new Vector3(0f, 1f, 0f);

    public DrawVirtualBottomLine drawVirtualBottomLine;

    public void Awake()
    {
        moveAction = GetComponent<MoveAction>();
    }

    public void Start()
    {

    }

    // private void Update() {
    //     if(Input.GetKeyDown(KeyCode.U)){
    //         MoveTest();
    //     }
    // }
    public void Move(){
        targetPosition = drawVirtualBottomLine.GetEndPoint();
        moveAction.StartMoveActionFromPosition(targetPosition);
        Debug.Log("이동한다~!!!@!@");
    }

    // void MoveTest(){
    //     moveAction.StartMoveActionFromPosition(targetPosition);
    //     Debug.Log("이동한다~!!!@!@");
    // }
    
        
}
