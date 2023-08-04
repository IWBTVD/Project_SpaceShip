using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class UnitHUDCanvas : MonoBehaviour
{
    private SpaceUnit spaceUnit;
    public TMP_Text actionNames;

    bool isUIActived;

    private void Awake() 
    {
        spaceUnit = GetComponentInParent<SpaceUnit>();
    }

    public void ActiveUI()
    {   
        if(!isUIActived){
            Debug.Log("Activate Line Object");
            gameObject.SetActive(true);
            SetActionText();
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

    private void SetActionText(){
        string text = spaceUnit.GetActionNames();
        actionNames.SetText(text);
    }
}
