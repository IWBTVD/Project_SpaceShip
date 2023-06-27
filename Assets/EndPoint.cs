using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndPoint : MonoBehaviour
{

    private Transform ParentTransform;
    private Transform thisTransform;
    private float xPos;
    private float zPos;
    void Start()
    {
        // 부모 객체의 Transform 컴포넌트를 가져옵니다.
        ParentTransform = transform.parent;
        thisTransform = GetComponent<Transform>();
        Debug.Log(ParentTransform.name);
        xPos = ParentTransform.position.x;
        zPos = ParentTransform.position.z;

    }

    void Update()
    {
        Vector3 newPosition = new Vector3(xPos, 1, zPos);
        thisTransform.position = newPosition;
    }
}
