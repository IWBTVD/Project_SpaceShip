using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileLauncher : MonoBehaviour
{
    [SerializeField] GameObject m_goMissile = null;
    [SerializeField] Transform m_tfMissileSpawn = null;
    
    public Transform targetTransform;
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
        t_missile.GetComponent<Missile0_02>().targetTransform = targetTransform ;

        // 보드 위 미사일을 실제 크기 공간(머리 위 하늘)에도 보여줌
        if (ActualVisualModelsManager.Instance)
            ActualVisualModelsManager.Instance.CreateMirror(t_missile.transform, ActualVisualModelsManager.MirrorKind.Projectile);
    }
}
