using UnityEngine;

public class cameraEffects : MonoBehaviour
{
    public playerMovementRB movement;
    public mouseLook mouseLookScript;

    private Rigidbody rb;
    private Transform camTransform;

    [Header("Bob Settings")]
    public float walkBobSpeed = 12f;
    public float walkBobAmount = 0.04f;
    public float crouchBobSpeed = 6f;
    public float crouchBobAmount = 0.03f;
    private float timer;
    private float defaultYPos;

    [Header("Crouch Lean Wobble")]
    [Tooltip("องศาการเอียงหัวซ้าย-ขวาตอนคลาน")]
    public float crouchLeanWobbleAmount = 3f;
    private float currentCrouchWobble;

    [Header("Run Lean Wobble (NEW)")]
    [Tooltip("องศาการเอียงหัวซ้าย-ขวาตอนวิ่งสับ (แนะนำ 4 - 6 กำลังดี)")]
    public float runLeanWobbleAmount = 5f;
    private float currentRunWobble;

    [Header("Unity Event Lean Shake Settings (NEW)")]
    [Tooltip("ความเร็วในการสั่นโยก (ถ้ายิ่งเยอะจะยิ่งสั่นถี่และไว)")]
    public float eventShakeSpeed = 25f;
    private float currentEventShake;
    private float shakeIntensityTarget;
    private float shakeFadeSpeed;
    private float shakeTimer;

    [Header("Crouch Audio Settings")]
    public AudioSource audioSource;
    public AudioClip crouchFootstepSound;
    public float crouchStepInterval = 0.6f;
    private float stepTimer;

    [Header("Lean Settings: Mouse")]
    public float mouseLeanAmount = 1.5f;
    public float mouseLeanSpeed = 10f;
    private float currentMouseLean;

    [Header("Lean Settings: Movement (A,D)")]
    public float moveLeanAmount = 2.0f;
    public float moveLeanSpeed = 4f;
    private float currentMoveLean;

    void Start()
    {
        rb = movement.GetComponent<Rigidbody>();
        camTransform = transform;
        defaultYPos = camTransform.localPosition.y;

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        HandleLean();
        HandleBob();
    }

    private void HandleLean()
    {
        // 1. คำนวณ Mouse Lean
        float mouseX = Input.GetAxisRaw("Mouse X");
        float targetMouseLean = -mouseX * mouseLeanAmount;
        currentMouseLean = Mathf.Lerp(currentMouseLean, targetMouseLean, Time.deltaTime * mouseLeanSpeed);

        // 2. คำนวณ Movement Lean (A,D)
        float moveX = Input.GetAxisRaw("Horizontal");
        float targetMoveLean = -moveX * moveLeanAmount;
        currentMoveLean = Mathf.Lerp(currentMoveLean, targetMoveLean, Time.deltaTime * moveLeanSpeed);

        Vector3 vel = rb.linearVelocity;
        float horizontalSpeedSqr = new Vector2(vel.x, vel.z).sqrMagnitude;
        bool isMoving = horizontalSpeedSqr > 0.1f && movement.state != playerMovementRB.movementState.air;

        // 3. คำนวณ Crouch Lean Wobble (ส่ายหัวซ้ายขวาตอนคลาน)
        float targetCrouchWobble = 0f;
        if (playerMovementRB.isCrouching && isMoving)
        {
            targetCrouchWobble = Mathf.Cos(timer) * crouchLeanWobbleAmount;
        }
        currentCrouchWobble = Mathf.Lerp(currentCrouchWobble, targetCrouchWobble, Time.deltaTime * 8f);

        // 4. คำนวณ Run Lean Wobble (ส่ายหัวซ้ายขวาตอนวิ่งสับ)
        float targetRunWobble = 0f;
        if (playerMovementRB.isRuning && isMoving)
        {
            targetRunWobble = Mathf.Cos(timer) * runLeanWobbleAmount;
        }
        currentRunWobble = Mathf.Lerp(currentRunWobble, targetRunWobble, Time.deltaTime * 10f);

        // 5. [เพิ่มเข้ามา] คำนวณคำสั่งสั่นจาก Unity Event (สั่นแบบสุ่มองศาเอียงซ้ายขวา)
        if (shakeIntensityTarget > 0.01f)
        {
            shakeTimer += Time.deltaTime * eventShakeSpeed;
            // ใช้ Mathf.Sin เพื่อสร้างคลื่นสั่นซ้ายขวาคูณความแรง และค่อยๆ ลดความแรงลงตามเวลา (Fade)
            currentEventShake = Mathf.Sin(shakeTimer) * shakeIntensityTarget;
            shakeIntensityTarget = Mathf.MoveTowards(shakeIntensityTarget, 0f, Time.deltaTime * shakeFadeSpeed);
        }
        else
        {
            currentEventShake = Mathf.Lerp(currentEventShake, 0f, Time.deltaTime * 10f);
            shakeTimer = 0f;
        }

        // รวมค่า Lean ทั้งหมด (Mouse + Move + คลานโยก + วิ่งโยก + สั่นจาก Event) ส่งไปที่ zRotation
        float targetTotalLean = currentMouseLean + currentMoveLean + currentCrouchWobble + currentRunWobble + currentEventShake;
        mouseLookScript.zRotation = Mathf.Lerp(mouseLookScript.zRotation, targetTotalLean, Time.deltaTime * 10f);
    }

    private void HandleBob()
    {
        Vector3 vel = rb.linearVelocity;
        float horizontalSpeedSqr = new Vector2(vel.x, vel.z).sqrMagnitude;

        if (horizontalSpeedSqr > 0.1f && movement.state != playerMovementRB.movementState.air)
        {
            float speedFactor = 1f;
            float currentBobSpeed = walkBobSpeed;
            float currentBobAmount = walkBobAmount;

            if (playerMovementRB.isRuning)
            {
                speedFactor = 1.4f;
                currentBobSpeed = walkBobSpeed * speedFactor;
                currentBobAmount = walkBobAmount * speedFactor;
            }
            else if (playerMovementRB.isCrouching)
            {
                currentBobSpeed = crouchBobSpeed;
                currentBobAmount = crouchBobAmount;
                HandleCrouchSound();
            }

            timer += Time.deltaTime * currentBobSpeed;
            float newY = defaultYPos + Mathf.Sin(timer) * currentBobAmount;
            camTransform.localPosition = new Vector3(0, newY, camTransform.localPosition.z);
        }
        else
        {
            timer = 0;
            stepTimer = 0;
            Vector3 targetPos = new Vector3(0, defaultYPos, camTransform.localPosition.z);
            camTransform.localPosition = Vector3.Lerp(camTransform.localPosition, targetPos, Time.deltaTime * 5f);
        }
    }

    private void HandleCrouchSound()
    {
        stepTimer += Time.deltaTime;
        if (stepTimer >= crouchStepInterval)
        {
            PlayCrouchFootstep();
            stepTimer = 0f;
        }
    }

    private void PlayCrouchFootstep()
    {
        if (crouchFootstepSound != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(0.85f, 1.1f);
            audioSource.PlayOneShot(crouchFootstepSound);
        }
        else
        {
            Debug.Log("ทำการเล่นเสียงคลานแล้วไง ไอโง่");
        }
    }

    public void ApplyPickupLean(float amount)
    {
        mouseLookScript.zRotation += amount;
    }

    // ==========================================
    // [เพิ่มเข้ามา] FUNCTIONS FOR UNITY EVENTS
    // ==========================================

    /// <summary>
    /// สั่งสั่นกล้องแบบหมุนเอียง (Lean Shake) สามารถพิมพ์เรียกจาก Unity Event ได้โดยตรง
    /// </summary>
    /// <param name="intensity">ความแรงในการเอียงสั่น (แนะนำ 5 - 15)</param>
    /// <param name="duration">ระยะเวลาของการสั่นรวม (วินาที)</param>
    public void TriggerLeanShake(float intensity, float duration)
    {
        shakeIntensityTarget = intensity;
        // คำนวณความเร็วในการจางหาย: ความแรง / ระยะเวลา
        shakeFadeSpeed = duration > 0f ? (intensity / duration) : intensity;
        shakeTimer = 0f;
    }

    /// <summary>
    /// ฟังก์ชันทางเลือกสั้นๆ สำหรับ Unity Event ที่ไม่ต้องการส่งพารามิเตอร์หลายตัว (ใช้ค่า Default ภายใน)
    /// </summary>
    public void TriggerDefaultLeanShake()
    {
        TriggerLeanShake(8f, 0.5f); // ความแรง 8, นาน 0.5 วินาที
    }
}