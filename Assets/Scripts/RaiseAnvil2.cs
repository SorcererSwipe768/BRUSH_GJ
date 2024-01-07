using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaiseAnvil2 : MonoBehaviour
{
    public GameObject anvil;
    public bool canPlayAnim = true;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Here");
        Animator anim = anvil.GetComponent<Animator>();
        if (canPlayAnim)
        {
            anim.Play("anvil2up");
            canPlayAnim = false;
            Debug.Log("goofy ahh uncle");
        }
    }
}

