using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBoardManager : MonoBehaviour
{

    public enum GameStage
    {
        Standby, //이때는 둘다 로그인중
        UnitSetting, // 제한시간안에 말을 세팅하는 단계
        GamePlaying, // 게임을 플레이하는 단계
        EndofGame, // 게임이 종료 되고 결과를 나타내는 단계
        Reseting, // 게임을 초기화하고 다음 게임을 준비하는 단계

    }


    public Transform boardPlane;

    public float boardSize = 5f;
    public float worldSize = 100f;

    public bool redturn;
    public bool blueturn;

    private GameStage currentStage;

    /// <summary>
    /*  
    처음에 일정 시간 ex 3분 안에 이번 게임에 사용할 말과 위치 등등 정하기..

    먼저 완료버튼을 누른 순서대로 차례 우선권 갖기
    
    */
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

    public static WorldBoardManager Instance {get; private set;}
    

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
        initTurnValue();
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

    private void initTurnValue(){
        redturn = false;
        blueturn = false;
    }

    
     public void SetGameStage(GameStage stage)
    {
        currentStage = stage;

        // Perform any specific actions based on the stage
        switch (currentStage)
        {
            case GameStage.Standby:
                // Actions for the standby stage
                break;

            case GameStage.UnitSetting:
                // Actions for UnitSetting
                
                break;

            case GameStage.GamePlaying:
                // Actions for GamePlaying
                break;

            case GameStage.EndofGame:
                // Actions for EndofGame
                break;

            case GameStage.Reseting:
                // Actions for Reseting
                break;
        }
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
    }

    public GameStage CurrentStage
    {
        get { return currentStage; }
    }

}
