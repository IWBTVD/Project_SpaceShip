using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile0_02 : MonoBehaviour
{
    Rigidbody m_rigid = null;
    // Transform m_tfTarget = null;

    [SerializeField] float m_speed = 0f;
    float m_currentSpeed = 0f;
    public Transform m_tfTarget;
    [SerializeField] ParticleSystem m_psEffect = null;
    [SerializeField] LayerMask m_layerMask = 0;
    
    //미사일의 데미지 우주선마다 다름
    private int damage;
    IEnumerator LaunchDelay()
    {
        yield return new WaitUntil(() => m_rigid.velocity.y < 0f);
        yield return new WaitForSeconds(0.1f);

        m_psEffect.Play();

        yield return new WaitForSeconds(7f);
        Destroy(gameObject);
    }

    void Start()
    {
        m_rigid = GetComponent<Rigidbody>();

        StartCoroutine(LaunchDelay());
    }

     void Update()
    {
        if (m_tfTarget != null)
        {
            if (m_currentSpeed <= m_speed) 
                m_currentSpeed += m_speed * Time.deltaTime;

            transform.position += transform.forward * m_currentSpeed * Time.deltaTime;

            Vector3 t_dir = (m_tfTarget.position - transform.position).normalized;
            transform.forward = Vector3.Lerp(transform.forward, t_dir, 0.5f); // Increase the second parameter for faster rotation

        }
    }

    private void OnTriggerEnter(Collider other) {
        if(other.transform.CompareTag("Enemy")){
            Destroy(gameObject);
        }
    }



    // 삭제 예정
    // void SearchEnemy()
    // {
    //     Collider[] t_cols = Physics.OverlapSphere(transform.position, 100f, m_layerMask);

    //     if (t_cols.Length > 0)
    //     {
    //         m_tfTarget = t_cols[Random.Range(0, t_cols.Length)].transform;
    //     }
    // }
}
