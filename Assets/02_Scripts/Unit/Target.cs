using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    
    private TargetManager TargetManager;

    private void Start() {
        TargetManager = TargetManager.Instance;
    }

    public void setTarget(){
        TargetManager.RegisterTarget(this.transform);
        
    }



}