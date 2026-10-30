using UnityEngine;
using UnityEngine.InputSystem;

public class playerMovementRB : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float crouchSpeed;

    public float groundDrag;

    [Header("Crouch Settings")]
    public float crouchYScale = 0.5f;
    private float startYScale;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Ceiling Check (Manual Settings)")]
    [Tooltip("เลือก Layer ของท่อหรือช่องแคบเท่านั้น (ห้ามเลือก Layer หลังคาทั่วไป)")]
    public LayerMask whatIsCeiling;

    [Tooltip("กำหนดขนาดความกว้าง/รัศมีของหัวผู้เล่นในการเช็ค (ค่าเริ่มต้นแนะนำ 0.5)")]
    public float playerRadius = 0.5f;

    [Tooltip("ระยะยิงเช็คขึ้นไปด้านบนนับจากจุดกึ่งกลางตัว (ถ้าหลังคาสูงกว่าค่านี้จะไม่หมอบ)")]
    public float ceilingCheckDistance = 1.5f;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;

    [Header("Keybinds")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.C;

    public Transform oRT;

    float horizontalI;
    float verticalIP;

    Vector3 moveDirection;
    Rigidbody rb;

    public movementState state;
    private bool isSlowedByCutscene = false;
    private float temporaryCutsceneSpeed = 1f;

    public static bool isRuning;
    public static bool isCrouching;

    [Header("Footstep Settings")]
    public AudioSource footstepAudioSource;
    public AudioClip[] walkFootsteps;
    public AudioClip[] sprintFootsteps;
    public AudioClip[] crouchFootsteps;
    public float walkStepInterval = 0.5f;
    public float sprintStepInterval = 0.3f;
    public float crouchStepInterval = 0.8f;
    private float footstepTimer;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        startYScale = transform.localScale.y;

        if (footstepAudioSource == null)
        {
            footstepAudioSource = GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        stateHandler();
        SpeedControl();
        HandleFootsteps();

        if (grounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    public enum movementState
    {
        walking,
        runing,
        crouching,
        air
    }

    private void MyInput()
    {
        horizontalI = Input.GetAxisRaw("Horizontal");
        verticalIP = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(crouchKey))
        {
            StartCrouch();
        }

        if (Input.GetKeyUp(crouchKey))
        {
            TryStopCrouch();
        }
    }

    private void StartCrouch()
    {
        transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }

    private void TryStopCrouch()
    {
        if (!IsCeilingAbove())
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            isCrouching = false;
        }
    }

    private void stateHandler()
    {
        if (isSlowedByCutscene)
        {
            moveSpeed = temporaryCutsceneSpeed;
            state = movementState.walking;
            isRuning = false;
            return;
        }
        if (Input.GetKey(crouchKey) || IsCeilingAbove())
        {
            moveSpeed = crouchSpeed;
            state = movementState.crouching;
            isCrouching = true;
            isRuning = false;

            if (transform.localScale.y != crouchYScale)
            {
                transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            }
        }
        else if (!Input.GetKey(crouchKey) && transform.localScale.y == crouchYScale)
        {
            TryStopCrouch();
        }
        else if (grounded && Input.GetKey(sprintKey))
        {
            moveSpeed = sprintSpeed;
            state = movementState.runing;
            isRuning = true;
            isCrouching = false;
        }
        else if (grounded)
        {
            moveSpeed = walkSpeed;
            state = movementState.walking;
            isRuning = false;
            isCrouching = false;
        }
        else
        {
            state = movementState.air;
        }
    }

    private void MovePlayer()
    {
        moveDirection = oRT.forward * verticalIP + oRT.right * horizontalI;

        if (OnSlope())
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
    }

    private void SpeedControl()
    {
        Vector3 vel = rb.linearVelocity;
        if (new Vector2(vel.x, vel.z).magnitude > moveSpeed)
        {
            Vector3 limitedVel = new Vector3(vel.x, 0, vel.z).normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, vel.y, limitedVel.z);
        }
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

    private bool IsCeilingAbove()
    {
        Vector3 rayOrigin = transform.position;
        return Physics.SphereCast(rayOrigin, playerRadius, Vector3.up, out _, ceilingCheckDistance, whatIsCeiling);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 endPoint = transform.position + Vector3.up * ceilingCheckDistance;
        Gizmos.DrawWireSphere(endPoint, playerRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, endPoint);
    }

    public void SetCutsceneSlow(bool shouldSlow, float speedValue = 1f)
    {
        isSlowedByCutscene = shouldSlow;
        temporaryCutsceneSpeed = speedValue;
    }

    // === ระบบจัดการเสียงเดิน (Footsteps Logic) ===
    private void HandleFootsteps()
    {
        // เช็คว่าอยู่บนพื้นและมีการกดปุ่มเดิน (มีค่า Input แนวตั้งหรือแนวนอน)
        if (!grounded || state == movementState.air) return;

        if (Mathf.Abs(horizontalI) > 0.1f || Mathf.Abs(verticalIP) > 0.1f)
        {
            float currentInterval = walkStepInterval;

            if (state == movementState.runing)
            {
                currentInterval = sprintStepInterval;
            }
            else if (state == movementState.crouching)
            {
                currentInterval = crouchStepInterval;
            }

            footstepTimer += Time.deltaTime;

            if (footstepTimer >= currentInterval)
            {
                PlayFootstepSound();
                footstepTimer = 0f;
            }
        }
        else
        {
            // รีเซ็ต Timer ทันทีเมื่อหยุดเดิน เพื่อให้ก้าวแรกดังทันทีที่เริ่มเดินใหม่
            footstepTimer = walkStepInterval;
        }
    }

    private void PlayFootstepSound()
    {
        if (footstepAudioSource == null) return;

        AudioClip[] clipsToPlay = walkFootsteps;

        if (state == movementState.runing && sprintFootsteps != null && sprintFootsteps.Length > 0)
        {
            clipsToPlay = sprintFootsteps;
        }
        else if (state == movementState.crouching && crouchFootsteps != null && crouchFootsteps.Length > 0)
        {
            clipsToPlay = crouchFootsteps;
        }
        else if (walkFootsteps == null || walkFootsteps.Length == 0)
        {
            return; // ถ้าไม่ได้ใส่เสียงเดินไว้ ให้ข้าม
        }

        // สุ่มเสียงในอาเรย์เพื่อให้เสียงไม่จำเจ
        AudioClip clip = clipsToPlay[Random.Range(0, clipsToPlay.Length)];
        if (clip != null)
        {
            footstepAudioSource.PlayOneShot(clip);
        }
    }
}