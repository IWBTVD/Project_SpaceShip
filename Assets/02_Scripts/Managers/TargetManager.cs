using UnityEngine;
using UnityEngine.Events;
using System;

public class TargetManager : MonoBehaviour
{


    public Action<Transform> OnAttackEvent;
    private static TargetManager instance;
    public static TargetManager Instance
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

    public void RegisterTarget(Transform target)
    {
        this.target = target;
        OnAttackEvent.Invoke(target);
    }

    public Transform GetTarget(){
        return target;
    }

}
