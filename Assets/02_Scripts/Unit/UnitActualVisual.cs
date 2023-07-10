using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitActualVisual : MonoBehaviour
{
    public SpaceUnit spaceUnit;

    private void Awake()
    {
        if(!spaceUnit)
        {
            transform.GetComponentInParent<SpaceUnit>();
        }
    }

    private void Start()
    {
        ActualVisualModelsManager.Instance.RegisterActualVisualOnManager(transform);
    }
}