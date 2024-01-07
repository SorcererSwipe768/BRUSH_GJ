using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grappling : MonoBehaviour
{
    [Header("References")]
    private PlayerMovement pm;
    public Transform cam;
    public Transform gunTip;
    public LayerMask whatIsGrappleable;
    public LineRenderer lr;
    public Rigidbody rb;
    public Transform player;
    public GameObject hitObj;

    [Header("Grappling")]
    public float maxGrappleDistance;
    public float grappleDelayTime;
    public float overshootYAxis;

    private Vector3 grapplePoint;

    [Header("Cooldown")]
    public float grapplingCd;
    private float grapplingCdTimer;

    [Header("Input")]
    public KeyCode grappleKey = KeyCode.Mouse1;

    public bool grappling;

    private void Start()
    {
        pm = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(grappleKey)) StartGrapple();

        if (grapplingCdTimer > 0)
            grapplingCdTimer -= Time.deltaTime;

        RaycastHit goofy;

        if (Physics.Raycast(cam.position, cam.forward, out goofy, maxGrappleDistance, whatIsGrappleable))
        {
            hitObj.SetActive(true);

            hitObj.transform.position = goofy.point;
        }
        else
        {
            hitObj.SetActive(false);
        }
    }

    private void StartGrapple()
    {
        if (grapplingCdTimer > 0) return;

        grappling = false;

        GetComponent<SwingScript>().StopSwing();

        RaycastHit hit;

        if (Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, whatIsGrappleable))
        {

            pm.freeze = true;

            grappling = true;

            grapplePoint = hit.point;

            rb.drag = 0;

            Invoke(nameof(ExecuteGrapple), grappleDelayTime);
        }
        else
        {

            grappling = false;

            Invoke(nameof(StopGrapple), grappleDelayTime);

        }


    }

    private void ExecuteGrapple()
    {
        pm.freeze = false;

        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

        if (grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis;

        pm.JumpToPosition(grapplePoint, highestPointOnArc);

        Invoke(nameof(StopGrapple), 1f);
    }

    public void StopGrapple()
    {
        pm.freeze = false;

        grappling = false;

        pm.activeGrapple = false;

        grapplingCdTimer = grapplingCd;
    }

    public bool IsGrappling()
    {
        return grappling;
    }

    public Vector3 GetGrapplePoint()
    {
        return grapplePoint;
    }
}