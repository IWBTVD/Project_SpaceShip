using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

using System;

public class WorldBoardManager1 : MonoBehaviour
{

    public enum GameStage
    {
        Standby, //이때는 둘다 로그인중
        UnitSetting, // 제한시간안에 말을 세팅하는 단계
        GamePlaying, // 게임을 플레이하는 단계
        EndofGame, // 게임이 종료 되고 결과를 나타내는 단계
        Reseting, // 게임을 초기화하고 다음 게임을 준비하는 단계
    }


    private GameStage currentStage;

    // Define an event for broadcasting the current stage
    public event Action<GameStage> OnCurrentStageChanged;


    public static WorldBoardManager1 Instance { get; private set; }


    void Awake()
    {
        if (null == Instance)
        {
           
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            
        }

        //처음 시작할때 단계를 설정한다.
        SetGameStage(GameStage.Standby);
    }

    private void TriggerCurrentStageChangedEvent()
    {
        OnCurrentStageChanged?.Invoke(currentStage);
    }


    public void SetGameStage(GameStage stage)
    {
        currentStage = stage;

        // Perform any specific actions based on the stage
        switch (currentStage)
        {
            case GameStage.Standby:

                break;

            case GameStage.UnitSetting:

                break;

            case GameStage.GamePlaying:
                //비콘들 전부 비활성화
                break;

            case GameStage.EndofGame:

                break;

            case GameStage.Reseting:

                break;
        }

        TriggerCurrentStageChangedEvent();
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



}
