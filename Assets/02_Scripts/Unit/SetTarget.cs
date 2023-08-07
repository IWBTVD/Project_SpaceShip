using UnityEngine;
using UnityEngine.Events;

public class SetTarget : MonoBehaviour
{
    public UnityEvent<Transform> OnAttackEvent;
    
    // Call this method to trigger the attack event and notify the target
    public void AttackTarget()
    {   
        Transform target = GetComponentInParent<Transform>();
        Debug.Log(target.name);
        OnAttackEvent.Invoke(target);
    }

    private void Update() {
        if(Input.anyKeyDown){
            AttackTarget();
        }
    }
}
