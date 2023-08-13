using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using Photon.Pun;
public class SpaceShipManager : MonoBehaviour, IPunObservable
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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //내가 써야되는 것들(내컴퓨터에서 내것)
        if(stream.IsWriting)
        {
            stream.SendNext(target);
        }
        //내가 받아야되는 것들(내컴퓨터에서 다른 플레이어 것)
        if(stream.IsReading)
        {
            target = (Transform)stream.ReceiveNext();
        }
    }

}
