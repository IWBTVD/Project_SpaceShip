using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 보드(축소 맵) ↔ 실제 크기 우주 동기화 매니저.
///
/// - 보드 가운데 행성(boardAnchor)과 플레이어가 밟고 있는 행성(realAnchor)의 스케일 비로 축척을 자동 계산한다. (현재 씬 기준 약 3321:1)
/// - 수평 위치는 그 축척 그대로, 전투 평면은 플레이어 머리 위 상공(battleAltitude)으로 옮긴다.
///   → 보드에서 가운데 행성 근처에서 싸우면 실제로는 플레이어 머리 바로 위에서 보인다.
/// - 보드 위 물체(우주선 ActualVisual, 미사일, 보드 장식)는 스크립트/콜라이더가 제거된 "비주얼 복제본"을 실제 크기 공간에 만들고 매 프레임 따라가게 한다.
/// - 우주선/미사일은 실제 축척대로 키우면 너무 크거나 작아 보일 수 있으므로, 보기 좋은 크기(배율 + 카메라 기준 시야각 범위)로 보정한다.
/// </summary>
[DefaultExecutionOrder(1000)]
public class ActualVisualModelsManager : MonoBehaviour
{
    public enum MirrorKind
    {
        /// <summary>보드 장식(행성, 소행성, 스테이션) - 실제 축척 그대로</summary>
        Scenery,
        /// <summary>우주선 - 보기 좋은 크기로 보정</summary>
        Ship,
        /// <summary>미사일 등 투사체 - 보기 좋은 크기로 보정</summary>
        Projectile,
    }

    public static ActualVisualModelsManager Instance { get; private set; }

    [Header("기준 행성 (비우면 이름으로 자동 탐색)")]
    [SerializeField, Tooltip("보드 가운데 행성 (MapPlane/Planet (1))")] private Transform boardAnchor;
    [SerializeField, Tooltip("플레이어가 밟고 있는 실제 행성 (PlayerEarth)")] private Transform realAnchor;
    [SerializeField] private string boardAnchorName = "Planet (1)";
    [SerializeField] private string boardAnchorParentName = "MapPlane";
    [SerializeField] private string realAnchorName = "PlayerEarth";

    [Header("축척")]
    [SerializeField, Tooltip("0 이하면 두 행성 스케일 비로 자동 계산")] private float ratioOverride = 0f;
    [SerializeField, Tooltip("지면(행성 꼭대기) 기준 전투 평면 높이(m). 보드 가운데 = 플레이어 머리 바로 위")]
    private float battleAltitude = 900f;

    [Header("우주선 크기 (보기 좋은 크기)")]
    [SerializeField, Tooltip("실제 축척 크기 대비 배율 (1 = 축척 그대로)")] private float shipSizeMultiplier = 0.6f;
    [SerializeField, Tooltip("플레이어 눈 기준 최소 시야각(도) - 멀어도 이 이상 크기로 보임")] private float shipMinAngle = 1.5f;
    [SerializeField, Tooltip("플레이어 눈 기준 최대 시야각(도) - 가까워도 하늘을 다 가리지 않음")] private float shipMaxAngle = 12f;

    [Header("미사일 크기")]
    [SerializeField] private float projectileSizeMultiplier = 0.6f;
    [SerializeField] private float projectileMinAngle = 0.6f;
    [SerializeField] private float projectileMaxAngle = 5f;

    [Header("기타")]
    [SerializeField, Tooltip("보드 장식(MapPlane 자식들)을 실제 공간에도 만들지")] private bool mirrorBoardScenery = true;
    [SerializeField, Tooltip("UnitSetting 단계 전에는 실제 공간 우주선 숨김")] private bool hideShipsBeforeSetting = true;
    [SerializeField, Tooltip("카메라 Far Clip 최소값(m) - 수 km 밖 전투가 잘리지 않도록")] private float minCameraFarClip = 20000f;

    private readonly List<Mirror> mirrors = new();
    private Transform realWorldRoot;
    private Transform cloneHolder; // 비활성 상태라 여기서 복제하면 Awake/OnEnable이 돌지 않음
    private Vector3 realCenter;
    private float ratio = 1f;
    private bool isReady;
    private float nextCameraCheckTime;

    public float Ratio => ratio;
    public bool IsReady => isReady;
    public Transform BoardAnchor => boardAnchor;
    public Transform RealAnchor => realAnchor;
    /// <summary>실제 공간 전투 평면 중심 (보드 가운데 행성에 대응)</summary>
    public Vector3 RealCenter => realCenter;
    /// <summary>플레이어가 서 있는 지면 높이</summary>
    public float GroundY => realCenter.y - battleAltitude;

    private class Mirror
    {
        public Transform source;
        public Transform clone;
        public MirrorKind kind;
        public Transform[] srcChildren;
        public Transform[] dstChildren;
        public ParticleSystem[] srcParticles;
        public ParticleSystem[] dstParticles;
        public float unitRadius; // 스케일 1일 때 반지름(로컬 메쉬 기준)
    }

    private void Awake()
    {
        // 씬에 이 컴포넌트가 두 개(ActualVisualModelsManager 오브젝트, PlayerEarth) 붙어 있음 → 하나만 동작
        if (Instance && Instance != this)
        {
            enabled = false;
            return;
        }
        Instance = this;

        realWorldRoot = new GameObject("RealWorldMirrors").transform;
        cloneHolder = new GameObject("RealWorldMirrors_CloneHolder").transform;
        cloneHolder.gameObject.SetActive(false);

        ResolveAnchors();
    }

    private void Start()
    {
        if (Instance != this) return;
        if (isReady && mirrorBoardScenery)
            MirrorBoardScenery();
    }

    private void OnDestroy()
    {
        if (Instance != this) return;
        Instance = null;
        if (realWorldRoot) Destroy(realWorldRoot.gameObject);
        if (cloneHolder) Destroy(cloneHolder.gameObject);
    }

    public void RegisterActualVisualOnManager(Transform actualVisual)
    {
        // 보드 위 비주얼(축소 모델)은 기존처럼 매니저 밑에서 보드 위를 움직인다.
        actualVisual.SetParent(transform, true);
        // 실제 크기 공간에 따라다니는 복제본 생성
        CreateMirror(actualVisual, MirrorKind.Ship);
    }

    #region 좌표 변환

    private void ResolveAnchors()
    {
        if (!boardAnchor)
        {
            GameObject parent = GameObject.Find(boardAnchorParentName);
            if (parent)
            {
                Transform t = parent.transform.Find(boardAnchorName);
                if (t) boardAnchor = t;
            }
            if (!boardAnchor)
            {
                GameObject go = GameObject.Find(boardAnchorName);
                if (go) boardAnchor = go.transform;
            }
        }
        if (!realAnchor)
        {
            GameObject go = GameObject.Find(realAnchorName);
            if (go) realAnchor = go.transform;
        }

        if (!boardAnchor || !realAnchor)
        {
            Debug.LogWarning($"[ActualVisualModelsManager] 기준 행성을 찾지 못했습니다. board={boardAnchor}, real={realAnchor} → 실제 크기 동기화 비활성");
            isReady = false;
            return;
        }

        ratio = ratioOverride > 0f
            ? ratioOverride
            : realAnchor.lossyScale.x / Mathf.Max(1e-9f, boardAnchor.lossyScale.x);

        // 전투 평면 중심 = 플레이어가 서 있는 지면(보드 테이블이 놓인 바닥) 위 battleAltitude
        // (행성 메쉬 바운딩박스 꼭대기는 지형 돌출 때문에 실제 서 있는 곳보다 높게 잡힘)
        float groundY = boardAnchor.root.position.y;
        realCenter = new Vector3(realAnchor.position.x, groundY + battleAltitude, realAnchor.position.z);

        isReady = true;
        Debug.Log($"[ActualVisualModelsManager] 보드:실제 = 1:{ratio:0.#}, 전투 평면 중심 {realCenter}");
    }

    /// <summary>보드 월드 좌표 → 실제 크기 공간 좌표</summary>
    public Vector3 BoardToReal(Vector3 boardWorldPos)
    {
        return realCenter + (boardWorldPos - boardAnchor.position) * ratio;
    }

    /// <summary>실제 크기 공간 좌표 → 보드 월드 좌표</summary>
    public Vector3 RealToBoard(Vector3 realWorldPos)
    {
        return boardAnchor.position + (realWorldPos - realCenter) / ratio;
    }

    /// <summary>보드 판(MapPlane) 범위 안에 있는지 (수평 기준)</summary>
    public bool IsOnBoard(Vector3 boardWorldPos)
    {
        Transform board = boardAnchor.parent;
        if (!board) return true;
        Renderer r = board.GetComponent<Renderer>();
        if (r)
        {
            Bounds b = r.bounds;
            return boardWorldPos.x >= b.min.x && boardWorldPos.x <= b.max.x
                && boardWorldPos.z >= b.min.z && boardWorldPos.z <= b.max.z;
        }
        Vector3 local = board.InverseTransformPoint(boardWorldPos);
        return Mathf.Abs(local.x) <= 5.5f && Mathf.Abs(local.z) <= 5.5f; // 기본 Plane = 10x10
    }

    #endregion

    #region 미러 생성

    private void MirrorBoardScenery()
    {
        Transform board = boardAnchor.parent;
        if (!board) return;
        foreach (Transform child in board)
        {
            // 가운데 행성은 플레이어가 밟고 있는 PlayerEarth가 이미 그 역할
            if (child == boardAnchor) continue;
            if (!child.gameObject.activeInHierarchy) continue;
            if (child.GetComponentInChildren<Renderer>() == null) continue;
            CreateMirror(child, MirrorKind.Scenery);
        }
    }

    /// <summary>보드 위 물체의 실제 크기 비주얼 복제본을 만든다. 원본이 파괴되면 복제본도 사라진다.</summary>
    public Transform CreateMirror(Transform source, MirrorKind kind)
    {
        if (!isReady || !source) return null;

        GameObject clone = Instantiate(source.gameObject, cloneHolder);
        clone.name = source.name + " (Real)";
        StripToVisual(clone);

        Mirror m = new Mirror
        {
            source = source,
            clone = clone.transform,
            kind = kind,
            srcChildren = source.GetComponentsInChildren<Transform>(true),
            dstChildren = clone.GetComponentsInChildren<Transform>(true),
            srcParticles = source.GetComponentsInChildren<ParticleSystem>(true),
            dstParticles = clone.GetComponentsInChildren<ParticleSystem>(true),
            unitRadius = MeasureUnitRadius(source),
        };

        foreach (ParticleSystem ps in m.dstParticles)
        {
            ParticleSystem.MainModule main = ps.main;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
        }

        clone.transform.SetParent(realWorldRoot, false);
        mirrors.Add(m);
        UpdateMirror(m, GetViewCamera());
        return clone.transform;
    }

    /// <summary>
    /// 보드 위 위치에 대응하는 실제 크기 공간에 이펙트(폭발 등)를 생성한다.
    /// 크기는 우주선과 같은 규칙(축척 × 배율, 카메라 시야각 범위)으로 정한다.
    /// </summary>
    public GameObject SpawnRealEffect(GameObject prefab, Vector3 boardPos, float boardRadius, float prefabRadius, float minAngle, float maxAngle)
    {
        if (!isReady || !prefab) return null;
        Vector3 realPos = BoardToReal(boardPos);
        float radius = boardRadius * ratio * shipSizeMultiplier;
        Camera cam = GetViewCamera();
        if (cam)
        {
            float d = Vector3.Distance(cam.transform.position, realPos);
            float min = d * Mathf.Tan(minAngle * 0.5f * Mathf.Deg2Rad);
            float max = d * Mathf.Tan(maxAngle * 0.5f * Mathf.Deg2Rad);
            radius = Mathf.Clamp(radius, min, Mathf.Max(min, max));
        }
        GameObject go = Instantiate(prefab, realPos, Quaternion.identity, realWorldRoot);
        go.transform.localScale = Vector3.one * (radius / Mathf.Max(1e-6f, prefabRadius));
        return go;
    }

    /// <summary>복제본에서 렌더링 외 요소(스크립트, 물리, 소리, 포톤 등) 제거</summary>
    private static void StripToVisual(GameObject root)
    {
        // RequireComponent로 자동 추가된 컴포넌트는 의존하는 쪽보다 앞에 붙으므로,
        // 오브젝트별로 뒤에서부터 지우면 "다른 컴포넌트가 의존 중" 에러를 피할 수 있다.
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            Component[] comps = t.GetComponents<Component>();
            for (int i = comps.Length - 1; i >= 0; i--)
            {
                Component c = comps[i];
                if (c == null || c is Transform) continue; // null = 깨진(Missing) 스크립트
                if (c is MonoBehaviour || c is Joint || c is Collider || c is Rigidbody
                    || c is AudioSource || c is AudioListener || c is Camera || c is Light
                    || c is Canvas || c is CharacterController || c is UnityEngine.AI.NavMeshAgent)
                {
                    DestroyImmediate(c);
                }
            }
        }
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = root.layer;
    }

    /// <summary>스케일 1 기준 렌더러 반지름</summary>
    private static float MeasureUnitRadius(Transform source)
    {
        Renderer[] renderers = source.GetComponentsInChildren<Renderer>(true);
        bool has = false;
        Bounds bounds = default;
        foreach (Renderer r in renderers)
        {
            if (r is ParticleSystemRenderer || r is LineRenderer || r is TrailRenderer) continue;
            if (!has) { bounds = r.bounds; has = true; }
            else bounds.Encapsulate(r.bounds);
        }
        float scale = Mathf.Max(1e-9f, source.lossyScale.x);
        return has ? bounds.extents.magnitude / scale : 0f;
    }

    #endregion

    #region 매 프레임 동기화

    private void LateUpdate()
    {
        if (!isReady) return;

        if (Time.unscaledTime >= nextCameraCheckTime)
        {
            nextCameraCheckTime = Time.unscaledTime + 1f;
            EnsureCameraFarClip();
        }

        Camera cam = GetViewCamera();
        for (int i = mirrors.Count - 1; i >= 0; i--)
        {
            Mirror m = mirrors[i];
            if (!m.source || !m.clone)
            {
                if (m.clone) Destroy(m.clone.gameObject);
                mirrors.RemoveAt(i);
                continue;
            }
            UpdateMirror(m, cam);
        }
    }

    private void UpdateMirror(Mirror m, Camera cam)
    {
        bool visible = m.source.gameObject.activeInHierarchy && IsOnBoard(m.source.position);
        if (visible && m.kind == MirrorKind.Ship && hideShipsBeforeSetting && WorldBoardManager.Instance
            && WorldBoardManager.Instance.CurrentStage == WorldBoardManager.GameStage.Standby)
            visible = false;

        if (m.clone.gameObject.activeSelf != visible)
            m.clone.gameObject.SetActive(visible);
        if (!visible) return;

        Vector3 realPos = BoardToReal(m.source.position);
        m.clone.SetPositionAndRotation(realPos, m.source.rotation);

        float trueScale = m.source.lossyScale.x * ratio;
        float scale = trueScale;
        switch (m.kind)
        {
            case MirrorKind.Ship:
                scale = NiceScale(trueScale * shipSizeMultiplier, m.unitRadius, realPos, cam, shipMinAngle, shipMaxAngle);
                break;
            case MirrorKind.Projectile:
                scale = NiceScale(trueScale * projectileSizeMultiplier, m.unitRadius, realPos, cam, projectileMinAngle, projectileMaxAngle);
                break;
        }
        m.clone.localScale = Vector3.one * scale;

        // 자식들(회전하는 스테이션 파츠, 미사일 이펙트 등) 로컬 움직임 복사
        int n = Mathf.Min(m.srcChildren.Length, m.dstChildren.Length);
        for (int i = 1; i < n; i++)
        {
            Transform s = m.srcChildren[i];
            Transform d = m.dstChildren[i];
            if (!s || !d) continue;
            d.SetLocalPositionAndRotation(s.localPosition, s.localRotation);
            d.localScale = s.localScale;
            if (d.gameObject.activeSelf != s.gameObject.activeSelf)
                d.gameObject.SetActive(s.gameObject.activeSelf);
        }

        int pn = Mathf.Min(m.srcParticles.Length, m.dstParticles.Length);
        for (int i = 0; i < pn; i++)
        {
            ParticleSystem s = m.srcParticles[i];
            ParticleSystem d = m.dstParticles[i];
            if (!s || !d) continue;
            if (s.isPlaying && !d.isPlaying) d.Play(false);
            else if (!s.isPlaying && d.isPlaying) d.Stop(false);
        }
    }

    /// <summary>카메라에서 본 시야각이 [minAngle, maxAngle] 안에 들도록 스케일 보정</summary>
    private static float NiceScale(float baseScale, float unitRadius, Vector3 realPos, Camera cam, float minAngle, float maxAngle)
    {
        if (!cam || unitRadius <= 0f) return baseScale;
        float dist = Vector3.Distance(cam.transform.position, realPos);
        if (dist <= 0.01f) return baseScale;
        float minScale = dist * Mathf.Tan(minAngle * 0.5f * Mathf.Deg2Rad) / unitRadius;
        float maxScale = dist * Mathf.Tan(maxAngle * 0.5f * Mathf.Deg2Rad) / unitRadius;
        return Mathf.Clamp(baseScale, minScale, Mathf.Max(minScale, maxScale));
    }

    private static Camera GetViewCamera()
    {
        Camera cam = Camera.main;
        if (cam && cam.isActiveAndEnabled) return cam;
        foreach (Camera c in Camera.allCameras)
            if (c.isActiveAndEnabled && c.targetTexture == null) return c;
        return null;
    }

    private void EnsureCameraFarClip()
    {
        foreach (Camera c in Camera.allCameras)
        {
            if (c.targetTexture != null) continue;
            if (c.farClipPlane < minCameraFarClip) c.farClipPlane = minCameraFarClip;
        }
    }

    #endregion
}
