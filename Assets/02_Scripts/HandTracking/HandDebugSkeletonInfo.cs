using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 핸드 트래킹의 정보를 디버그 로그로 쏴주는 스크립트.
/// 핸드 트래킹의 정보를 담고 출력하는 아주 좋은 예시이기에 가져옴.
/// 나중에 포톤 서버와 위치 동기화를 위해 참고할 목적으로 작성함.
/// </summary>
public class HandDebugSkeletonInfo : MonoBehaviour
{
    public enum HandInfoFrequency
    {
        None,   //출력 안함
        Once,   //한번만 출력함
        Repeat, //항상 출력함
    }

    [Header("선택적 할당 요소")]
    [SerializeField] private OVRHand hand;
    [SerializeField] private OVRSkeleton handSkeleton;
    [SerializeField] private HandInfoFrequency handInfoFrequency = HandInfoFrequency.Once;

    private bool isHandInfoDisplayed = false;
    private bool isPauseDisplay = false;

    private void Awake()
    {
        if (!hand) hand = GetComponent<OVRHand>();
        if (!handSkeleton) handSkeleton = GetComponent<OVRSkeleton>();
    }

    private void Update()
    {
#if UNITY_EDITOR
        //에디터 상에서 실행중일 경우 스페이스 바를 입력받아 출력모드 변경
        if (Input.GetKeyDown(KeyCode.Space)) isPauseDisplay = !isPauseDisplay;
#endif

        if(hand.IsTracked && !isPauseDisplay)
        {
            //한번만 출력할거면 한번만 출력함
            if (handInfoFrequency == HandInfoFrequency.Once && !isHandInfoDisplayed)
            {
                //DisplayBoneInfo();
                isHandInfoDisplayed = true;
            }
            //계속 출력해야하면 계속 출력함
            else if (handInfoFrequency == HandInfoFrequency.Repeat)
            {
                //DisplayBoneInfo();
            }

        }
    }

    private void DisplayBoneInfo()
    {
        foreach(var bone in handSkeleton.Bones)
        {
            Debug.Log($"{handSkeleton.GetSkeletonType()}: bone Id = {bone.Id}, pos = {bone.Transform.position}");
        }

        //Debug.Log($"{handSkeleton.GetSkeletonType()}: num of bone = {handSkeleton.GetCurrentNumBones()}");
        //Debug.Log($"{handSkeleton.GetSkeletonType()}: num of skinnable bone = {handSkeleton.GetCurrentNumSkinnableBones()}");
        //Debug.Log($"{handSkeleton.GetSkeletonType()}: start bone Id = {handSkeleton.GetCurrentStartBoneId()}");
        //Debug.Log($"{handSkeleton.GetSkeletonType()}: end bone Id = {handSkeleton.GetCurrentEndBoneId()}");
    }
}
