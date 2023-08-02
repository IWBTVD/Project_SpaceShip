      using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeaconManager : MonoBehaviour
{
    // Static reference to the singleton instance
    private static BeaconManager instance;

    // Public property to access the singleton instance
    public static BeaconManager Instance
    {
        get { return instance; }
    }

    // List to store all beacon instances
    private List<SpaceshipPresetBeacon> allBeacons = new List<SpaceshipPresetBeacon>();

    private void Awake()
    {
        // Ensure there's only one instance of BeaconManager
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Add beacon to the list when created
    public void RegisterBeacon(SpaceshipPresetBeacon beacon)
    {
        allBeacons.Add(beacon);
    }

    // Access isUnit values of all beacons
    public List<bool> GetAllBeaconIsUnitValues()
    {
        List<bool> isUnitValues = new List<bool>();
        foreach (var beacon in allBeacons)
        {
            isUnitValues.Add(beacon.isUnit);
        }
        return isUnitValues;
    }
}
