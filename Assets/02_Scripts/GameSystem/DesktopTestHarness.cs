using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// 헤드셋 없이 PC(에디터/윈도우 빌드)에서 키보드·마우스로 테스트하기 위한 모드.
/// VR 장치가 활성화되지 않은 상태에서 In-Game 씬이 열리면 자동으로 생성된다. (씬 수정 불필요)
///
/// 조작
///  - 우클릭 드래그: 시점 회전 / WASD: 이동 / Q,E: 아래·위 / Shift: 빠르게 / 휠: 이동 속도
///  - 좌클릭 드래그: 우주선 말 잡기 → 놓기 (VR의 잡기/놓기와 같은 이벤트 호출), 드래그 중 휠: 높이
///  - 우주선 이동 후 적 우주선에 Ctrl+좌클릭: 공격
///  - 1: 테이블 시점 / 2: 행성 위에서 하늘 보기 / 3: 전투 공역 전체 보기
///  - Enter: 게임 시작 / R, B: 레드·블루 준비 / N: 다음 턴 / G: 다음 단계 강제
///  - F2: AI 상대(블루 팀, SimpleOpponentAI) 켜기/끄기 / F1: 도움말 표시/숨김
/// </summary>
public class DesktopTestHarness : MonoBehaviour
{
    public static DesktopTestHarness Instance { get; private set; }

    [Tooltip("온라인 접속이 이 시간(초) 안에 안 되면 Photon 오프라인 모드로 전환 (0 이하면 전환 안 함)")]
    public float offlineFallbackSeconds = 8f;

    private Camera cam;
    private float yaw, pitch;
    private float moveSpeed = 1.5f;
    private bool showHelp = true;

    private SpaceUnit dragging;
    private Oculus.Interaction.PointableUnityEventWrapper dragWrapper;
    private float dragPlaneY;
    private Vector3 dragOffset;
    private SpaceUnit lastMovedUnit;
    private string lastMessage = "";
    private float lastMessageTime;

    #region 자동 생성

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (!Application.isEditor && !Debug.isDebugBuild) return;
        if (XRSettings.isDeviceActive) return;
        if (FindAnyObjectByType<WorldBoardManager>() == null) return; // In-Game 씬에서만
        if (Instance) return;

        new GameObject("DesktopTestHarness").AddComponent<DesktopTestHarness>();
    }

    private void Awake()
    {
        Instance = this;

        GameObject camGo = new GameObject("DesktopTestCamera");
        camGo.transform.SetParent(transform, false);
        camGo.tag = "MainCamera";
        cam = camGo.AddComponent<Camera>();
        cam.nearClipPlane = 0.02f;
        cam.farClipPlane = 30000f;
        cam.depth = 100;
        // VR 카메라처럼 포스트프로세싱(톤매핑·블룸·비네트) 적용
        UnityEngine.Rendering.Universal.CameraExtensions.GetUniversalAdditionalCameraData(cam).renderPostProcessing = true;
        camGo.AddComponent<AudioListener>();

        ai = gameObject.AddComponent<SimpleOpponentAI>();
    }

    private SimpleOpponentAI ai;

    public bool IsDragging(SpaceUnit unit) => dragging == unit;

    private IEnumerator Start()
    {
        Log("PC 테스트 모드 시작 (F1: 도움말)");

        // ActualVisualModelsManager 준비 후 테이블 시점
        yield return null;
        SetView(1);

        // 늦게 XR이 켜지면 PC 모드 종료
        yield return new WaitForSeconds(1f);
        if (XRSettings.isDeviceActive)
        {
            Debug.Log("[DesktopTestHarness] VR 장치가 감지되어 PC 테스트 모드를 끕니다.");
            Destroy(gameObject);
            yield break;
        }

        // 온라인 접속이 안 되면 오프라인 모드로
        if (offlineFallbackSeconds > 0f)
        {
            float t = 0f;
            while (t < offlineFallbackSeconds && !PhotonNetwork.InRoom)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            if (!PhotonNetwork.InRoom && !PhotonNetwork.OfflineMode)
            {
                Log("Photon 온라인 접속 실패 → 오프라인 모드로 전환");
                PhotonNetwork.Disconnect();
                while (PhotonNetwork.IsConnected) yield return null;
                // 오프라인 모드가 되면 OnConnectedToMaster → PhotonManager가 방 생성 → 플레이어 스폰
                PhotonNetwork.OfflineMode = true;
            }
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        // 꺼둔 VR 카메라 복구
        foreach (Camera c in FindObjectsByType<Camera>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            if (c != cam) c.enabled = true;
    }

    #endregion

    private void Update()
    {
        SuppressVrRig();

        if (Input.GetKeyDown(KeyCode.F1)) showHelp = !showHelp;
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetView(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetView(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetView(3);
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) StartGame();
        if (Input.GetKeyDown(KeyCode.F2) && ai)
        {
            ai.aiEnabled = !ai.aiEnabled;
            Log("AI 상대 " + (ai.aiEnabled ? "켜짐" : "꺼짐"));
        }
        if (Input.GetKeyDown(KeyCode.R)) Ready(1);
        if (Input.GetKeyDown(KeyCode.B)) Ready(2);
        if (Input.GetKeyDown(KeyCode.N) && WorldBoardManager.Instance)
        {
            WorldBoardManager.Instance.nextTurn();
            Log("다음 턴: " + WorldBoardManager.Instance.CurrentTurn);
        }
        if (Input.GetKeyDown(KeyCode.G) && WorldBoardManager.Instance)
        {
            WorldBoardManager.Instance.NextStage();
            Log("단계 강제 진행: " + WorldBoardManager.Instance.CurrentStage);
        }

        UpdateFlyCamera();
        UpdatePieceDrag();
    }

    #region 카메라

    private float nextSuppressTime;

    /// <summary>VR 리그 카메라/리스너/키보드 이동을 꺼서 PC 카메라만 쓰게 함</summary>
    private void SuppressVrRig()
    {
        if (Time.unscaledTime < nextSuppressTime) return;
        nextSuppressTime = Time.unscaledTime + 0.5f;

        foreach (Camera c in FindObjectsByType<Camera>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            if (c != cam && c.enabled && c.targetTexture == null) c.enabled = false;
        foreach (AudioListener l in FindObjectsByType<AudioListener>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            if (l.gameObject != cam.gameObject && l.enabled) l.enabled = false;
        // OVRPlayerController도 WASD를 읽기 때문에 끔
        foreach (OVRPlayerController pc in FindObjectsByType<OVRPlayerController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            if (pc.enabled) pc.enabled = false;
    }

    private void UpdateFlyCamera()
    {
        Transform t = cam.transform;

        if (Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * 3f;
            pitch -= Input.GetAxis("Mouse Y") * 3f;
            pitch = Mathf.Clamp(pitch, -89f, 89f);
        }
        t.rotation = Quaternion.Euler(pitch, yaw, 0f);

        if (!dragging)
        {
            float wheel = Input.mouseScrollDelta.y;
            if (Mathf.Abs(wheel) > 0.01f)
                moveSpeed = Mathf.Clamp(moveSpeed * (wheel > 0 ? 1.25f : 0.8f), 0.1f, 5000f);
        }

        Vector3 dir = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) dir += t.forward;
        if (Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.LeftControl)) dir -= t.forward;
        if (Input.GetKey(KeyCode.D)) dir += t.right;
        if (Input.GetKey(KeyCode.A)) dir -= t.right;
        if (Input.GetKey(KeyCode.E)) dir += Vector3.up;
        if (Input.GetKey(KeyCode.Q)) dir -= Vector3.up;
        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? 5f : 1f);
        t.position += dir * speed * Time.unscaledDeltaTime;
    }

    private void SetView(int view)
    {
        ActualVisualModelsManager m = ActualVisualModelsManager.Instance;
        if (!m || !m.IsReady)
        {
            Log("ActualVisualModelsManager가 준비되지 않아 시점 프리셋을 쓸 수 없습니다.");
            return;
        }

        Vector3 board = m.BoardAnchor.position;
        switch (view)
        {
            case 1: // 테이블 시점
                LookFrom(board + new Vector3(0f, 0.9f, -1.1f), board);
                moveSpeed = 1.5f;
                Log("시점: 테이블");
                break;
            case 2: // 플레이어 행성 위에서 하늘 보기
                Vector3 feet = new Vector3(m.RealCenter.x, m.GroundY, m.RealCenter.z);
                // 플레이어 위치 바로 위는 기지 지붕 구조물이 가려서 그 위(20m)에서 올려다봄
                cam.transform.position = feet + Vector3.up * 20f + Vector3.back * 3f;
                pitch = -60f; yaw = 0f;
                moveSpeed = 20f;
                Log("시점: 행성 위에서 하늘 (전투는 머리 위 상공)");
                break;
            case 3: // 전투 공역 전체
                LookFrom(m.RealCenter + new Vector3(0f, 2500f, -5500f), m.RealCenter);
                moveSpeed = 500f;
                Log("시점: 전투 공역 전체");
                break;
        }
    }

    private void LookFrom(Vector3 from, Vector3 target)
    {
        cam.transform.position = from;
        Vector3 e = Quaternion.LookRotation(target - from).eulerAngles;
        pitch = e.x > 180f ? e.x - 360f : e.x;
        yaw = e.y;
    }

    #endregion

    #region 우주선 말 조작

    private void UpdatePieceDrag()
    {
        if (Input.GetMouseButtonDown(0) && !IsMouseOverHelp())
        {
            SpaceUnit unit = PickUnit();
            if (unit)
            {
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                    TryAttack(unit);
                else
                    BeginDrag(unit);
            }
        }

        if (dragging)
        {
            float wheel = Input.mouseScrollDelta.y;
            if (Mathf.Abs(wheel) > 0.01f) dragPlaneY += wheel * 0.01f;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, new Vector3(0f, dragPlaneY, 0f));
            if (plane.Raycast(ray, out float enter))
                dragging.transform.position = ray.GetPoint(enter) + dragOffset;

            if (Input.GetMouseButtonUp(0)) EndDrag();
        }
    }

    private SpaceUnit PickUnit()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 50f, ~0, QueryTriggerInteraction.Collide);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (RaycastHit h in hits)
        {
            SpaceUnit su = h.collider.GetComponentInParent<SpaceUnit>();
            if (su) return su;
        }
        return null;
    }

    private void BeginDrag(SpaceUnit unit)
    {
        dragging = unit;
        dragWrapper = unit.GetComponent<Oculus.Interaction.PointableUnityEventWrapper>();
        if (!dragWrapper) dragWrapper = unit.GetComponentInChildren<Oculus.Interaction.PointableUnityEventWrapper>(true);

        // 비콘이 있으면 비콘 높이 + 약간, 없으면 현재 높이 + 약간 떠서 이동
        dragPlaneY = unit.transform.position.y + 0.03f;
        Beacon beacon = FindAnyObjectByType<Beacon>();
        if (beacon) dragPlaneY = beacon.transform.position.y + 0.03f;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, new Vector3(0f, dragPlaneY, 0f));
        dragOffset = Vector3.zero;
        if (plane.Raycast(ray, out float enter))
        {
            Vector3 p = ray.GetPoint(enter);
            dragOffset = new Vector3(unit.transform.position.x - p.x, 0f, unit.transform.position.z - p.z);
        }

        if (dragWrapper) dragWrapper.WhenSelect.Invoke();
        Log($"잡기: {unit.name}");
    }

    private void EndDrag()
    {
        SpaceUnit unit = dragging;
        dragging = null;
        if (dragWrapper) dragWrapper.WhenUnselect.Invoke();
        dragWrapper = null;

        if (WorldBoardManager.Instance && WorldBoardManager.Instance.CurrentStage == WorldBoardManager.GameStage.GamePlaying)
            lastMovedUnit = unit;
        Log($"놓기: {unit.name}");
    }

    private void TryAttack(SpaceUnit target)
    {
        if (!lastMovedUnit)
        {
            Log("공격: 먼저 GamePlaying 단계에서 내 우주선을 이동시키세요.");
            return;
        }
        if (target == lastMovedUnit)
        {
            Log("공격: 자기 자신은 공격할 수 없습니다.");
            return;
        }
        // VR에서 상대 우주선을 선택할 때와 같은 경로 (SetTarget → RPC → SpaceShipManager → Attack)
        target.SetTarget();
        Log($"공격: {lastMovedUnit.name} → {target.name}");
    }

    #endregion

    #region 게임 진행 단축키

    private void StartGame()
    {
        WorldBoardManager wbm = WorldBoardManager.Instance;
        if (wbm && wbm.CurrentStage == WorldBoardManager.GameStage.EndofGame)
        {
            wbm.RestartGame();
            Log("초기화 → Enter를 한 번 더 누르면 새 게임");
            return;
        }
        StartButton button = FindAnyObjectByType<StartButton>();
        if (!button) { Log("시작 버튼이 없습니다 (이미 시작됨)"); return; }
        if (!PhotonNetwork.InRoom) { Log("아직 Photon 방에 들어가지 않았습니다. 잠시 후 다시 누르세요."); return; }
        button.StartGame();
        Log("게임 시작 → 우주선 배치 단계");
    }

    private void Ready(int teamColor)
    {
        if (!WorldBoardManager.Instance) return;
        foreach (SetOrderButton b in FindObjectsByType<SetOrderButton>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (b.teamColor == teamColor)
            {
                b.SendTeamColor();
                Log((teamColor == 1 ? "레드" : "블루") + " 준비 완료");
                return;
            }
        }
        WorldBoardManager.Instance.SetOrder(teamColor);
        Log((teamColor == 1 ? "레드" : "블루") + " 준비 완료 (버튼 없이 직접 호출)");
    }

    #endregion

    #region 화면 표시

    private void Log(string msg)
    {
        lastMessage = msg;
        lastMessageTime = Time.unscaledTime;
        Debug.Log("[DesktopTestHarness] " + msg);
    }

    private Rect helpRect = new Rect(10, 10, 450, 300);

    private bool IsMouseOverHelp()
    {
        if (!showHelp) return false;
        Vector2 m = Input.mousePosition;
        m.y = Screen.height - m.y;
        return helpRect.Contains(m);
    }

    private void OnGUI()
    {
        GUIStyle label = new GUIStyle(GUI.skin.label) { fontSize = 13, richText = true };

        if (showHelp)
        {
            GUI.Box(helpRect, GUIContent.none);
            GUILayout.BeginArea(new Rect(helpRect.x + 10, helpRect.y + 6, helpRect.width - 20, helpRect.height - 10));
            WorldBoardManager wbm = WorldBoardManager.Instance;
            ActualVisualModelsManager avm = ActualVisualModelsManager.Instance;
            string photon = PhotonNetwork.OfflineMode ? "오프라인" : (PhotonNetwork.InRoom ? $"온라인 방 ({PhotonNetwork.CurrentRoom.PlayerCount}명)" : PhotonNetwork.NetworkClientState.ToString());
            GUILayout.Label($"<b>PC 테스트 모드</b>   Photon: {photon}", label);
            string timeInfo = "-";
            if (wbm)
            {
                switch (wbm.CurrentStage)
                {
                    case WorldBoardManager.GameStage.UnitSetting: timeInfo = $"배치 {wbm.remainedTime:0}초"; break;
                    case WorldBoardManager.GameStage.GamePlaying: timeInfo = $"턴 {wbm.turnRemainedTime:0}초"; break;
                    case WorldBoardManager.GameStage.EndofGame: timeInfo = $"재시작까지 {wbm.resultRemainedTime:0}초"; break;
                }
            }
            GUILayout.Label($"단계: <b>{(wbm ? wbm.CurrentStage.ToString() : "-")}</b>   턴: <b>{(wbm ? wbm.CurrentTurn.ToString() : "-")}</b>   남은 시간: {timeInfo}", label);
            GUILayout.Label($"축척 1:{(avm ? avm.Ratio.ToString("0") : "-")}   마지막 이동 우주선: {(lastMovedUnit ? lastMovedUnit.name : "-")}", label);
            GUILayout.Space(6);
            GUILayout.Label("우클릭 드래그: 시점 / WASD·Q·E: 이동 / Shift: 빠르게 / 휠: 속도", label);
            GUILayout.Label("좌클릭 드래그: 우주선 잡기·놓기 (드래그 중 휠: 높이)", label);
            GUILayout.Label("Ctrl+좌클릭(적 우주선): 마지막 이동한 우주선으로 공격", label);
            GUILayout.Label("1: 테이블   2: 행성 위에서 하늘   3: 전투 공역 전체", label);
            GUILayout.Label("Enter: 게임 시작 (결과 화면에선 바로 재시작)   R/B: 레드·블루 준비", label);
            GUILayout.Label("N: 다음 턴   G: 다음 단계 (디버그)", label);
            GUILayout.Label($"F2: AI 상대(블루) 켜기/끄기 — <b>{(ai && ai.aiEnabled ? "켜짐" : "꺼짐")}</b>: {(ai ? ai.Status : "-")}", label);
            GUILayout.Label("F1: 도움말 숨기기", label);
            GUILayout.EndArea();
        }

        DrawBattleStatus(label);

        if (!string.IsNullOrEmpty(lastMessage) && Time.unscaledTime - lastMessageTime < 4f)
        {
            GUIStyle msg = new GUIStyle(GUI.skin.box) { fontSize = 15, alignment = TextAnchor.MiddleCenter };
            GUI.Box(new Rect(Screen.width * 0.5f - 250, Screen.height - 60, 500, 34), lastMessage, msg);
        }
    }

    /// <summary>오른쪽 위: 양 팀 우주선 HP / 가운데: 승패 배너</summary>
    private void DrawBattleStatus(GUIStyle label)
    {
        WorldBoardManager wbm = WorldBoardManager.Instance;
        if (!wbm) return;
        bool battle = wbm.CurrentStage == WorldBoardManager.GameStage.GamePlaying || wbm.CurrentStage == WorldBoardManager.GameStage.EndofGame;
        if (!battle) return;

        System.Text.StringBuilder red = new System.Text.StringBuilder("<b><color=#ff6655>RED</color></b>\n");
        System.Text.StringBuilder blue = new System.Text.StringBuilder("<b><color=#66aaff>BLUE</color></b>\n");
        SpaceUnit[] units = FindObjectsByType<SpaceUnit>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        System.Array.Sort(units, (a, b) => string.CompareOrdinal(a.name, b.name));
        foreach (SpaceUnit su in units)
        {
            if (su.myShipIndex.team == Team.None) continue;
            string line = su.IsDestroyed ? $"<color=#888888>{su.name}  격추</color>\n" : $"{su.name}  HP {su.Hp}/{su.MaxHp}  (공격 {su.AttackDamage})\n";
            (su.myShipIndex.team == Team.Red ? red : blue).Append(line);
        }
        Rect r = new Rect(Screen.width - 290, 10, 280, 260);
        GUI.Box(r, GUIContent.none);
        GUI.Label(new Rect(r.x + 10, r.y + 6, r.width - 20, r.height - 10), red.ToString() + "\n" + blue.ToString(), label);

        if (wbm.CurrentStage == WorldBoardManager.GameStage.EndofGame)
        {
            string text = wbm.Winner == Team.None ? "무승부" : (wbm.Winner == Team.Red ? "<color=#ff6655>RED 승리!</color>" : "<color=#66aaff>BLUE 승리!</color>");
            GUIStyle big = new GUIStyle(GUI.skin.box) { fontSize = 42, alignment = TextAnchor.MiddleCenter, richText = true };
            GUI.Box(new Rect(Screen.width * 0.5f - 260, Screen.height * 0.35f, 520, 110),
                    $"{text}\n<size=16>{wbm.resultRemainedTime:0}초 후 초기화 (Enter: 바로)</size>", big);
        }
    }

    #endregion
}
