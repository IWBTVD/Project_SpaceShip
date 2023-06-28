using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandInfo : MonoBehaviour
{
    public enum HandType
    {
        Left,
        Right,
    }

    public Transform thumb;
    public Transform indexFinger;
    public Transform middleFinger;

    public List<Transform> thumbs;
    public List<Transform> indexes;
    public List<Transform> middles;

    private void Awake()
    {
        thumbs.Add(thumb);
        thumbs.Add(thumb.GetChild(0));
        thumbs.Add(thumbs[0].GetChild(0));

        indexes.Add(indexFinger);
        indexes.Add(indexFinger.GetChild(0));
        indexes.Add(indexes[0].GetChild(0));

        middles.Add(middleFinger);
        middles.Add(middleFinger.GetChild(0));
        middles.Add(middles[0].GetChild(0));
    }
}
