using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingScript : MonoBehaviour
{
    [Header("Input")]
    public KeyCode swingKey = KeyCode.Mouse1;

    [Header("References")]
    public Transform gunTip, cam, player;
    public LayerMask whatIsGrappleable;
    public PlayerMovement pm;

    [Header("OdmGear")]
    public Transform orientation;
    public Rigidbody rb;
    public float horizontalThrustForce;
    public float forwardThrustForce;
    public float extendCableSpeed;

    [Header("Swinging")]
    private float maxSwingDistance = 25f;
    private Vector3 swingPoint;
    private SpringJoint joint;

    public bool isSwinging;

    private void StartSwing()
    {
        GetComponent<Grappling>().StopGrapple();

        RaycastHit hit;

        if(Physics.Raycast(cam.position, cam.forward, out hit, maxSwingDistance, whatIsGrappleable))
        {
            isSwinging = true;

            rb.drag = 0;

            swingPoint = hit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = swingPoint;

            float distanceFromPoint = Vector3.Distance(player.position, swingPoint);

            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;

            joint.spring = 4.5f;
            joint.damper = 7f;
            joint.massScale = 4.5f;
        }
    }

    private void OdmGearMovement()
    {
        // right
        if (Input.GetKey(KeyCode.D)) rb.AddForce(orientation.right * horizontalThrustForce * Time.deltaTime);
        // left
        if (Input.GetKey(KeyCode.A)) rb.AddForce(-orientation.right * horizontalThrustForce * Time.deltaTime);

        // forward
        if (Input.GetKey(KeyCode.W)) rb.AddForce(orientation.forward * horizontalThrustForce * Time.deltaTime);

        // shorten cable
        if (Input.GetKey(KeyCode.Space))
        {
            Vector3 directionToPoint = swingPoint - transform.position;
            rb.AddForce(directionToPoint.normalized * forwardThrustForce * Time.deltaTime);

            float distanceFromPoint = Vector3.Distance(transform.position, swingPoint);

            joint.maxDistance = distanceFromPoint * 0.3f;
            joint.minDistance = distanceFromPoint * 0.25f;
        }
        // extend cable
        if (Input.GetKey(KeyCode.S))
        {
            float extendedDistanceFromPoint = Vector3.Distance(transform.position, swingPoint) + extendCableSpeed;

            joint.maxDistance = extendedDistanceFromPoint * 0.3f;
            joint.minDistance = extendedDistanceFromPoint * 0.25f;
        }
    }

    public void StopSwing()
    {
        isSwinging = false;

        Destroy(joint);
    }

    void Update()
    {
        if(Input.GetKeyDown(swingKey)) StartSwing();
        if (Input.GetKeyUp(swingKey)) StopSwing();

        if (joint != null) OdmGearMovement();
    }

    public bool ReturnSwinging()
    {
        return isSwinging;
    }

    public Vector3 GetSwingPoint()
    {
        return swingPoint;
    }
}
