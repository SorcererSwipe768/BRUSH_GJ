using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawner : MonoBehaviour
{
    private Vector3 startPos;

    Rigidbody rb;

    void Start()
    {
        startPos = transform.position;
        rb = GetComponent<Rigidbody>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "DeathPlane")
        {
            transform.position = startPos;
            rb.velocity = new Vector3(0, 0, 0);
    
        }
    }
}
