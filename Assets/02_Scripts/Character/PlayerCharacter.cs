using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    public HandInfo handInfo;

    public Transform leftHandTransform;
    public Transform rightHandTransform;

    //public List<Transform> leftHandBoneList;
    //public List<Transform> rightHandBoneList;

    public Transform[] leftThumb;
    public Transform[] leftIndex;
    public Transform[] leftFinger;

    public Transform[] rightThumb;
    public Transform[] rightIndex;
    public Transform[] rightFinger;

    private void Awake()
    {
        leftFinger = leftHandTransform.GetChild(0).GetComponentsInChildren<Transform>();
        leftIndex = leftHandTransform.GetChild(1).GetComponentsInChildren<Transform>();
        leftThumb = leftHandTransform.GetChild(2).GetComponentsInChildren<Transform>();

        rightFinger = rightHandTransform.GetChild(0).GetComponentsInChildren<Transform>();
        rightIndex = rightHandTransform.GetChild(1).GetComponentsInChildren<Transform>();
        rightThumb = rightHandTransform.GetChild(2).GetComponentsInChildren<Transform>();
    }

    private void Update()
    {
        for(int i = 0; i < 3; i++)
        {
            //rightFinger[i].localPosition = handInfo.middles[i].localPosition;
            rightFinger[i].localRotation = handInfo.middles[i].localRotation;

            //rightIndex[i].localPosition = handInfo.indexes[i].localPosition;
            rightIndex[i].localRotation = handInfo.indexes[i].localRotation;

            //rightThumb[i].localPosition = handInfo.thumbs[i].localPosition;
            rightThumb[i].localRotation = handInfo.thumbs[i].localRotation;
        }
    }
}
