using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBoardManager : MonoBehaviour
{
    public Transform boardPlane;

    public float boardSize = 5f;
    public float worldSize = 100f;

    bool redturn;
    bool blueturn;


    /// <summary>
    /*  
    처음에 일정 시간 ex 3분 안에 이번 게임에 사용할 말과 위치 등등 정하기..

    먼저 완료버튼을 누른 순서대로 차례 우선권 갖기
    
    */
    /// </sumary>

    /*
        각팀 차례 일때
    장기 말을 잡을 수 있게 하는 bool값 ture
    잡았을 때 잡은 물체를 받을 변수 NowGarbObject
    이동후 선택한 상대방 물체를 받을 변수 NowSetTargetObject
    
    공격이나 타케팅등 해당 이벤트를 처리 (각자 오브젝트에서)

    
    Card가 펼쳐질 차례를 알려줄 bool값 
    Card에서 나온 이벤트를 받을 변수 (ex 적 공격, 유성우, 등..)

    */


}
