using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public bool IsEnemy { get; private set; } = false;

    private int actionPoint = 2;
}
