using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileLauncher : MonoBehaviour
{
    [SerializeField] GameObject m_goMissile = null;
    [SerializeField] Transform m_tfMissileSpawn = null;
    
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.S)){
            GameObject t_missile = Instantiate(m_goMissile, m_tfMissileSpawn.position, m_tfMissileSpawn.rotation);
            t_missile.GetComponent<Rigidbody>().velocity = Vector3.forward * 5f;
        }
        
    }
}
