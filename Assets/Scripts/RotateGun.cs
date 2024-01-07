using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateGun : MonoBehaviour
{
    public Grappling gg;
    public SwingScript ss;

    private Quaternion desiredRotation;
    private float rotationSpeed = 5;

    void Update()
    {
        if (!gg.IsGrappling() && !ss.ReturnSwinging())
        {
            desiredRotation = transform.parent.rotation;
        }

        else if (ss.ReturnSwinging())
        {
            desiredRotation = Quaternion.LookRotation(ss.GetSwingPoint() - transform.position);
        }

        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, Time.deltaTime * rotationSpeed);


    }
}
