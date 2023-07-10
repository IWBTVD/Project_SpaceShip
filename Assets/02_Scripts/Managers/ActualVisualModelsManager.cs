using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActualVisualModelsManager : MonoBehaviour
{
    public static ActualVisualModelsManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }


    public void RegisterActualVisualOnManager(Transform actualVisual)
    {
        actualVisual.parent = Instance.transform;
    }
}
