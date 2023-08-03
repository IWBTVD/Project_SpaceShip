using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitHUDCanvas : MonoBehaviour
{
    bool isUIActived;
    public void ActiveUI()
    {   
        if(!isUIActived){
            Debug.Log("Activate Line Object");
            gameObject.SetActive(true);
            isUIActived = true;
        }
    }

    public void DeactivateUI()
    {
        if(isUIActived){
            Debug.Log("deactivate Line Object");
            gameObject.SetActive(false);
            isUIActived = false;
        }
    }
}
