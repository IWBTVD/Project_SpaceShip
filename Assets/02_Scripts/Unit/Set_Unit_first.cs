using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Set_Unit_first : MonoBehaviour
{
    public bool isUnit = false;
    public bool isDetected = false;

    private Transform myTransform;
    [SerializeField] GameObject show_Detect;

    GameObject touchedObject;
    void Start(){
        myTransform = GetComponent<Transform>();
    }
    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("SpaceShip"))
        {
            Debug.Log("Collision with Bullet detected: 1");
            isDetected = true;
            show_Detect.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other){
        if (other.gameObject.CompareTag("SpaceShip"))
        {
            isDetected = false;
            show_Detect.SetActive(false);
        }
    }

    public void MoveToCenter(){
        if(isUnit != true){
            touchedObject.transform.position = myTransform.position;
            touchedObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }else
        {
            Debug.LogError("unit is assigned!");
        }
        
    }
}
