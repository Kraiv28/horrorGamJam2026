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
    public float ceilingCheckDistance = 1.5f; // <--- เพิ่มตัวแปรนี้ให้ปรับระยะได้เอง

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

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        stateHandler();
        SpeedControl();

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
            state = movementState.walking; // ตั้งสถานะเป็นเดินปกติ
            isRuning = false;
            return; // ออกจากฟังก์ชันทันที ไม่ให้คำสั่งด้านล่างมาเขียนทับค่านี้
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

    // === ปรับปรุงให้คำนวณจากค่า ceilingCheckDistance ที่เรากำหนดเอง ===
    private bool IsCeilingAbove()
    {
        Vector3 rayOrigin = transform.position;
        // ยิงเลเซอร์ทรงกลมขึ้นไปด้านบนตามระยะที่เราตั้งไว้ใน Inspector
        return Physics.SphereCast(rayOrigin, playerRadius, Vector3.up, out _, ceilingCheckDistance, whatIsCeiling);
    }

    private void OnDrawGizmosSelected()
    {
        // วาดเส้นจำลองระยะในหน้า Scene เพื่อให้ตั้งค่าได้ง่าย
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
}