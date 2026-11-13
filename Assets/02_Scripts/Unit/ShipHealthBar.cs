using UnityEngine;

/// <summary>
/// 보드 위 우주선 머리 위에 뜨는 HP 바 (팀 색). 카메라를 향해 돌아간다.
/// 전투 중(GamePlaying)·결과 화면(EndofGame)에서 배치된 우주선에만 보인다.
/// </summary>
public class ShipHealthBar : MonoBehaviour
{
    private const float Width = 0.07f;
    private const float Height = 0.009f;

    private SpaceUnit unit;
    private Transform fill;
    private Transform background;
    private Material fillMat;
    private Material bgMat;
    private float heightOffset = -1f;
    private Team shownTeam = (Team)(-1);

    public static ShipHealthBar Attach(SpaceUnit unit)
    {
        // 우주선 말은 스케일이 매우 작으므로 자식으로 두지 않고 따라다니게 함
        GameObject go = new GameObject(unit.name + " HP");
        ShipHealthBar bar = go.AddComponent<ShipHealthBar>();
        bar.unit = unit;
        bar.Build();
        return bar;
    }

    private void Build()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (!shader) shader = Shader.Find("Sprites/Default");
        bgMat = new Material(shader);
        fillMat = new Material(shader);
        SetColor(bgMat, new Color(0.05f, 0.05f, 0.05f, 1f));

        background = CreateQuad("Background", bgMat);
        background.localScale = new Vector3(Width + 0.004f, Height + 0.004f, 1f);
        background.localPosition = new Vector3(0f, 0f, 0.0005f); // 카메라 기준 뒤쪽
        fill = CreateQuad("Fill", fillMat);
        fill.localScale = new Vector3(Width, Height, 1f);
    }

    private Transform CreateQuad(string n, Material mat)
    {
        GameObject q = GameObject.CreatePrimitive(PrimitiveType.Quad);
        q.name = n;
        Destroy(q.GetComponent<Collider>());
        MeshRenderer r = q.GetComponent<MeshRenderer>();
        r.sharedMaterial = mat;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;
        q.transform.SetParent(transform, false);
        return q.transform;
    }

    private static void SetColor(Material m, Color c)
    {
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Color")) m.SetColor("_Color", c);
    }

    private void LateUpdate()
    {
        if (!unit)
        {
            Destroy(gameObject);
            return;
        }

        WorldBoardManager wbm = WorldBoardManager.Instance;
        bool show = unit.gameObject.activeInHierarchy && unit.IsAliveInBattle && wbm
                    && (wbm.CurrentStage == WorldBoardManager.GameStage.GamePlaying || wbm.CurrentStage == WorldBoardManager.GameStage.EndofGame);
        if (fill.gameObject.activeSelf != show)
        {
            fill.gameObject.SetActive(show);
            background.gameObject.SetActive(show);
        }
        if (!show) return;

        if (heightOffset < 0f) heightOffset = unit.ShipRadius() + 0.015f;
        if (shownTeam != unit.myShipIndex.team)
        {
            shownTeam = unit.myShipIndex.team;
            SetColor(fillMat, shownTeam == Team.Red ? new Color(1f, 0.25f, 0.2f) : new Color(0.25f, 0.6f, 1f));
        }

        Camera cam = Camera.main;
        transform.position = unit.transform.position + Vector3.up * heightOffset;
        if (cam) transform.rotation = cam.transform.rotation;

        float frac = unit.MaxHp > 0 ? Mathf.Clamp01((float)unit.Hp / unit.MaxHp) : 0f;
        fill.localScale = new Vector3(Mathf.Max(0.0001f, Width * frac), Height, 1f);
        fill.localPosition = new Vector3(-Width * 0.5f + Width * frac * 0.5f, 0f, 0f);
    }

    private void OnDestroy()
    {
        if (fillMat) Destroy(fillMat);
        if (bgMat) Destroy(bgMat);
    }
}
