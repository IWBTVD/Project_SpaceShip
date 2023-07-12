using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceshipAI : MonoBehaviour
{
    [SerializeField] private float maxSpeed;
    [SerializeField] private float minSpeed;
    [SerializeField] private float defaultSpeed;

    [SerializeField] private float speedLerpAmount;
    [SerializeField] private float turningForce;

    [SerializeField] private List<Transform> initialWaypoints;

    private Queue<Transform> waypointQueue;

    private Transform currentWaypoint;

    private float prevWaypointDistance;
    private float waypointDistance;
    private bool isComingClose;

    private float speed;
    private float prevRotY;
    private float currRotY;
    private float rotateAmount;
    private float zRotateValue;
    private float turningTime;

    private void Start()
    {
        speed = defaultSpeed;

        turningTime = 1 / turningForce;

        waypointQueue = new Queue<Transform>();
        foreach(Transform t in initialWaypoints)
        {
            waypointQueue.Enqueue(t);
        }
        ChangeWaypoint();
    }

    private void Update()
    {
        CheckWaypoint();
        Rotate();
        //ZAxisRotate();
        Move();
    }

    /// <summary>
    /// 이동할 지점을 설정하는 함수. 호출할 때마다 <see cref="waypointQueue"/>에서 하나씩 꺼내어 다음 목표 지점으로 사용함
    /// </summary>
    private void ChangeWaypoint()
    {
        if(waypointQueue.Count == 0) {
            currentWaypoint = null;
            return;
        }

        Debug.Log("Change Waypoint");
        currentWaypoint = waypointQueue.Dequeue();
        waypointDistance = Vector3.Distance(transform.position, currentWaypoint.position);
        prevWaypointDistance = waypointDistance;

        isComingClose = false;
    }

    /// <summary>
    /// 특정 지점까지 이동했는지 판단함. 선회력의 제한으로 목표지점까지 정확히 이동하는것이 불가능할 수 있기 때문에,
    /// 목표 지점과 거리가 멀어지며 비행하는 경우 해당 지점을 통과하였다고 판단하여 <see cref="ChangeWaypoint()"/>를 실행해서 다음 목표지점을 설정
    /// </summary>
    private void CheckWaypoint()
    {
        if (currentWaypoint == null) return;
        waypointDistance = Vector3.Distance(transform.position, currentWaypoint.position);

        if(waypointDistance >= prevWaypointDistance)
        {
            //비행체가 웨이포인트 지점으로부터 멀어지게 비행하고있음
            if (isComingClose) ChangeWaypoint();
        }
        else
        {
            isComingClose = true;
        }

        prevWaypointDistance = waypointDistance;
    }

    /// <summary>
    /// 목표 지점을 향해 회전하는 코드
    /// </summary>
    private void Rotate()
    {
        if (currentWaypoint == null) return;

        Vector3 targetDir = currentWaypoint.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(targetDir);

        float delta = Quaternion.Angle(transform.rotation, lookRotation);
        if(delta > 0f)
        {
            float lerpAmount = Mathf.SmoothDampAngle(delta, 0.0f, ref rotateAmount, turningTime);
            lerpAmount = 1.0f - (lerpAmount / delta);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, lerpAmount);
        }
    }

    private void ZAxisRotate()
    {

    }

    private void Move()
    {
        transform.Translate(new Vector3(0, 0, speed) * Time.deltaTime);
    }


}
