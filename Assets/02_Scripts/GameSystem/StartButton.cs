using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartButton : MonoBehaviour
{
    [SerializeField] Oculus.Interaction.InteractableUnityEventWrapper eventWrapper;

    public void OnEnable()
    {
        //이벤트 구독
        eventWrapper.WhenSelect.AddListener(StartGame);
    }

    public void OnDisable()
    {
        //disable될 때 이벤트 구독 해제
        eventWrapper.WhenSelect.RemoveListener(StartGame);
    }

    public void OnDestroy()
    {
        //destroy될 때 이벤트 구독 해제
        eventWrapper.WhenSelect.RemoveListener(StartGame);
    }

    public void StartGame(){
        WorldBoardManager.Instance.NextStage();
        Destroy(gameObject);
    }
}
