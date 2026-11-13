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

    [Header("턴 / 결과")]
    [Tooltip("한 턴 제한 시간(초). 시간이 지나면 자동으로 턴이 넘어감 (0 이하면 무제한)")]
    public float turnTimeLimit = 60f;
    [Tooltip("승패 결과를 보여주는 시간(초). 이후 자동으로 초기화되어 다시 시작 대기")]
    public float resultShowTime = 10f;

    /// <summary>현재 턴 남은 시간</summary>
    public float turnRemainedTime { get; private set; }
    /// <summary>결과 화면 남은 시간</summary>
    public float resultRemainedTime { get; private set; }
    /// <summary>승리 팀 (None = 무승부/진행 중)</summary>
    public Team Winner { get; private set; } = Team.None;

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

    public static WorldBoardManager Instance { get; private set; }

    public PhotonView pv;

    void Awake()
    {
        if (null == Instance)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
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

    /// <summary>결과 화면을 기다리지 않고 바로 초기화 → 다시 시작 대기</summary>
    public void RestartGame()
    {
        if (currentStage == GameStage.EndofGame) resultRemainedTime = 0f;
    }

    public void StopGame()
    {

    }

    private void Update() {
        switch (currentStage)
        {
            case GameStage.UnitSetting:
                if (isTimerStart)
                {
                    remainedTime -= Time.deltaTime;
                    SetUiText(FormatTime(remainedTime));

                    if (TimesUp())
                        isTimerStart = false;
                }
                break;

            case GameStage.GamePlaying:
                UpdateTurnTimer();
                break;

            case GameStage.EndofGame:
                resultRemainedTime -= Time.deltaTime;
                if (resultRemainedTime <= 0f)
                {
                    NextStage(); // → Reseting (초기화)
                    NextStage(); // → Standby (시작 버튼 다시 누르면 새 게임)
                }
                break;
        }
    }

    private static string FormatTime(float seconds)
    {
        int s = Mathf.Max(0, Mathf.CeilToInt(seconds));
        return $"{s / 60:00}:{s % 60:00}";
    }

    private void SetUiText(string text)
    {
        if (uiText) uiText.text = text;
    }

    public bool TimesUp()
    {
        if (isHostReady && isMemberReady)
        {
            SetUiText("Ready to play");
            Debug.Log("양 측 플레이어가 모두 준비를 마침");
            NextStage(); // 지금은 각자 돌아감 뭔가 서버에서 하면 좋을거 같음
            return true;
        }

        if (remainedTime <= 0)
        {
            // 시간 안에 둘 다 준비하지 않아도 배치된 우주선으로 전투 시작 (예전엔 여기서 멈춰 버렸음)
            Debug.Log("제한 시간 종료!!! → 전투 시작");
            NextStage();
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
                SetUiText("");
                break;

            case GameStage.UnitSetting:
                remainedTime = prepareTime;
                isTimerStart = true;
                Winner = Team.None;

                break;

            case GameStage.GamePlaying:
                isTimerStart = false;
                // 3대3 게임: 빈 비콘은 남은 우주선으로 자동 배치
                AutoFillBeacons();
                //비콘들 전부 비활성화
                if (BeaconManager.Instance) BeaconManager.Instance.DisactiveAllBeacon();
                // 아무도 준비 버튼을 안 누르고 시간이 끝났으면 레드 선공
                if (currentTurn == Turn.None) currentTurn = Turn.Red;
                turnRemainedTime = turnTimeLimit;
                // 한쪽이 우주선을 하나도 배치하지 않았으면 바로 승패 결정 (배치 동기화가 끝날 시간을 둠)
                StartCoroutine(CheckVictoryAfter(1f));
                break;

            case GameStage.EndofGame:
                resultRemainedTime = resultShowTime;
                break;

            case GameStage.Reseting:
                ResetMatch();
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

        TriggerCurrentStageChangedEvent();
    }

    public GameStage CurrentStage
    {
        get { return currentStage; }
    }

    #region 3대3 자동 배치

    /// <summary>
    /// 팀마다 비콘 3개 = 우주선 3척. 배치 시간이 끝났을 때 비어 있는 비콘에 남은 우주선을 채운다.
    /// 모든 클라이언트가 같은 순서(비콘 팀·번호, 우주선 이름)로 채우므로 결과가 같다.
    /// </summary>
    private void AutoFillBeacons()
    {
        List<Beacon> beacons = new List<Beacon>();
        foreach (Beacon b in FindObjectsByType<Beacon>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (b.myIndex.team != Team.None) beacons.Add(b);
        beacons.Sort((a, b) => a.myIndex.team != b.myIndex.team ? a.myIndex.team.CompareTo(b.myIndex.team) : a.myIndex.index.CompareTo(b.myIndex.index));

        List<SpaceUnit> free = new List<SpaceUnit>();
        HashSet<(Team, int)> occupied = new HashSet<(Team, int)>();
        foreach (SpaceUnit su in FindObjectsByType<SpaceUnit>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (su.myShipIndex.team == Team.None) free.Add(su);
            else occupied.Add((su.myShipIndex.team, su.myShipIndex.index));
        }
        free.Sort((a, b) => string.CompareOrdinal(a.name, b.name));

        int filled = 0;
        foreach (Beacon b in beacons)
        {
            if (occupied.Contains((b.myIndex.team, b.myIndex.index))) continue;
            if (free.Count == 0) break;
            SpaceUnit su = free[0];
            free.RemoveAt(0);
            su.PlaceOnBeacon(b);
            filled++;
            Debug.Log($"[3대3] 빈 비콘 {b.myIndex.team}#{b.myIndex.index}에 {su.name} 자동 배치");
        }
        if (filled > 0) Debug.Log($"[3대3] 자동 배치 {filled}척");
    }

    #endregion

    #region 승패

    private IEnumerator CheckVictoryAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        CheckVictory();
    }

    /// <summary>한 팀의 우주선이 모두 격추되면(또는 배치되지 않았으면) 게임 종료. 우주선 격추 시 호출됨.</summary>
    public void CheckVictory()
    {
        if (currentStage != GameStage.GamePlaying) return;

        int red = 0, blue = 0;
        foreach (SpaceUnit su in FindObjectsByType<SpaceUnit>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (!su.IsAliveInBattle) continue;
            if (su.myShipIndex.team == Team.Red) red++;
            else if (su.myShipIndex.team == Team.Blue) blue++;
        }
        if (red > 0 && blue > 0) return;

        EndGame(red > 0 ? Team.Red : blue > 0 ? Team.Blue : Team.None);
    }

    private void EndGame(Team winner)
    {
        Winner = winner;
        NextStage(); // GamePlaying → EndofGame
        string text = winner == Team.None ? "DRAW" : $"{winner.ToString().ToUpper()} WINS!";
        SetUiText(text);
        Debug.Log($"게임 종료: {text}");
    }

    #endregion

    #region 초기화

    /// <summary>모든 우주선·비콘·버튼·준비 상태를 처음으로 되돌림 (Reseting 단계)</summary>
    private void ResetMatch()
    {
        foreach (Missile0_02 m in FindObjectsByType<Missile0_02>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            Destroy(m.gameObject);

        foreach (SpaceUnit su in FindObjectsByType<SpaceUnit>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            su.ResetForNewGame();

        if (BeaconManager.Instance) BeaconManager.Instance.ResetAllBeacon();

        foreach (StartButton b in FindObjectsByType<StartButton>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            b.gameObject.SetActive(true);
        foreach (SetOrderButton b in FindObjectsByType<SetOrderButton>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            b.gameObject.SetActive(true);

        if (SpaceShipManager.Instance) SpaceShipManager.Instance.ResetForNewGame();

        isHostReady = false;
        isMemberReady = false;
        firstPreparedPlayer = -1;
        currentTurn = Turn.None;
        remainedTime = 0f;
        isTimerStart = false;
        turnRemainedTime = 0f;
        Winner = Team.None;
        SetUiText("");
        Debug.Log("게임 초기화 완료 → 시작 버튼을 누르면 새 게임");
    }

    #endregion

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

        turnRemainedTime = turnTimeLimit;
        TriggerCurrentTurnChangedEvent();
    }

    #region 턴 제한 시간

    private void UpdateTurnTimer()
    {
        if (turnTimeLimit <= 0f) return;

        turnRemainedTime -= Time.deltaTime;
        SetUiText($"{currentTurn.ToString().ToUpper()} TURN  {FormatTime(turnRemainedTime)}");

        if (turnRemainedTime > 0f) return;
        turnRemainedTime = turnTimeLimit; // 중복 호출 방지 (RPC가 도착하면 다시 설정됨)

        // 방장(또는 혼자)만 판정해서 모두에게 알림
        if (!PhotonNetwork.InRoom)
            SkipTurnRPC((int)currentTurn);
        else if (PhotonNetwork.IsMasterClient)
            photonView.RPC(nameof(SkipTurnRPC), RpcTarget.All, (int)currentTurn);
    }

    /// <summary>제한 시간 초과로 턴 넘김. 이미 넘어간 턴이면 무시</summary>
    [PunRPC]
    public void SkipTurnRPC(int fromTurn)
    {
        if (currentStage != GameStage.GamePlaying || (int)currentTurn != fromTurn) return;
        Debug.Log($"턴 시간 초과 → {currentTurn} 턴 종료");
        nextTurn();
    }

    #endregion

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
        if (currentStage == GameStage.Standby) // 중복 시작 방지
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
