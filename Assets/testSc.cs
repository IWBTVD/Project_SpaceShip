// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class testSc : MonoBehaviour
// {
//     private WorldBoardManager1 boardManager;

//     // Start is called before the first frame update
//     void Start()
//     {
//         boardManager = WorldBoardManager1.Instance;

//         // Subscribe to the OnCurrentStageChanged event
//         boardManager.OnCurrentStageChanged += OnCurrentStageChangedHandler;
//     }

//     private void OnCurrentStageChangedHandler(WorldBoardManager1.GameStage stage)
//     {
//         Debug.Log("Current stage changed: " + stage.ToString());
//         // You can perform any specific actions based on the current stage here if needed.
//     }

//     private void OnDestroy()
//     {
//         // Unsubscribe from the event to avoid memory leaks
//         if (boardManager != null)
//         {
//             boardManager.OnCurrentStageChanged -= OnCurrentStageChangedHandler;
//         }
//     }

//     private void Update()
//     {
//         if (Input.GetKeyDown(KeyCode.Space))
//         {
//             boardManager.NextStage();
//         }
//     }
// }

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testSc : MonoBehaviour
{
    public float moveSpeed = 4.0f;
    public float rotationSpeed = 3.0f;

    void Update()
    {
        // 비행기를 앞/뒤/좌/우로 움직이기
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 moveDirection = new Vector3(horizontalInput, 0.0f, verticalInput);
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        // 비행기를 아래로 움직이기 (Q 키)
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);
        }

        // 비행기를 위로 움직이기 (E 키)
        if (Input.GetKey(KeyCode.E))
        {
            transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
        }

        // 비행기의 회전 (위치에 따라 조절해야 할 수 있음)
        float rotateInput = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.up, rotateInput * rotationSpeed);
    }
}

