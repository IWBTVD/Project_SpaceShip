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

    public void Awake()
    {
        moveAction = GetComponent<MoveAction>();
    }

    public void Start()
    {

    }
}
