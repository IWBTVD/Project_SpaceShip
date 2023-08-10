using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    
    private SpaceShipManager spaceShipManager;

    private void Start() {
        spaceShipManager = SpaceShipManager.Instance;
    }

    public void setTarget(){
        spaceShipManager.RegisterTarget(this.transform);
        
    }

}