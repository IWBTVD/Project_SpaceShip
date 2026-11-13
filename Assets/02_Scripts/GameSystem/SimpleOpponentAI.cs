using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// 혼자 테스트할 때 블루 팀을 대신 플레이하는 간단한 AI.
/// (혼자 접속하면 방장 = 레드 팀이므로 AI는 블루 팀)
///
/// - UnitSetting: 남은 우주선 말 중 절반(블루 비콘 수 이하)을 블루 비콘에 배치,
///                레드가 준비되면(또는 남은 시간 10초 이하) 블루 준비
/// - GamePlaying: 블루 턴이 오면 블루 우주선 하나를 가장 가까운 레드 우주선 쪽으로 이동 → 공격 → 턴 넘김
///
/// VR 손으로 잡고 놓을 때와 같은 이벤트(WhenSelect/WhenUnselect)를 호출해서 실제 게임 로직을 그대로 탄다.
/// 다른 플레이어가 방에 들어오면 자동으로 쉰다.
/// </summary>
public class SimpleOpponentAI : MonoBehaviour
{
    public bool aiEnabled = true;
    public WorldBoardManager.Turn aiTeamTurn = WorldBoardManager.Turn.Blue;
    public Team aiTeam = Team.Blue;
    public int aiReadyColor = 2; // SetOrder 번호 (1 레드, 2 블루)

    [Header("행동")]
    [Tooltip("한 번에 이동하는 최대 거리 (보드 기준 m)")] public float maxMoveDistance = 0.25f;
    [Tooltip("목표와 남겨둘 거리 (보드 기준 m)")] public float keepDistance = 0.08f;
    public float thinkDelay = 1.5f;

    public string Status { get; private set; } = "대기";

    private bool placedThisSetting;
    private bool busy;

    private bool OtherHumanInRoom => PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.PlayerCount > 1;

    private void Update()
    {
        WorldBoardManager wbm = WorldBoardManager.Instance;
        if (!aiEnabled || !wbm || busy) return;
        if (OtherHumanInRoom) { Status = "다른 플레이어 접속 중 → AI 쉼"; return; }

        switch (wbm.CurrentStage)
        {
            case WorldBoardManager.GameStage.UnitSetting:
                if (!placedThisSetting)
                    StartCoroutine(PlaceShips());
                // 블루(방원) AI는 레드가 준비하면 따라서 준비 → 레드 선턴. 레드 AI(테스트용)는 배치 후 바로 준비
                else if (!IsAiReady(wbm) && (aiReadyColor == 1 || wbm.isHostReady || wbm.remainedTime <= 10f))
                    ReadyUp(wbm);
                break;

            case WorldBoardManager.GameStage.GamePlaying:
                placedThisSetting = false;
                if (wbm.CurrentTurn == aiTeamTurn)
                    StartCoroutine(TakeTurn());
                else
                    Status = "상대 턴 대기";
                break;

            default:
                placedThisSetting = false;
                Status = "대기 (" + wbm.CurrentStage + ")";
                break;
        }
    }

    private bool IsAiReady(WorldBoardManager wbm) => aiReadyColor == 1 ? wbm.isHostReady : wbm.isMemberReady;

    #region 배치

    private IEnumerator PlaceShips()
    {
        busy = true;
        placedThisSetting = true;
        Status = "우주선 배치 중";
        yield return new WaitForSeconds(thinkDelay);

        List<Beacon> freeBeacons = new();
        foreach (Beacon b in FindObjectsByType<Beacon>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            if (b.myIndex.team == aiTeam && !b.isUnit) freeBeacons.Add(b);
        freeBeacons.Sort((a, b) => a.myIndex.index.CompareTo(b.myIndex.index));

        // 3대3: 자기 팀 비콘 3개를 모두 채움 (우주선 8척이라 상대 몫도 충분히 남음)
        int count = freeBeacons.Count;
        int placed = 0;
        for (int i = 0; i < count; i++)
        {
            // 배치 도중 상대가 가져갈 수 있으므로 매번 새로 고름
            List<SpaceUnit> unplaced = FindUnplaced();
            if (unplaced.Count == 0 || freeBeacons[i].isUnit) continue;
            SpaceUnit unit = unplaced[unplaced.Count - 1];
            claimed.Add(unit);
            yield return PlaceOnBeacon(unit, freeBeacons[i]);
            claimed.Remove(unit);
            placed++;
            yield return new WaitForSeconds(0.4f);
        }

        Status = aiReadyColor == 1 ? $"우주선 {placed}대 배치 완료" : $"우주선 {placed}대 배치 완료, 레드 준비 대기";
        busy = false;
    }

    // 여러 AI/사람이 같은 말을 동시에 집지 않도록
    private static readonly HashSet<SpaceUnit> claimed = new();

    private List<SpaceUnit> FindUnplaced()
    {
        List<SpaceUnit> list = new();
        foreach (SpaceUnit su in FindObjectsByType<SpaceUnit>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            if (su.myShipIndex.team == Team.None && !IsHeldByHuman(su) && !claimed.Contains(su)) list.Add(su);
        list.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
        return list;
    }

    private IEnumerator PlaceOnBeacon(SpaceUnit unit, Beacon beacon)
    {
        Oculus.Interaction.PointableUnityEventWrapper wrapper = GetWrapper(unit);
        if (wrapper) wrapper.WhenSelect.Invoke();

        unit.transform.position = beacon.transform.position;
        // 비콘 트리거(OnTriggerEnter)가 처리될 시간
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        if (wrapper) wrapper.WhenUnselect.Invoke(); // → SpaceShipPlaceToBeacon.MoveToCenter
        yield return null;

        // 트리거가 안 잡혔으면 직접 배치 (MoveToCenter와 같은 결과)
        if (unit.myShipIndex.team != aiTeam)
        {
            unit.myShipIndex = beacon.myIndex;
            unit.transform.SetPositionAndRotation(beacon.transform.position, Quaternion.identity);
            SpaceShipPlaceToBeacon place = unit.GetComponent<SpaceShipPlaceToBeacon>();
            if (place && place.origin)
                place.origin.transform.SetPositionAndRotation(beacon.transform.position, Quaternion.identity);
            beacon.isUnit = true;
        }
    }

    private void ReadyUp(WorldBoardManager wbm)
    {
        foreach (SetOrderButton b in FindObjectsByType<SetOrderButton>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (b.teamColor == aiReadyColor)
            {
                b.SendTeamColor();
                Status = "준비 완료";
                return;
            }
        }
        wbm.SetOrder(aiReadyColor);
        Status = "준비 완료";
    }

    #endregion

    #region 턴 진행

    private IEnumerator TakeTurn()
    {
        busy = true;
        Status = "생각 중";
        yield return new WaitForSeconds(thinkDelay);

        // 상대 미사일이 아직 날아가는 중이면 착탄(데미지 적용)까지 기다렸다가 판단
        float waitMissile = 0f;
        while (FindAnyObjectByType<Missile0_02>() && waitMissile < 4f) { waitMissile += Time.deltaTime; yield return null; }
        yield return new WaitForSeconds(0.2f);

        WorldBoardManager wbm = WorldBoardManager.Instance;
        if (!wbm || wbm.CurrentTurn != aiTeamTurn || wbm.CurrentStage != WorldBoardManager.GameStage.GamePlaying) { busy = false; yield break; }

        List<SpaceUnit> mine = new();
        List<SpaceUnit> enemies = new();
        foreach (SpaceUnit su in FindObjectsByType<SpaceUnit>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (!su.IsAliveInBattle) continue;
            if (su.myShipIndex.team == aiTeam) mine.Add(su);
            else enemies.Add(su);
        }

        if (mine.Count == 0 || enemies.Count == 0)
        {
            Status = mine.Count == 0 ? "배치된 블루 우주선 없음 → 턴 넘김" : "공격할 레드 우주선 없음 → 턴 넘김";
            yield return new WaitForSeconds(0.5f);
            wbm.nextTurn();
            busy = false;
            yield break;
        }

        // 적과 가장 가까운 내 우주선 + 그 적을 고름
        SpaceUnit attacker = null, target = null;
        float best = float.MaxValue;
        foreach (SpaceUnit m in mine)
        {
            if (m.ActionPoints < 2) continue;
            foreach (SpaceUnit e in enemies)
            {
                // 가깝고, HP가 낮아 격추하기 쉬운 적 우선
                float d = Vector3.Distance(m.transform.position, e.transform.position)
                          + 0.05f * Mathf.Max(0, e.Hp - m.AttackDamage);
                if (d < best) { best = d; attacker = m; target = e; }
            }
        }
        if (!attacker)
        {
            Status = "행동 가능한 우주선 없음 → 턴 넘김";
            wbm.nextTurn();
            busy = false;
            yield break;
        }

        // 1) 이동: 잡기 → 목표 쪽으로 옮기기 → 놓기 (MoveAction이 보드 위·실제 공간에서 2초간 이동)
        Status = $"{attacker.name} 이동";
        Vector3 from = attacker.transform.position;
        Vector3 to = target.transform.position;
        Vector3 flat = new Vector3(to.x - from.x, 0f, to.z - from.z);
        float moveDist = Mathf.Clamp(flat.magnitude - keepDistance, 0f, maxMoveDistance);
        // 높이는 그대로 (사람이 공중에서 놓으면 그 높이로 이동하는 규칙이라, 올려서 옮기면 매 턴 위로 쌓임)
        Vector3 dest = from + flat.normalized * moveDist;

        Oculus.Interaction.PointableUnityEventWrapper wrapper = GetWrapper(attacker);
        if (wrapper) wrapper.WhenSelect.Invoke(); // 바닥선 표시
        const float liftTime = 0.6f;
        for (float t = 0f; t < liftTime; t += Time.deltaTime)
        {
            attacker.transform.position = Vector3.Lerp(from, dest, t / liftTime);
            yield return null;
        }
        attacker.transform.position = dest;
        yield return null; // DrawVirtualBottomLine이 도착점 계산
        yield return null;
        if (wrapper) wrapper.WhenUnselect.Invoke(); // → SpaceUnit.Move

        yield return new WaitForSeconds(2.6f); // MoveAction 이동 시간(2초) 대기

        // 2) 공격 (AttackAction 완료 시 WorldBoardManager.nextTurn 호출)
        if (target && attacker.ActionPoints == 1)
        {
            Status = $"{attacker.name} → {target.name} 공격";
            target.SetTarget(); // VR에서 상대 우주선을 선택하는 것과 같은 경로
        }
        else
        {
            Status = "공격 불가 → 턴 넘김";
            wbm.nextTurn();
        }

        // 턴이 넘어갈 때까지 대기
        float wait = 0f;
        while (wbm.CurrentTurn == aiTeamTurn && wait < 3f) { wait += Time.deltaTime; yield return null; }
        if (wbm.CurrentTurn == aiTeamTurn && wbm.CurrentStage == WorldBoardManager.GameStage.GamePlaying) wbm.nextTurn();

        busy = false;
    }

    #endregion

    private static Oculus.Interaction.PointableUnityEventWrapper GetWrapper(SpaceUnit unit)
    {
        var w = unit.GetComponent<Oculus.Interaction.PointableUnityEventWrapper>();
        return w ? w : unit.GetComponentInChildren<Oculus.Interaction.PointableUnityEventWrapper>(true);
    }

    private static bool IsHeldByHuman(SpaceUnit su) =>
        DesktopTestHarness.Instance && DesktopTestHarness.Instance.IsDragging(su);
}
