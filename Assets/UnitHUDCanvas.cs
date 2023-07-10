using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitHUDCanvas : MonoBehaviour
{

    public void ActiveUI()
    {
        Debug.Log("Activate Line Object");
        gameObject.SetActive(true);
    }

    public void DeactivateUI()
    {
        Debug.Log("deactivate Line Object");
        gameObject.SetActive(false);
    }
}
