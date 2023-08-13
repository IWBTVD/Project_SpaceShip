using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileLauncher : MonoBehaviour
{
    [SerializeField] GameObject m_goMissile = null;
    [SerializeField] Transform m_tfMissileSpawn = null;
    
    public Vector3 targetPosition;
    // void Update()
    // {
    //     if(Input.GetKeyDown(KeyCode.S)){
    //         // GameObject t_missile = Instantiate(m_goMissile, m_tfMissileSpawn.position, m_tfMissileSpawn.rotation);
    //         // t_missile.GetComponent<Missile0_02>().m_tfTarget = target ;
    //         target.tag = "Enemy";
    //         MissileLaunch();
    //     }
        
    // }

    public void MissileLaunch(){
        GameObject t_missile = Instantiate(m_goMissile, m_tfMissileSpawn.position, m_tfMissileSpawn.rotation);
        t_missile.GetComponent<Missile0_02>().targetPosition = targetPosition ;
    }
}
