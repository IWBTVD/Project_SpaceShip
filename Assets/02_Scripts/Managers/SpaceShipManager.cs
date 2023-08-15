using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using Photon.Pun;
public class SpaceShipManager : MonoBehaviour, IPunObservable
{


    public Action<Vector3> OnAttackEvent;

    private List<SpaceUnit> redShipList;
    private List<SpaceUnit> blueShipList;

    private static SpaceShipManager instance;
    public static SpaceShipManager Instance
    {
        get { return instance; }
    }
    
    private Vector3 targetPosition;

    public int myIndex = 1;

    //방금 움직인 비행선 인덱스
    public ShipIndex currentlyMovedSpaceshipIndex;
    //공격 대상으로 지정된 비행선 인덱스
    public ShipIndex targetedSpaceshipIndex;

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

    public void SelectAttackTarget(ShipIndex victimIndex)
    {
        targetedSpaceshipIndex = victimIndex;
        SpaceUnit attacker = null;

        Transform victimTransform;
        bool amIRed = PhotonNetwork.LocalPlayer.CustomProperties.ContainsValue("Red");
        bool isRedTurn = WorldBoardManager.Instance.CurrentTurn == WorldBoardManager.Turn.Red;
        if (amIRed == isRedTurn){
            foreach (SpaceUnit su in redShipList)
            {
                if (su.myShipIndex.Equals(currentlyMovedSpaceshipIndex))
                {
                    attacker = su;
                    break;
                }
            }

            foreach (SpaceUnit su in blueShipList){
                if(su.myShipIndex.Equals(targetedSpaceshipIndex)){
                    victimTransform = su.transform;
                }
            }

        }
        if(!amIRed == !isRedTurn){
            foreach (SpaceUnit su in blueShipList)
            {
                if (su.myShipIndex.Equals(currentlyMovedSpaceshipIndex))
                {
                    attacker = su;
                    break;
                }
            }

            foreach (SpaceUnit su in redShipList){
                if(su.myShipIndex.Equals(targetedSpaceshipIndex)){
                    victimTransform = su.transform;
                }
            }
        }
    }

    public void ExecuteAttack()
    {

    }

    public void RegisterRedShip(SpaceUnit spaceUnit){
        redShipList.Add(spaceUnit);
    }
    public void RegisterBlueShip(SpaceUnit spaceUnit){
        blueShipList.Add(spaceUnit);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
    }

}
