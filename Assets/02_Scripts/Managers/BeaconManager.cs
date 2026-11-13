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

    /// <summary>새 게임: 비콘을 다시 켜고 비어 있는 상태로</summary>
    public void ResetAllBeacon()
    {
        foreach (var beacon in redBeacon) ResetBeacon(beacon);
        foreach (var beacon in blueBeacon) ResetBeacon(beacon);
    }

    private static void ResetBeacon(Beacon beacon)
    {
        if (!beacon) return;
        beacon.isUnit = false;
        if (beacon.spaceshipDetectionVisual) beacon.spaceshipDetectionVisual.SetActive(false);
        beacon.gameObject.SetActive(true);
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
