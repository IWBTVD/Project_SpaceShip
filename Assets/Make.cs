using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;

public class Make : MonoBehaviour
{
    private LineRenderer lineRenderer;
    public Transform objectA; // A 객체의 Transform 컴포넌트를 참조할 변수
    public Transform ground; // 바닥 객체의 Transform 컴포넌트를 참조할 변수

    public Grabbable grabbable;
    public GameObject lineObject;
    private bool wasGrabbed = false;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
    }

    void Update()
    {
        Vector3 startPoint = objectA.position;
        Vector3 endPoint = new Vector3(objectA.position.x, ground.position.y, objectA.position.z);

        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);

        bool isGrabbed = grabbable.isGrabbed;

        if (!wasGrabbed && isGrabbed)
        {
            ActivateLineObject();
        }
        else if (wasGrabbed && !isGrabbed) // 추가된 부분: 물체를 놓았을 때 lineObject를 비활성화
        {
            DeactivateLineObject();
            wasGrabbed = false; // 수정된 부분: lineObject를 비활성화할 때 wasGrabbed 변수도 초기화
        }

        wasGrabbed = isGrabbed;
    }

    void ActivateLineObject()
    {
        lineObject.SetActive(true);
    }

    // lineObject를 비활성화하는 함수
    void DeactivateLineObject()
    {
        lineObject.SetActive(false);
    }
}
