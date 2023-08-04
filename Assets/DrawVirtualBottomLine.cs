using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;

public class DrawVirtualBottomLine : MonoBehaviour
{
    private LineRenderer lineRenderer;
    //private Transform myTransform; // A 객체의 Transform 컴포넌트를 참조할 변수
    public GameObject ground; // 바닥 객체의 Transform 컴포넌트를 참조할 변수
    public GameObject ActualVisual;

    private Vector3 endPoint;

    //Slot일꺼임
    GameObject BeaconObject;
    
    private bool isUnit;

    public Vector3 GetEndPoint(){
        return endPoint;
    }
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
    }

    void Update()
    {
        RaycastHit hit;
        float raycastDistance = 1.0f; // Raycast 거리

        Vector3 startPoint;

        // A 개체의 아래 방향으로 Raycast를 쏩니다.
        if (Physics.Raycast(this.transform.position, -Vector3.up, out hit, raycastDistance))
        {   
            startPoint = transform.position;

            // 충돌한 개체가 B 개체인지 확인합니다.
            if (hit.collider.gameObject == ground)
            {
                // A 개체와 B 개체 사이에 다른 개체가 없다면 A 개체는 B 개체 위에 떠있는 것으로 간주합니다.
                endPoint = new Vector3(transform.position.x, ground.transform.position.y, transform.position.z);
            }
            else{
                endPoint = startPoint;
            }

            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, endPoint);
        }

    }

    // lineObject를 활성하는 함수
    public void ActivateLineObject()
    {
        Debug.Log("Activate Line Object");
        gameObject.SetActive(true);
    }

    // lineObject를 비활성화하는 함수
    public void DeactivateLineObject()
    {
        Debug.Log("deactivate Line Object");
        // MoveOriginObject();
        gameObject.SetActive(false);
    }


}
