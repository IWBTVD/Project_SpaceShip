using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Photon.Pun;

//[RequireComponent(typeof(PhotonView))]
public class Timer : MonoBehaviour
{
    /*
    public void OnPointerClick(PointerEventData eventData)
    {
        Pause = !Pause;
    }
    [SerializeField] private TMP_Text  uiText;

    public int Duration;

    private int remainingDuration;

    private bool Pause;

    private void Start()
    {
        Being(Duration);
    }

    private void Being(int Second)
    {
        remainingDuration = Second;
        StartCoroutine(UpdateTimer());
    }

    private IEnumerator UpdateTimer()
    {
        while(remainingDuration >= 0)
        {
            if (!Pause)
            {
                uiText.text = $"{remainingDuration / 60:00}:{remainingDuration % 60:00}";
                remainingDuration--;
                yield return new WaitForSeconds(1f);
            }
            yield return null;
        }
        OnEnd();
    }

    private void OnEnd()
    {
        //End Time , if want Do something
        print("End");
    }
    */

    [SerializeField] private TMP_Text uiText;

    /// <summary>
    /// 준비 시간
    /// </summary>
    public float prepareTime = 120f;
    /// <summary>
    /// 현재 남아있는 시간
    /// </summary>
    public float remainedTime = 0f;

    /// <summary>
    /// 방장이 준비되었는지(내가 준비되었는지가 아니라 방장이 준비되었는지를 기준으로 함. 멀티플레이기 때문에 '나'를 주체로 하면 코드가 괜히 복잡해질 우려가 있음
    /// </summary>
    public bool isHostReady = false;
    /// <summary>
    /// 방원이 준비되었는지
    /// </summary>
    public bool isMemberReady = false;

    /// <summary>
    /// 누가 먼저 준비를 끝마쳤는가? 0 : 방장, 1 : 방원
    /// </summary>
    public int firstPreparedPlayer = -1;

    /// <summary>
    /// 타이머가 시작되었는지
    /// </summary>
    private bool isTimerStart = false;

    /// <summary>
    /// isTimerStart를 public으로 노출시켜주는 함수
    /// </summary>
    public bool IsTimerStart {
        //isTimerStart 값을 읽어오는건 public으로 설정됨.
        get => isTimerStart;

        //isTimerStart값을 쓰는건 private으로 설정됨. 값을 변경시 디버그 로그가 호출됨
        private set {
            isTimerStart = value;
            //현재 시간을 준비시간만큼 설정
            remainedTime = prepareTime;
            //isTimerStart가 참이면 시작되었습니다! 거짓이면 끝났습니다!
            Debug.Log($"준비 시간이 " + (isTimerStart? "시작되었습니다!" : "끝났습니다!"));
        }
    }

    private void Start()
    {
        IsTimerStart = true;
    }

    private void Update()
    {
        if(IsTimerStart)
        {
            //타이머 작동
            remainedTime -= Time.deltaTime;
            uiText.text = $"{remainedTime / 60:00}:{remainedTime % 60:00}";

            //준비 시간이 종료되었는지 검사
            if (TimesUp())
                IsTimerStart = false;
        }
    }

    /// <summary>
    /// 정해진 시간이 끝났거나, 두 플레이어 모두 조기에 준비를 마쳤을 때 턴 종료
    /// </summary>
    /// <returns>준비 시간이 끝났으면 true</returns>
    public bool TimesUp()
    {
        if (remainedTime <= 0)
        {
            Debug.Log("제한 시간 종료!!!");
            return true;
        }

        else if(isHostReady && isMemberReady)
        {
            Debug.Log("양 측 플레이어가 모두 준비를 마침");
            return true;
        }

        return false;
    }
}
