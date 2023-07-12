using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationRotator : MonoBehaviour
{
    [SerializeField] private Transform topPart;
    [SerializeField] private Transform bottomPart;

    [SerializeField] private float topPartRotationSpeed = 10f;
    [SerializeField] private float middlePartRotationSpeed = -5f;


    void Update()
    {
        topPart.Rotate(Vector3.up, topPartRotationSpeed * Time.deltaTime);
        bottomPart.Rotate(Vector3.up, topPartRotationSpeed * Time.deltaTime);

        // �߾Ӻθ� �ð� �ݴ� �������� ȸ��
        transform.Rotate(Vector3.up, middlePartRotationSpeed * Time.deltaTime);
    }
}
