using UnityEngine;

/// <summary>
/// 우주선 피격/격추 이펙트. 보드 위(축소)와 실제 크기 하늘(ActualVisualModelsManager) 양쪽에 띄운다.
/// 이펙트 프리팹: Resources/ShipExplosion (FORGE3D Explosion_003 기반)
/// </summary>
public static class ShipFx
{
    private const string PrefabPath = "ShipExplosion";
    /// <summary>프리팹 스케일 1일 때 대략적인 폭발 반지름(m)</summary>
    private const float PrefabRadius = 3f;

    private static GameObject prefab;
    private static bool loaded;

    private static GameObject Prefab
    {
        get
        {
            if (!loaded)
            {
                prefab = Resources.Load<GameObject>(PrefabPath);
                loaded = true;
                if (!prefab) Debug.LogWarning("[ShipFx] Resources/" + PrefabPath + " 이펙트 프리팹이 없습니다.");
            }
            return prefab;
        }
    }

    /// <summary>격추 폭발</summary>
    public static void Explode(Vector3 boardPos, float shipRadius)
    {
        Spawn(boardPos, shipRadius * 1.6f, 3f, 2f, 25f);
    }

    /// <summary>미사일 명중 (작은 폭발)</summary>
    public static void Hit(Vector3 boardPos)
    {
        Spawn(boardPos, 0.015f, 2f, 1f, 8f);
    }

    private static void Spawn(Vector3 boardPos, float boardRadius, float lifetime, float skyMinAngle, float skyMaxAngle)
    {
        GameObject p = Prefab;
        if (!p) return;

        // 보드 위
        GameObject board = Object.Instantiate(p, boardPos, Quaternion.identity);
        board.transform.localScale = Vector3.one * (boardRadius / PrefabRadius);
        PlayOnce(board, lifetime);

        // 머리 위 하늘 (실제 크기)
        ActualVisualModelsManager m = ActualVisualModelsManager.Instance;
        if (m && m.IsReady)
        {
            GameObject sky = m.SpawnRealEffect(p, boardPos, boardRadius, PrefabRadius, skyMinAngle, skyMaxAngle);
            // 실제 크기 하늘에서는 큰 폭발이 천천히 퍼지는 느낌으로 절반 속도
            if (sky) PlayOnce(sky, lifetime * 2f, 0.5f);
        }
    }

    private static void PlayOnce(GameObject go, float lifetime, float speed = 1f)
    {
        foreach (ParticleSystem ps in go.GetComponentsInChildren<ParticleSystem>(true))
        {
            ParticleSystem.MainModule main = ps.main;
            main.loop = false;
            main.simulationSpeed = speed;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            // 기본값 PauseAndCatchup이면 첫 프레임에 화면 밖 판정 시 멈췄다가 따라잡으며 파티클이 사라짐
            main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
            ps.Clear(false);
            ps.Play(false);
        }
        Object.Destroy(go, lifetime);
    }
}
