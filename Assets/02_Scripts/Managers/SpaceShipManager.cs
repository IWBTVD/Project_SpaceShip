using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using Photon.Pun;
public class SpaceShipManager : MonoBehaviour, IPunObservable
{


    public Action<Vector3> OnAttackEvent;

    private List<SpaceUnit> redShipList = new List<SpaceUnit>();
    private List<SpaceUnit> blueShipList = new List<SpaceUnit>();

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

    /// <summary>
    /// 이번 턴에 방금 이동한 우주선(공격자)이 victim을 공격할 수 있는지 확인하고, 가능하면 공격한다.
    /// SpaceUnit.SelectAttackTarget RPC로 모든 클라이언트에서 호출된다 (이동과 같은 방식).
    /// (예전엔 redShipList/blueShipList가 초기화되지 않아 NullReference, 찾은 뒤에도 Attack을 호출하지 않았음)
    /// </summary>
    public bool SelectAttackTarget(ShipIndex victimIndex)
    {
        WorldBoardManager wbm = WorldBoardManager.Instance;
        if (!wbm || wbm.CurrentStage != WorldBoardManager.GameStage.GamePlaying) return false;

        Team turnTeam = wbm.CurrentTurn == WorldBoardManager.Turn.Red ? Team.Red
                      : wbm.CurrentTurn == WorldBoardManager.Turn.Blue ? Team.Blue : Team.None;
        // 공격 대상은 상대 팀 우주선, 공격자는 이번 턴 팀이 방금 이동한 우주선
        if (turnTeam == Team.None || victimIndex.team == Team.None || victimIndex.team == turnTeam) return false;
        if (currentlyMovedSpaceshipIndex.team != turnTeam) return false;

        SpaceUnit attacker = null, victim = null;
        foreach (SpaceUnit su in FindObjectsByType<SpaceUnit>(FindObjectsSortMode.None))
        {
            if (su.myShipIndex.Equals(currentlyMovedSpaceshipIndex)) attacker = su;
            else if (su.myShipIndex.Equals(victimIndex)) victim = su;
        }
        if (!attacker || !victim || attacker.ActionPoints != 1) return false;

        targetedSpaceshipIndex = victimIndex;
        int damage = attacker.AttackDamage;
        attacker.Attack(victim.transform); // 미사일 발사 → 턴 넘어감
        victim.ReceiveAttack(damage);      // 미사일이 맞으면 데미지
        return true;
    }

    private void Start()
    {
        if (instance != this) return;
        if (WorldBoardManager.Instance)
            WorldBoardManager.Instance.OnCurrentTurnChanged += _ => currentlyMovedSpaceshipIndex = new ShipIndex();
    }

    /// <summary>새 게임 준비</summary>
    public void ResetForNewGame()
    {
        currentlyMovedSpaceshipIndex = new ShipIndex();
        targetedSpaceshipIndex = new ShipIndex();
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
