using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spaceship : MonoBehaviour
{
    [Header("필수 할당 요소")]
    [SerializeField, Tooltip("우주선 비주얼 할당해주세용")] private Transform spaceshipVisual;

    [Header("이동 세부 설정")]
    [SerializeField] private float speed = 2000f;
    [SerializeField] private float rollAmount;
    [SerializeField] private float pitchAmount;
    [SerializeField] private float yawAmount;
    [SerializeField] private float lerpAmount;
    [SerializeField] private float accelAmount; //가속력
    [SerializeField] private float maxSpeed = 3001.7f;  //최대 속력
    [SerializeField] private float calibrateAmount = 10f;  //기본 속력 보정값
    [SerializeField, Tooltip("자동 감속 여부")] private bool enableCalibrate = false; //속도 보정을 적용할 것인지 여부
    

    [Header("부스터 게임오브젝트 객체")]
    public List<GameObject> boosterObjects;
    public int boosterGear; //0 ~ 5

    private List<Vector3> boosterOriginalLocalScaleList = new();
    private Vector3 rotateValue;
    private Vector2 inputVector;
    private float speedReciprocal;  //maxSpeed의 역수
    private float defaultSpeed = 600f;

    private Rigidbody rigid;

    private void Awake()
    {

    }

    private void Start()
    {
        rigid = GetComponent<Rigidbody>();

        foreach (GameObject go in boosterObjects)
        {
            Vector3 originalScale = go.transform.localScale;
            boosterOriginalLocalScaleList.Add(originalScale);
        }

        speedReciprocal = 1 / maxSpeed;
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

        float accelEase = (maxSpeed - speed) * speedReciprocal;
        speed += boosterGear * accelAmount * accelEase * Time.fixedDeltaTime;

        //자동 속도 보정 여부
        if(boosterGear == 0 && enableCalibrate)
        {
            speed += (defaultSpeed - speed) * speedReciprocal * calibrateAmount * Time.fixedDeltaTime;
        }

        //move forward
        rigid.velocity = transform.forward * speed;
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

        //속도 2000이 기본값 : 1
        float boosterScale = speed / 2000f;
        foreach(GameObject go in boosterObjects)
        {
            Vector3 originalScale = boosterOriginalLocalScaleList[boosterObjects.IndexOf(go)];
            go.transform.localScale = originalScale * boosterScale;
        }
    }
}
