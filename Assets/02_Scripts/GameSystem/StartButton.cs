using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class StartButton : MonoBehaviourPun
{
    [SerializeField] Oculus.Interaction.InteractableUnityEventWrapper eventWrapper;

    [PunRPC]
    public void DeactivateButton()
    {
        gameObject.SetActive(false);
    }

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
        // 타이머 시작을 포톤으로 알림 이 안에 다음 단계로 진입 코드 담겨 있음
        WorldBoardManager.Instance.SetStandByPhase();
        photonView.RPC("DeactivateButton", RpcTarget.All);
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.S)){
            StartGame();
        }
    }
}
