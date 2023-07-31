using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Set_Unit_first : MonoBehaviour
{
    bool isUnit = false;



    private void OnCollisionEnter(Collision other) {
       if(other.gameObject.layer == LayerMask.NameToLayer("ShipExterior"))
        {
            print("1");
        }
    }
}
