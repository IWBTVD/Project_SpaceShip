using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using Photon.Pun;
public class SpaceShipManager : MonoBehaviour, IPunObservable
{


    public Action<Vector3> OnAttackEvent;

    private static SpaceShipManager instance;
    public static SpaceShipManager Instance
    {
        get { return instance; }
    }

    public List<SpaceUnit> myShipList;
    public List<SpaceUnit> enemyShipList;
    
    private Vector3 targetPosition;

    public int myIndex = 1;

    //방금 움직인 비행선 인덱스
    public int currentlyMovedSpaceshipIndex = 0;
    //공격 대상으로 지정된 비행선 인덱스
    public int targetedSpaceshipIndex = 0;

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

    public void RegisterShip(SpaceUnit spaceShip)
    {
        //allShips.Add(spaceShip);
    }

    public void RegisterTarget(Vector3 position)
    {
        targetPosition = position;
        OnAttackEvent.Invoke(targetPosition);
    }

    public Vector3 GetTarget(){
        return targetPosition;
    }
    public void PlacedSpaceshipToBeacon(SpaceUnit unit)
    {
        unit.myShipIndex = myIndex;
        myIndex++;
    }

    public void SelectAttackTarget(int victimIndex)
    {
        targetedSpaceshipIndex = victimIndex;
        

    }

    public void ExecuteAttack()
    {
        SpaceUnit attacker = null;
        SpaceUnit victim = null;

        bool amIRed = PhotonNetwork.LocalPlayer.CustomProperties.ContainsValue("Red");
        bool isRedTurn = WorldBoardManager.Instance.CurrentTurn == WorldBoardManager.Turn.Red;

        //레드의턴이고, 레드가 나임
        if (amIRed == isRedTurn)
        {
            //어태커 찾아서 할당
            foreach (SpaceUnit su in myShipList)
            {
                if (su.myShipIndex == currentlyMovedSpaceshipIndex)
                {
                    attacker = su;
                    break;
                }
            }

            foreach (SpaceUnit su in enemyShipList)
            {
                if (su.myShipIndex == targetedSpaceshipIndex)
                {
                    victim = su;
                    break;
                }
            }

            attacker.Attack();
        }
        else
        {

        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
    }

}
