using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rigidbody를 기반으로 움직이는 우주선
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Spaceship : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float rollAmount;
    [SerializeField] private float pitchAmount;
    [SerializeField] private float yawAmount;
    [SerializeField] private float lerpAmount;

    private Vector3 rotateValue;
    private Vector2 inputVector;

    private Rigidbody rigid;

    private void Start()
    {
        rigid = GetComponent<Rigidbody>();
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
}
