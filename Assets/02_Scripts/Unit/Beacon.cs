using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beacon : MonoBehaviour
{
    public bool isUnit = false;

    [SerializeField] GameObject spaceshipDetectionVisual;

    private void Start()
    {
        // Register this beacon with the BeaconManager
        BeaconManager.Instance.RegisterBeacon(this);

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("SpaceShip") && !isUnit)
        {
            Debug.Log("Collision with Bullet detected: 1");
            spaceshipDetectionVisual.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("SpaceShip") && !isUnit)
        {
            spaceshipDetectionVisual.SetActive(false);
        }
    }
}
