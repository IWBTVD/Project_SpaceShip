using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomOVRGrabber : OVRGrabber
{
    private bool grabEnabled = true; // 새로 추가한 필드

    // 기존 코드와 동일한 생성자, 메서드, 변수들...

    // 원할 때 잡기 기능을 끄거나 켜는 메서드 추가
    public void SetGrabEnabled(bool isEnabled)
    {
        grabEnabled = isEnabled;
        GrabVolumeEnable(isEnabled);
    }

    // Update 메서드 오버라이드
    public override void Update()
    {
        if (grabEnabled)
        {
            base.Update(); // 부모 클래스의 Update 메서드 호출
        }
    }

    // 기존 코드와 동일한 다른 메서드들...
}
