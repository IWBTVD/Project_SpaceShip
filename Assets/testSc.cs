using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testSc : MonoBehaviour
{
    private WorldBoardManager1 boardManager;

    // Start is called before the first frame update
    void Start()
    {
        boardManager = WorldBoardManager1.Instance;

        // Subscribe to the OnCurrentStageChanged event
        boardManager.OnCurrentStageChanged += OnCurrentStageChangedHandler;
    }

    private void OnCurrentStageChangedHandler(WorldBoardManager1.GameStage stage)
    {
        Debug.Log("Current stage changed: " + stage.ToString());
        // You can perform any specific actions based on the current stage here if needed.
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event to avoid memory leaks
        if (boardManager != null)
        {
            boardManager.OnCurrentStageChanged -= OnCurrentStageChangedHandler;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            boardManager.NextStage();
        }
    }
}
