using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

public class SpaceShipManager : MonoBehaviour
{


    public Action<Transform> OnAttackEvent;

    private List<GameObject> allShips = new List<GameObject>();
    private static SpaceShipManager instance;
    public static SpaceShipManager Instance
    {
        get { return instance; }
    }

    
    private Transform target;

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

    public void RegisterShip(GameObject spaceShip)
    {
        allShips.Add(spaceShip);
    }

    public void RegisterTarget(Transform target)
    {
        this.target = target;
        OnAttackEvent.Invoke(target);
    }

    public Transform GetTarget(){
        return target;
    }

}
