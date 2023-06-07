using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rigidbody�� ������� �����̴� ���ּ�
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Spaceship : MonoBehaviour
{
    [Header("�ʼ� �Ҵ� ���")]
    [SerializeField, Tooltip("���ּ� ���־� �Ҵ����ּ���")] private Transform spaceshipVisual;

    [Header("�̵� ���� ����")]
    [SerializeField] private float speed = 2000f;
    [SerializeField] private float rollAmount;
    [SerializeField] private float pitchAmount;
    [SerializeField] private float yawAmount;
    [SerializeField] private float lerpAmount;

    [Header("�ν��� ���ӿ�����Ʈ ��ü")]
    public List<GameObject> boosterObjects;

    private List<Vector3> boosterOriginalLocalScaleList = new();
    //private List<Vector3>

    private Vector3 rotateValue;
    private Vector2 inputVector;

    private Rigidbody rigid;

    private void Awake()
    {

    }

    private void Start()
    {
        rigid = spaceshipVisual.GetComponent<Rigidbody>();

        foreach (GameObject go in boosterObjects)
        {
            Vector3 originalScale = go.transform.localScale;
            boosterOriginalLocalScaleList.Add(originalScale);
        }
    }

    private void FixedUpdate()
    {
        Movement();
    }

    private void Movement()
    {
        Vector3 lerpVector = new Vector3(inputVector.y * pitchAmount, 0 * yawAmount, -inputVector.x * rollAmount);
        //rotateValue = new Vector3(inputVector.y * pitchAmount, 0 * yawAmount, -1 * inputVector.x * rollAmount);
        rotateValue = Vector3.Lerp(rotateValue, lerpVector, lerpAmount * Time.fixedDeltaTime);

        //rotate
        rigid.MoveRotation(rigid.rotation * Quaternion.Euler(rotateValue * Time.fixedDeltaTime));
        //move forward
        rigid.velocity = transform.forward * speed * Time.fixedDeltaTime;
    }

    public void GetRotateVector(Vector2 inputVector)
    {
        this.inputVector = inputVector;
    }

    public void Throttle(float strength)
    {
        float inputStrength = (strength - 50f) / 50f;

        if (inputStrength == 0)
            return;

        speed += inputStrength * 500 * Time.deltaTime;
        if(speed < 0)
        {
            speed = 0f;
        }
        else if(speed > 3000f)
        {
            speed = 3000f;
        }

        //�ӵ� 2000�� �⺻�� : 1
        float boosterScale = speed / 2000f;
        foreach(GameObject go in boosterObjects)
        {
            Vector3 originalScale = boosterOriginalLocalScaleList[boosterObjects.IndexOf(go)];
            go.transform.localScale = originalScale * boosterScale;
        }
    }
}
