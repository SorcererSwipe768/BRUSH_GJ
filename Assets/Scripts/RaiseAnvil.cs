using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaiseAnvil : MonoBehaviour
{
    public GameObject anvil;
    public bool canPlayAnim = true;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Here");
        Animator anim = anvil.GetComponent<Animator>();
        if(canPlayAnim)
        {
            anim.Play("anvilUp");
            canPlayAnim = false;
        }
    }
}
