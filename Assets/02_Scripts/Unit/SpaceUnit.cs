using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceUnit : MonoBehaviour
{
    [Header("필수 할당요소")]
    [SerializeField, Tooltip("우주선 비주얼")] private Transform spaceshipVisual;
    [SerializeField, Tooltip("ActualVisual에서 사용할 마테리얼")] private Material spaceshipMaterial;

    private Transform actualVisual;

    public void Awake()
    {
        
    }

    public void Start()
    {
        //실제 비주얼 생성
        actualVisual = Instantiate(spaceshipVisual, transform.position, transform.rotation);
        actualVisual.GetComponent<MeshRenderer>().material = spaceshipMaterial;

        actualVisual.transform.name = "actualVisual";
    }
    //잡으면 홀로그램을 시각화한다

    //놓았을 경우 이동 액션을 소모하여 말을 옮긴다
}
