using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Oculus.Interaction;
using Photon.Pun;

public enum Team
{
    None,
    Red,
    Blue,
}

public struct ShipIndex
{
    public Team team;
    public int index;

    public ShipIndex(Team team, int index)
    {
        this.team = team;
        this.index = index;
    }

    public void DebugLog()
    {
        Debug.LogFormat("team = {0} index = {1}", team, index);
    }
}

public class SpaceUnit : MonoBehaviourPun, IPunObservable, ITarget
{
    private const int ACTION_POINT_MAX = 2;

    private WorldBoardManager worldBoardManager;
    private SpaceShipManager spaceShipManager;

    private static EventHandler OnAnyActionPointsChanged;

    private MoveAction moveAction;
    private AttackAction attackAction;

    //list로 만들어야함
    private Queue<BaseAction> baseActionQueue = new Queue<BaseAction>();

    private Oculus.Interaction.PointableUnityEventWrapper eventWrapper;
    private InteractableUnityEventWrapper interactableUnityEventWrapper;
    private SpaceShipPlaceToBeacon spaceShipPlaceToBeacon;
    private Vector3 arrivalPoint;
    private Vector3 targetPosition;

    private WorldBoardManager.GameStage currentStage;
    private int actionPoints;

    public DrawVirtualBottomLine drawVirtualBottomLine;

    private Collider myColider;

    private PhotonView pv;

    public ShipIndex myShipIndex;

    public int ActionPoints => actionPoints;

    #region 전투 능력치 (HP / 공격력)

    [Header("전투 능력치 (0이면 우주선 종류별 기본값)")]
    [SerializeField] private int maxHp = 0;
    [SerializeField] private int attackDamage = 0;

    private int hp;
    private bool isDestroyed;
    private int pendingDamage;
    private bool missileArrived;

    public int MaxHp => maxHp;
    public int Hp => hp;
    public int AttackDamage => attackDamage;
    public bool IsDestroyed => isDestroyed;
    /// <summary>배치되어 전투에 참여 중이고 격추되지 않은 우주선</summary>
    public bool IsAliveInBattle => !isDestroyed && myShipIndex.team != Team.None;

    /// <summary>HP가 바뀌거나 격추/리셋될 때</summary>
    public event Action<SpaceUnit> OnHpChanged;

    // 종류별 기본 능력치 (이름 앞부분으로 구분)
    private static readonly (string prefix, int hp, int damage)[] statTable =
    {
        ("Colossal", 8, 3),
        ("Cruiser", 6, 2),
        ("Bomber", 5, 3),
        ("Fighter_Heavy", 5, 2),
        ("Fighter", 4, 2),
        ("Stealth", 3, 3),
        ("Drone", 3, 1),
    };

    private void ApplyDefaultStats()
    {
        int defHp = 4, defDmg = 2;
        foreach (var s in statTable)
        {
            if (name.StartsWith(s.prefix, StringComparison.OrdinalIgnoreCase))
            {
                defHp = s.hp; defDmg = s.damage;
                break;
            }
        }
        if (maxHp <= 0) maxHp = defHp;
        if (attackDamage <= 0) attackDamage = defDmg;
    }

    #endregion

    #region 리셋용 초기 상태

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialVisualPosition;
    private Quaternion initialVisualRotation;
    private Vector3 initialOriginPosition;
    private Quaternion initialOriginRotation;
    private string initialTag;

    #endregion

    public void Awake()
    {
        moveAction = GetComponent<MoveAction>();
        attackAction = GetComponent<AttackAction>();
        myColider = GetComponent<Collider>();
        pv = GetComponent<PhotonView>();

        // 이동·공격·배치·네트워크 구성요소가 없는 말(예: BoardTable/DeleteLater/ShipMeeple 테스트용)은 게임에서 제외
        if (!moveAction || !attackAction || !GetComponent<SpaceShipPlaceToBeacon>() || !pv)
        {
            Debug.LogWarning($"[SpaceUnit] {name}: 이동/공격/배치/PhotonView 구성요소가 없어 게임 말에서 제외합니다.");
            Destroy(this);
            return;
        }

        actionPoints = ACTION_POINT_MAX;
        ApplyDefaultStats();
        hp = maxHp;

        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialTag = gameObject.tag;
        initialized = true;
    }

    // Awake가 정상적으로 끝난(게임 말로 등록된) 우주선만 리셋 대상
    private bool initialized;

    private void Start()
    {
        worldBoardManager = WorldBoardManager.Instance;
        worldBoardManager.OnCurrentStageChanged += OnCurrentStageChangedHandler;
        worldBoardManager.OnCurrentTurnChanged += OnCurrentTurnChangedHandler;

        spaceShipManager = SpaceShipManager.Instance;

        eventWrapper = GetComponent<Oculus.Interaction.PointableUnityEventWrapper>();

        interactableUnityEventWrapper = GetComponent<InteractableUnityEventWrapper>();

        spaceShipPlaceToBeacon = GetComponent<SpaceShipPlaceToBeacon>();

        if (moveAction && moveAction.actualVisualTransform)
        {
            initialVisualPosition = moveAction.actualVisualTransform.position;
            initialVisualRotation = moveAction.actualVisualTransform.rotation;
        }
        if (spaceShipPlaceToBeacon && spaceShipPlaceToBeacon.origin)
        {
            initialOriginPosition = spaceShipPlaceToBeacon.origin.transform.position;
            initialOriginRotation = spaceShipPlaceToBeacon.origin.transform.rotation;
        }

        // 보드 위 HP 바
        ShipHealthBar.Attach(this);
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event to avoid memory leaks
        if (worldBoardManager != null)
        {
            worldBoardManager.OnCurrentStageChanged -= OnCurrentStageChangedHandler;
            worldBoardManager.OnCurrentTurnChanged -= OnCurrentTurnChangedHandler;
        }
    }

    // 턴이 바뀌면 행동력 회복 (없으면 한 번 이동·공격한 우주선은 다시 행동할 수 없음)
    private void OnCurrentTurnChangedHandler(WorldBoardManager.Turn turn)
    {
        actionPoints = ACTION_POINT_MAX;
    }

    #region 이동

    private static Team TurnTeam(WorldBoardManager wbm)
    {
        if (!wbm) return Team.None;
        return wbm.CurrentTurn == WorldBoardManager.Turn.Red ? Team.Red
             : wbm.CurrentTurn == WorldBoardManager.Turn.Blue ? Team.Blue : Team.None;
    }

    /// <summary>
    /// 잡았다 놓으면 호출 (GamePlaying). 규칙에 맞으면 모든 클라이언트에 이동 RPC, 아니면 원위치.
    /// 규칙: 자기 팀 턴에, 그 턴에 아직 아무 우주선도 이동하지 않았을 때 한 척만 이동 가능.
    /// </summary>
    private void Move()
    {
        string reason = null;
        WorldBoardManager wbm = WorldBoardManager.Instance;
        if (!wbm || wbm.CurrentStage != WorldBoardManager.GameStage.GamePlaying) reason = "전투 단계가 아님";
        else if (!IsAliveInBattle) reason = "전투에 참여하지 않은 우주선";
        else if (myShipIndex.team != TurnTeam(wbm)) reason = "상대 팀 턴";
        else if (!IsLocalPlayersTurn()) reason = "내 턴이 아님";
        else if (actionPoints != ACTION_POINT_MAX) reason = "이미 이동함";
        else if (spaceShipManager && spaceShipManager.currentlyMovedSpaceshipIndex.team == myShipIndex.team) reason = "이번 턴에 다른 우주선이 이미 이동함";

        if (reason != null)
        {
            Debug.Log($"[SpaceUnit] {name} 이동 불가: {reason} → 원위치");
            SnapBackToVisual();
            return;
        }

        // 놓은 위치(바닥선 끝점)를 함께 보내야 상대 화면에서도 같은 곳으로 이동함
        pv.RPC(nameof(MovePRC), RpcTarget.All, drawVirtualBottomLine.GetEndPoint());
    }

    /// <summary>잡고 옮겼지만 이동이 무효일 때, 보이는 모델(ActualVisual) 위치로 되돌림</summary>
    private void SnapBackToVisual()
    {
        if (moveAction && moveAction.actualVisualTransform)
            transform.SetPositionAndRotation(moveAction.actualVisualTransform.position, moveAction.actualVisualTransform.rotation);
    }

    [PunRPC]
    private void MovePRC(Vector3 arrival)
    {
        // 상대 클라이언트는 이 우주선을 잡지 않았으므로 말 위치도 맞춰 줌
        transform.position = arrival;
        arrivalPoint = arrival;
        moveAction.StartMoveAction(arrivalPoint);
        Debug.Log("이동한다!!");

        actionPoints -= 1;
    }

    #endregion

    #region 공격 / 피격

    public void Attack(Transform targetTransform)
    {
        if (actionPoints == 1)
        {
            // 공격이 끝나면 바로 턴이 넘어가고 행동력이 회복되므로, 먼저 차감
            actionPoints -= 1;
            attackAction.StartAttakAction(targetTransform);
            Debug.Log("공격한다!!");
        }
        else
        {
            if (actionPoints == 0)
            {
                Debug.Log("액션 끝남");
            }
            else
            {
                Debug.Log("적 없음");
            }
        }
    }

    /// <summary>
    /// 공격받음 (공격 RPC로 모든 클라이언트에서 같은 순서로 호출됨).
    /// 미사일이 맞는 순간(또는 최대 3초 후) 데미지 적용 → 모든 클라이언트 결과 동일.
    /// </summary>
    public void ReceiveAttack(int damage)
    {
        if (isDestroyed) return;
        pendingDamage += damage;
        missileArrived = false;
        StartCoroutine(ApplyDamageWhenHit());
    }

    /// <summary>미사일이 이 우주선에 맞았을 때 (Missile0_02)</summary>
    public void NotifyMissileHit()
    {
        missileArrived = true;
    }

    private IEnumerator ApplyDamageWhenHit()
    {
        float t = 0f;
        while (!missileArrived && t < 3f)
        {
            t += Time.deltaTime;
            yield return null;
        }
        int dmg = pendingDamage;
        pendingDamage = 0;
        if (dmg > 0) ApplyDamage(dmg);
    }

    private void ApplyDamage(int damage)
    {
        if (isDestroyed) return;
        hp = Mathf.Max(0, hp - damage);
        Debug.Log($"[SpaceUnit] {name} 피격 -{damage} → HP {hp}/{maxHp}");
        gameObject.tag = initialTag; // 미사일이 빗나가도 표적 태그 해제
        OnHpChanged?.Invoke(this);
        if (hp <= 0) DestroyShip();
    }

    private void DestroyShip()
    {
        isDestroyed = true;
        Debug.Log($"[SpaceUnit] {name} 격추!");

        Transform visual = moveAction ? moveAction.actualVisualTransform : null;
        Vector3 at = visual ? visual.position : transform.position;
        ShipFx.Explode(at, ShipRadius());

        if (visual) visual.gameObject.SetActive(false); // 하늘의 실제 크기 복제본도 같이 숨겨짐
        if (drawVirtualBottomLine) drawVirtualBottomLine.gameObject.SetActive(false);
        OnHpChanged?.Invoke(this);
        gameObject.SetActive(false);

        if (WorldBoardManager.Instance) WorldBoardManager.Instance.CheckVictory();
    }

    /// <summary>보드 위 모델 반지름 (폭발 크기용)</summary>
    public float ShipRadius()
    {
        Transform visual = moveAction ? moveAction.actualVisualTransform : null;
        Renderer[] rs = (visual ? visual : transform).GetComponentsInChildren<Renderer>();
        bool has = false;
        Bounds b = default;
        foreach (Renderer r in rs)
        {
            if (r is ParticleSystemRenderer || r is LineRenderer || r is TrailRenderer) continue;
            if (!has) { b = r.bounds; has = true; } else b.Encapsulate(r.bounds);
        }
        return has ? Mathf.Max(0.02f, b.extents.magnitude) : 0.05f;
    }

    #endregion

    #region 리셋

    /// <summary>새 게임을 위해 처음 상태로 되돌림 (WorldBoardManager.ResetMatch)</summary>
    public void ResetForNewGame()
    {
        if (!initialized) return; // 제외된 말 / 한 번도 켜지지 않은 오브젝트는 건드리지 않음
        StopAllCoroutines();
        gameObject.SetActive(true);
        isDestroyed = false;
        hp = maxHp;
        pendingDamage = 0;
        missileArrived = false;
        actionPoints = ACTION_POINT_MAX;
        myShipIndex = new ShipIndex();
        gameObject.tag = initialTag;

        transform.SetPositionAndRotation(initialPosition, initialRotation);
        if (moveAction && moveAction.actualVisualTransform)
        {
            moveAction.StopAllCoroutines();
            moveAction.actualVisualTransform.gameObject.SetActive(true);
            moveAction.actualVisualTransform.SetPositionAndRotation(initialVisualPosition, initialVisualRotation);
        }
        if (spaceShipPlaceToBeacon && spaceShipPlaceToBeacon.origin)
            spaceShipPlaceToBeacon.origin.transform.SetPositionAndRotation(initialOriginPosition, initialOriginRotation);
        if (drawVirtualBottomLine) drawVirtualBottomLine.gameObject.SetActive(false);
        if (interactableUnityEventWrapper) interactableUnityEventWrapper.enabled = false;

        OnHpChanged?.Invoke(this);
    }

    private void RevertToDefaultPosition()
    {
        transform.SetPositionAndRotation(initialPosition, initialRotation);
    }

    #endregion

    // 상태가 변할때마다 이벤트에 넣을거 추가
    private void OnCurrentStageChangedHandler(WorldBoardManager.GameStage stage)
    {
        Debug.Log("지금 stage changed: " + stage.ToString());
        if (!this) return;

        switch (stage)
        {
            case WorldBoardManager.GameStage.Standby:
                eventWrapper.WhenUnselect.RemoveListener(RevertToDefaultPosition);
                eventWrapper.WhenUnselect.AddListener(RevertToDefaultPosition);
                if (myColider) myColider.enabled = false;
                break;

            case WorldBoardManager.GameStage.UnitSetting:
                if (myColider) myColider.enabled = true;
                eventWrapper.WhenSelect.RemoveAllListeners();
                eventWrapper.WhenUnselect.RemoveAllListeners();
                //비콘에 배치 하는 함수 추가
                eventWrapper.WhenUnselect.AddListener(spaceShipPlaceToBeacon.MoveToCenter);

                break;

            case WorldBoardManager.GameStage.GamePlaying:
                //비콘에 배치하는 함수 제거
                eventWrapper.WhenUnselect.RemoveListener(spaceShipPlaceToBeacon.MoveToCenter);

                // 두 번째 판부터 중복 등록되지 않도록 먼저 제거
                eventWrapper.WhenSelect.RemoveListener(drawVirtualBottomLine.ActivateLineObject);
                eventWrapper.WhenUnselect.RemoveListener(drawVirtualBottomLine.DeactivateLineObject);
                eventWrapper.WhenUnselect.RemoveListener(Move);
                eventWrapper.WhenSelect.AddListener(drawVirtualBottomLine.ActivateLineObject);
                eventWrapper.WhenUnselect.AddListener(drawVirtualBottomLine.DeactivateLineObject);
                eventWrapper.WhenUnselect.AddListener(Move);
                interactableUnityEventWrapper.enabled = true;

                break;

            case WorldBoardManager.GameStage.EndofGame:
                // 결과 표시 중에는 이동 불가
                eventWrapper.WhenUnselect.RemoveListener(Move);
                break;

            case WorldBoardManager.GameStage.Reseting:

                break;
        }

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(myShipIndex);
        }
        else
        {
            myShipIndex = (ShipIndex)stream.ReceiveNext();
        }
    }

    #region 배치 동기화

    /// <summary>비콘에 배치했을 때 상대 클라이언트에도 같은 비콘에 놓이게 함</summary>
    public void SyncPlacement(ShipIndex beaconIndex)
    {
        if (PhotonNetwork.InRoom && !PhotonNetwork.OfflineMode && PhotonNetwork.CurrentRoom.PlayerCount > 1)
            pv.RPC(nameof(PlaceOnBeaconRPC), RpcTarget.Others, (int)beaconIndex.team, beaconIndex.index);
    }

    [PunRPC]
    private void PlaceOnBeaconRPC(int team, int index)
    {
        foreach (Beacon b in FindObjectsByType<Beacon>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if ((int)b.myIndex.team != team || b.myIndex.index != index) continue;
            PlaceOnBeacon(b);
            return;
        }
    }

    /// <summary>이 클라이언트에서 비콘에 바로 배치 (자동 배치/동기화용)</summary>
    public void PlaceOnBeacon(Beacon b)
    {
        myShipIndex = b.myIndex;
        transform.SetPositionAndRotation(b.transform.position, Quaternion.identity);
        SpaceShipPlaceToBeacon place = spaceShipPlaceToBeacon ? spaceShipPlaceToBeacon : GetComponent<SpaceShipPlaceToBeacon>();
        if (place && place.origin)
            place.origin.transform.SetPositionAndRotation(b.transform.position, Quaternion.identity);
        b.isUnit = true;
    }

    #endregion

    // VR에서 상대 우주선을 선택(InteractableUnityEventWrapper.WhenSelect)하면 호출됨
    public void SetTarget()
    {
        if (myShipIndex.team == Team.None || isDestroyed) return; // 배치 안 된/격추된 말은 대상 아님
        WorldBoardManager wbm = WorldBoardManager.Instance;
        if (!wbm || wbm.CurrentStage != WorldBoardManager.GameStage.GamePlaying) return;
        if (!IsLocalPlayersTurn()) return;         // 상대 턴에 내 우주선을 잡아도 상대 공격이 나가지 않도록
        pv.RPC(nameof(SelectAttackTarget), RpcTarget.All);
    }

    // 2인 접속 중이면 내 팀 턴일 때만 조작 가능. 혼자(PC 테스트/AI 상대)면 항상 허용
    public static bool IsLocalPlayersTurn()
    {
        if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom.PlayerCount <= 1) return true;
        if (!PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("team", out object team)) return true;
        WorldBoardManager.Turn turn = WorldBoardManager.Instance ? WorldBoardManager.Instance.CurrentTurn : WorldBoardManager.Turn.None;
        return (team as string) == turn.ToString();
    }

    [PunRPC]
    public void SelectAttackTarget()
    {
        // 미사일은 "Enemy" 태그에 맞으므로, 공격 성립 전에 태그를 먼저 바꿔 둠
        string prevTag = gameObject.tag;
        ChangeTagToEnemy();
        // 공격 대상이 나라고 전달 → 이번 턴에 이동한 상대 우주선이 있으면 공격
        if (!SpaceShipManager.Instance || !SpaceShipManager.Instance.SelectAttackTarget(myShipIndex))
            gameObject.tag = prevTag; // 자기 우주선을 잡았을 때 등 공격 불성립이면 원래대로
    }

    public void ChangeTagToEnemy()
    {
        gameObject.tag = "Enemy";
    }

    public void SetTargetRPC(Vector3 position)
    {

    }
}
