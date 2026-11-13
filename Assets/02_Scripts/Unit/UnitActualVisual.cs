using UnityEngine;

/// <summary>
/// 우주선 말의 비주얼. 게임 시작 시 말에서 떼어져 ActualVisualModelsManager 밑에서 보드 위를 움직이고,
/// 매니저가 실제 크기 공간에 따라다니는 복제본을 만들어 플레이어 머리 위 하늘에 보여준다.
/// </summary>
public class UnitActualVisual : MonoBehaviour
{
    public SpaceUnit spaceUnit;

    private void Awake()
    {
        if (!spaceUnit)
        {
            spaceUnit = GetComponentInParent<SpaceUnit>();
        }
    }

    private void Start()
    {
        if (ActualVisualModelsManager.Instance)
            ActualVisualModelsManager.Instance.RegisterActualVisualOnManager(transform);
        else
            Debug.LogWarning($"[UnitActualVisual] ActualVisualModelsManager가 씬에 없어 {name}의 실제 크기 비주얼을 만들지 못했습니다.");
    }
}
