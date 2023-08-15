using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Photon.Pun;

public class WorldBoardManager : MonoBehaviourPun, IPunObservable
{

    public enum GameStage
    {
        Standby, //이때는 둘다 로그인중
        UnitSetting, // 제한시간안에 말을 세팅하는 단계
        GamePlaying, // 게임을 플레이하는 단계
        EndofGame, // 게임이 종료 되고 결과를 나타내는 단계
        Reseting, // 게임을 초기화하고 다음 게임을 준비하는 단계
    }

    // 현재 유저 턴 상태
    public enum Turn { None, Red, Blue }

    public bool isTimerStart = false;
    [SerializeField] private TMP_Text uiText;
    /// <summary>
    /// 준비 시간
    /// </summary>
    public float prepareTime = 120f;
    /// <summary>
    /// 현재 남아있는 시간
    /// </summary>
    public float remainedTime = 0f;

    /// <summary>
    /// 방장이 준비되었는지(내가 준비되었는지가 아니라 방장이 준비되었는지를 기준으로 함. 멀티플레이기 때문에 '나'를 주체로 하면 코드가 괜히 복잡해질 우려가 있음
    /// </summary>
    public bool isHostReady = false;
    /// <summary>
    /// 방원이 준비되었는지
    /// </summary>
    public bool isMemberReady = false;

    /// <summary>
    /// 누가 먼저 준비를 끝마쳤는가? 1 : 방장, 2 : 방원
    /// </summary>
    public int firstPreparedPlayer = -1;

    public Transform boardPlane;

    public float boardSize = 5f;
    public float worldSize = 100f;

    private GameStage currentStage;
    public event Action<GameStage> OnCurrentStageChanged;

    private Turn currentTurn;
    public Turn CurrentTurn
    {
        get { return currentTurn; }
    }

    public event Action<Turn> OnCurrentTurnChanged;

    /// <summary>
    ///처음에 일정 시간 ex 3분 안에 이번 게임에 사용할 말과 위치 등등 정하기..
    ///
    ///먼저 완료버튼을 누른 순서대로 차례 우선권 갖기
    /// </sumary>


    /*
        각팀 차례 일때
    장기 말을 잡을 수 있게 하는 bool값 ture
    잡았을 때 잡은 물체를 받을 변수 NowGarbObject
    이동후 선택한 상대방 물체를 받을 변수 NowSetTargetObject

    공격이나 타케팅등 해당 이벤트를 처리 (각자 오브젝트에서)


    Card가 펼쳐질 차례를 알려줄 bool값
    Card에서 나온 이벤트를 받을 변수 (ex 적 공격, 유성우, 등..)

    */

    public static WorldBoardManager Instance { get; private set; }

    public PhotonView pv;
    
    void Awake()
    {
        if (null == Instance)
        {
            //이 클래스 인스턴스가 탄생했을 때 전역변수 instance에 게임매니저 인스턴스가 담겨있지 않다면, 자신을 넣어준다.
            Instance = this;

            //씬 전환이 되더라도 파괴되지 않게 한다.
            //gameObject만으로도 이 스크립트가 컴포넌트로서 붙어있는 Hierarchy상의 게임오브젝트라는 뜻이지만,
            //나는 헷갈림 방지를 위해 this를 붙여주기도 한다.
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            //만약 씬 이동이 되었는데 그 씬에도 Hierarchy에 GameMgr이 존재할 수도 있다.
            //그럴 경우엔 이전 씬에서 사용하던 인스턴스를 계속 사용해주는 경우가 많은 것 같다.
            //그래서 이미 전역변수인 instance에 인스턴스가 존재한다면 자신(새로운 씬의 GameMgr)을 삭제해준다.
            Destroy(this.gameObject);
        }

        //처음 시작할때 단계를 설정한다.
        SetGameStage(GameStage.Standby);
    }

    public void InitGame()
    {
        // 각 bool값 초기화
        InitTurnValue();
    }

    public void PauseGame()
    {

    }

    public void ContinueGame()
    {

    }

    public void RestartGame()
    {

    }

    public void StopGame()
    {

    }

    private void Update() {
        if (isTimerStart)
        {
            // timer run
            remainedTime -= Time.deltaTime;
            uiText.text = $"{remainedTime / 60:00}:{remainedTime % 60:00}";

            // Check if the ready time has expired
            if (TimesUp())
                isTimerStart = false;
        }
    }

    public bool TimesUp()
    {
        if (remainedTime <= 0)
        {
            Debug.Log("제한 시간 종료!!!");
            return true;
        }

        else if (isHostReady && isMemberReady)
        {
            uiText.text = "Ready to play";
            Debug.Log("양 측 플레이어가 모두 준비를 마침");
            NextStage(); // 지금은 각자 돌아감 뭔가 서버에서 하면 좋을거 같음
            return true;
        }

        return false;
    }

    /// 상태에 관한 부분
    
    private void TriggerCurrentStageChangedEvent()
    {
        OnCurrentStageChanged?.Invoke(currentStage);
    }
    
    public void SetGameStage(GameStage stage)
    {
        currentStage = stage;

        switch (currentStage)
        {
            case GameStage.Standby:

                break;

            case GameStage.UnitSetting:
                remainedTime = prepareTime;
                isTimerStart = true;

                break;

            case GameStage.GamePlaying:
                //비콘들 전부 비활성화
                BeaconManager.Instance.DisactiveAllBeacon();
                break;

            case GameStage.EndofGame:

                break;

            case GameStage.Reseting:

                break;
        }

        // TriggerCurrentStageChangedEvent();
    }

    public void NextStage()
    {
        switch (currentStage)
        {
            case GameStage.Standby:
                SetGameStage(GameStage.UnitSetting);
                Debug.Log("Current stage: Standby -> Next stage: UnitSetting");
                break;

            case GameStage.UnitSetting:
                SetGameStage(GameStage.GamePlaying);
                Debug.Log("Current stage: UnitSetting -> Next stage: GamePlaying");
                break;

            case GameStage.GamePlaying:
                SetGameStage(GameStage.EndofGame);
                Debug.Log("Current stage: GamePlaying -> Next stage: EndofGame");
                break;

            case GameStage.EndofGame:
                SetGameStage(GameStage.Reseting);
                Debug.Log("Current stage: EndofGame -> Next stage: Reseting");
                break;

            case GameStage.Reseting:
                SetGameStage(GameStage.Standby);
                Debug.Log("Current stage: Reseting -> Next stage: Standby");
                break;
        }

        TriggerCurrentStageChangedEvent();
    }

    public GameStage CurrentStage
    {
        get { return currentStage; }
    }


    
    /// 순서에 관한 부분
   
    private void TriggerCurrentTurnChangedEvent()
    {
        OnCurrentTurnChanged?.Invoke(currentTurn);
    }

    public void SetOrder(int teamColor)
    {   
        // -1 이면 첫번째 누른 사람이니까 해당 번호를 할당
        if(firstPreparedPlayer == -1){
            firstPreparedPlayer = teamColor;
            currentTurn = (Turn)firstPreparedPlayer;
            Debug.Log("현재 순서는 "+CurrentTurn + firstPreparedPlayer);
        }

        switch(teamColor){
            case 1:
                isHostReady = true;
                break;
            case 2:
                isMemberReady = true;
                break;
        }

    }

    public void nextTurn(){

        switch (currentTurn)
        {
            case Turn.None:
                currentTurn = Turn.Red; 
                break;
            case Turn.Red:
                currentTurn = Turn.Blue; 
                break;
            case Turn.Blue:
                currentTurn = Turn.Red; 
                break;
        }

        TriggerCurrentTurnChangedEvent();
    }


    

    
    private void InitTurnValue() 
    {
        currentTurn = Turn.None;

    }

    public void SetStandByPhase()
    {
        //타이머 작동시키는 RPC함수 호출
        photonView.RPC(nameof(StartTimerRPC), RpcTarget.All);
    }

    /// <summary>
    /// 타이머 호출
    /// </summary>
    [PunRPC]
    public void StartTimerRPC()
    {
        Debug.Log("타이머가 작동되기 시작하였음!");
        NextStage();
    }

    /// <summary>
    /// 스탠드바이 페이즈 종료
    /// </summary>
    [PunRPC]
    public void EndStandByPhaseRPC()
    {

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isHostReady);
            stream.SendNext(isMemberReady);
            stream.SendNext(firstPreparedPlayer);
            stream.SendNext(currentTurn);
        }

        else
        {
            isHostReady = (bool)stream.ReceiveNext();
            isMemberReady = (bool)stream.ReceiveNext();
            firstPreparedPlayer = (int)stream.ReceiveNext();
            currentTurn = (Turn)stream.ReceiveNext();
        }
    }
}
