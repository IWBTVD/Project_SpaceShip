using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Beacon : MonoBehaviour
{
    public ShipIndex myIndex;
    public GameObject spaceshipDetectionVisual;
    SpaceUnit su;

    public bool isUnit;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("SpaceShip"))
        {
            spaceshipDetectionVisual.SetActive(true);
            isUnit = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("SpaceShip"))
        {
            spaceshipDetectionVisual.SetActive(false);
            isUnit = false;
        }
    }
}
