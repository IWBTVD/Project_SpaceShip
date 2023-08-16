using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using Photon.Pun;
public class SpaceShipManager : MonoBehaviour, IPunObservable
{


    public Action<Vector3> OnAttackEvent;

    private List<GameObject> allShips = new List<GameObject>();
    private static SpaceShipManager instance;
    public static SpaceShipManager Instance
    {
        get { return instance; }
    }

    
    private Vector3 targetPosition;

    public bool currentHasMovedShip = false;
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
        LogListContents(allShips);
    }

     private void LogListContents(List<GameObject> listToLog) // Change the type to match your list's type
    {
        Debug.Log("List Contents:");
        foreach (var item in listToLog)
        {
            Debug.Log(item.name);
        }
    }

    public void RegisterTarget(Vector3 position)
    {
        targetPosition = position;
        OnAttackEvent.Invoke(targetPosition);
    }

    public Vector3 GetTarget(){
        return targetPosition;
    }

    public void ProtectiveshipAttack(GameObject gameObject){
         foreach (GameObject ship in allShips)
        {
            if (ship != gameObject)
            {
                Collider shipCollider = ship.GetComponent<Collider>();
                if (shipCollider != null)
                {
                    shipCollider.enabled = false;
                }
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //내가 써야되는 것들(내컴퓨터에서 내것)
        if(stream.IsWriting)
        {
            stream.SendNext(targetPosition);
        }
        //내가 받아야되는 것들(내컴퓨터에서 다른 플레이어 것)
        if(stream.IsReading)
        {
            targetPosition = (Vector3)stream.ReceiveNext();
        }
    }

}
