using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetOrderButton : MonoBehaviour
{
    [SerializeField] Oculus.Interaction.InteractableUnityEventWrapper eventWrapper;

    public void OnEnable()
    {
        //이벤트 구독
        eventWrapper.WhenSelect.AddListener(SendButtonNum);
    }

    public void OnDisable()
    {
        //disable될 때 이벤트 구독 해제
        eventWrapper.WhenSelect.RemoveListener(SendButtonNum);
    }

    public void OnDestroy()
    {
        //destroy될 때 이벤트 구독 해제
        eventWrapper.WhenSelect.RemoveListener(SendButtonNum);
    }

    public void SendButtonNum(){
        
        gameObject.SetActive(false);
    }
}
