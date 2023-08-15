using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeaconManager : MonoBehaviour
{
    private static BeaconManager instance;

    public static BeaconManager Instance
    {
        get { return instance; }
    }

    [SerializeField] List<Beacon> redBeacon = new ();
    [SerializeField] List<Beacon> blueBeacon = new ();


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        int i = 1;

        foreach (var beacon in redBeacon)
        {
            beacon.myIndex = new ShipIndex(Team.Red, i);
            beacon.myIndex.DebugLog();
            i++;
        }

        i = 1;

        foreach (var beacon in blueBeacon)
        {
            beacon.myIndex = new ShipIndex(Team.Blue, i);
            beacon.myIndex.DebugLog();
            i++;
        }
    }

    public void DisactiveAllBeacon()
    {
        // 모든 비콘 비활성화
        foreach (var beacon in redBeacon)
        {
            beacon.gameObject.SetActive(false);
        }

        // 모든 비콘 비활성화
        foreach (var beacon in blueBeacon)
        {
            beacon.gameObject.SetActive(false);
        }
    }

}
