using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    public float dashSpeedChangeFactor;


    public float maxYSpeed;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    [Header("FOV")]
    public PlayerCam cam;
    public float grappleFov = 95f;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Glide Cooldowns")]
    public float glideTimer;
    public float glideCooldown;
    public GameObject gliderModel;
    public bool canGlide = true;
    public bool glideReady = true;

    public Grappling gp;
    public SwingScript ss;
    public Grappling gg;

    public bool isJumping = false;
    public bool isWalking = false;

    public MovementState state;

    public enum MovementState
    {
        freeze,
        sprinting,
        walking,
        dashing,
        gliding,
        grappling,
        swinging,
        air
    }

    public bool dashing;

    public bool freeze;

    public bool activeGrapple;

    public bool swinging;

    public bool gliding;

    public Transform orientation;

    public float lastGlide;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        gp = GetComponent<Grappling>();
        ss = GetComponent<SwingScript>();
        readyToJump = true;
        glideReady = true;
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

        StateHandler();
        MyInput();
        SpeedControl();
        GetSlopeMoveDirection();

        if (gliding && glideReady && !grounded)
        {
            state = MovementState.gliding;
            OpenGlider();
            rb.drag = 10f;
            desiredMoveSpeed = 27f;
            cam.DoFov(95f);
        }

        if(rb.drag < 5 || activeGrapple || ss.isSwinging || grounded)
        {
            glideReady = false;
            StartCoroutine(ResetGlide());
            CloseGlider();
        }

    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }     
    }

    public void CounterMovement()
    {
        Vector3 vel = rb.velocity;
        vel.y = 0f;

        float coefficientOfFriction = 12f;

        rb.AddForce(-vel * coefficientOfFriction, ForceMode.Acceleration);
    }

    public void MovePlayer()
    {
        if (swinging) return;
        if (state == MovementState.dashing) return;
        if (state == MovementState.freeze) return;

        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope && moveSpeed > 12)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 10f, ForceMode.Force);
            CounterMovement();

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 120f, ForceMode.Force);
        }

        else if (OnSlope() && !exitingSlope && moveSpeed <= 12)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 10f, ForceMode.Force);
            CounterMovement();

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        // on ground
        else if (grounded && !OnSlope())
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
            CounterMovement();
        }

        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        if (activeGrapple) return;
        if (gg.grappling) return;
        if (state == MovementState.sprinting) return;
        // limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }

        if (maxYSpeed != 0 && rb.velocity.y > maxYSpeed)
        {
            rb.velocity = new Vector3(rb.velocity.x, maxYSpeed, rb.velocity.z);
        }
    }

    private bool enableMovementOnNextTouch;

    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        activeGrapple = true;

        velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        Invoke(nameof(SetVelocity), 0.1f);

        Invoke(nameof(ResetFov), 2f);
    }

    private Vector3 velocityToSet;

    private void SetVelocity()
    {
        enableMovementOnNextTouch = true;
        rb.velocity = velocityToSet;

        cam.DoFov(grappleFov);
    }

    public void ResetFov()
    {
        cam.DoFov(80f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;

            GetComponent<Grappling>().StopGrapple();

            activeGrapple = false;

            cam.DoFov(80f);
        }
    }

    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity)
            + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }

    private void Jump()
    {
        rb.drag = 0f;
        exitingSlope = true;

        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        exitingSlope = false;

        readyToJump = true;
    }

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    private MovementState lastState;
    private bool keepMomentum;

    private void StateHandler()
    {

        if(freeze)
        {
            gliding = false;
            rb.drag = 0f;
            state = MovementState.freeze;
            desiredMoveSpeed = 0;
            rb.velocity = Vector3.zero;
        }

        else if(activeGrapple)
        {
            gliding = false;
            rb.drag = 0f;
            state = MovementState.grappling;
            desiredMoveSpeed = 0f;
        }

        else if (ss.isSwinging)
        {
            gliding = false;
            rb.drag = 0f;
            state = MovementState.swinging;
            desiredMoveSpeed = 15f;
        }

        else if (dashing)
        {
            gliding = false;
            rb.drag = 0f;
            state = MovementState.dashing;
            desiredMoveSpeed = 24f;
            speedChangeFactor = dashSpeedChangeFactor;

        }

        else if (grounded && Input.GetKey(sprintKey))
        {
            gliding = false;
            rb.drag = 0f;
            state = MovementState.sprinting;
            desiredMoveSpeed = 17f;
            cam.DoFov(95f);
        }

        else if (grounded)
        {
            gliding = false;
            rb.drag = 0f;
            state = MovementState.walking;
            desiredMoveSpeed = 12f;
            cam.DoFov(80f);
        }

        else if (!grounded && Input.GetKey(KeyCode.Q))
        {
            if (!ss.isSwinging)
            {
                Invoke(nameof(glide), glideTimer);
            }
        }

        else
        {
            gliding = false;
            rb.drag = 0f;
            state = MovementState.air;
            desiredMoveSpeed = 12f;

            if (desiredMoveSpeed < 17f)
                desiredMoveSpeed = 12f;
            else
                desiredMoveSpeed = 17f;

        }

        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;
        if(lastState == MovementState.dashing) keepMomentum = true;

        if (desiredMoveSpeedHasChanged)
        {
            if (keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                StopAllCoroutines();
                moveSpeed = desiredMoveSpeed;
            }
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastState = state;
    }

    private float speedChangeFactor;

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        float boostFactor = speedChangeFactor;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            time += Time.deltaTime * boostFactor;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
        speedChangeFactor = 1f;
        keepMomentum = false;
    }

    private bool OnSlope()
    {

        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    public void glide()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            gliding = true;
        }
    }

    public void OpenGlider()
    {
        Animator anim = gliderModel.GetComponent<Animator>();
        anim.ResetTrigger("gliderClosing");
        anim.SetTrigger("gliderOpened");
    }

    public void CloseGlider()
    {
        Animator anim = gliderModel.GetComponent<Animator>();
        glideReady = false;
        anim.ResetTrigger("gliderOpened");
        anim.SetTrigger("gliderClosing");
        StartCoroutine(ResetGlide());
    }

    IEnumerator ResetGlide()
    {
        yield return new WaitForSeconds(glideCooldown);
        glideReady = true;
    }

}