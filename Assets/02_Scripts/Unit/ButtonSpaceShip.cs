using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSpaceShip : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void ExportValue()
    {
        int valueToEnqueue = gameObject.name.ToLower() == "bluebutton" ? 0 : 1;
        WorldBoardManager.Instance.playerQueue.Enqueue(valueToEnqueue);
    }

    void update(){
        if(Input.GetKeyDown(KeyCode.A)){
            Debug.Log(WorldBoardManager.Instance.playerQueue.Peek());
        }
    }
}
