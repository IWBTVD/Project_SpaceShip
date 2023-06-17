using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float turningForce;
    [SerializeField] float maxSpeed;
    [SerializeField] float accelAmount;
    [SerializeField] float lifetime = 10f;

    private float currentSpeed;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Launch(Transform target, float launchSpeed)
    {
        this.target = target;
        currentSpeed = launchSpeed;
    }

    private void Update()
    {
        if(currentSpeed < maxSpeed)
        {
            currentSpeed += accelAmount * Time.deltaTime;
        }

        transform.Translate(new Vector3(0, 0, currentSpeed * Time.deltaTime));
    }
}
