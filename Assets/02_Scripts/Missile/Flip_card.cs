using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flip_card : MonoBehaviour
{
    public Animation CardAni;
    void Start()
    {
        CardAni = GetComponent<Animation>();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CardAni.Play("Card_Flip");
        }
    }

    public void Card_Flip(){
        gameObject.SetActive(true);
        CardAni.Play("Card_Flip");
    }

    public void Card_return(){
        gameObject.SetActive(false);
    }
}
