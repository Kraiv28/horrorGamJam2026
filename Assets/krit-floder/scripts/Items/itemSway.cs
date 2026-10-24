using UnityEngine;

public class itemSway : MonoBehaviour
{
    [Header("Dependencies")]
    public playerMovementRB movement;

    [Header("Sway Settings")]
    public float swayAmount = 0.02f;
    public float maxSwayAmount = 0.06f;
    public float swaySmoothness = 8f;

    [Header("Rotation Sway")]
    public float rotationSwayAmount = 4f;
    public float rotationSmoothness = 5f;

    [Header("Item Bobbing (Idle/Run)")]
    public float bobSpeed = 2f;
    public float bobAmount = 0.01f;

    [Header("Run Modifiers")]
    public float runSwayMultiplier = 2.5f; // เมื่อวิ่งจะเหวี่ยงแรงขึ้นกี่เท่า
    public float runBobMultiplier = 3.0f;  // เมื่อวิ่งจะกระเพื่อมแรงขึ้นกี่เท่า
    public float runBobSpeedMultiplier = 2.0f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float currentBobTimer;

    void Start()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    void Update()
    {
        // ตรวจสอบว่ากำลังวิ่งและขยับตัวอยู่จริงหรือไม่
        bool isMoving = movement.GetComponent<Rigidbody>().linearVelocity.magnitude > 0.1f;
        bool isCurrentlyRunning = playerMovementRB.isRuning && isMoving;

        ApplySway(isCurrentlyRunning);
        ApplyBobbing(isCurrentlyRunning);
    }

    private void ApplySway(bool isRunning)
    {
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        // ถ้าวิ่ง ให้คูณแรงเหวี่ยงเข้าไป
        float multiplier = isRunning ? runSwayMultiplier : 1f;

        float moveX = -mouseX * swayAmount * multiplier;
        float moveY = -mouseY * swayAmount * multiplier;

        moveX = Mathf.Clamp(moveX, -maxSwayAmount * multiplier, maxSwayAmount * multiplier);
        moveY = Mathf.Clamp(moveY, -maxSwayAmount * multiplier, maxSwayAmount * multiplier);

        Vector3 targetPosition = new Vector3(moveX, moveY, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, initialPosition + targetPosition, Time.deltaTime * swaySmoothness);

        // Rotation Sway
        float tiltY = mouseX * rotationSwayAmount * multiplier;
        float tiltX = mouseY * rotationSwayAmount * multiplier;

        Quaternion targetRotation = Quaternion.Euler(initialRotation.eulerAngles.x - tiltX, initialRotation.eulerAngles.y + tiltY, 0);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * rotationSmoothness);
    }

    private void ApplyBobbing(bool isRunning)
    {
        // ปรับความเร็วและระยะการ Bob ตามสถานะการวิ่ง
        float speedMod = isRunning ? runBobSpeedMultiplier : 1f;
        float amountMod = isRunning ? runBobMultiplier : 1f;

        currentBobTimer += Time.deltaTime * bobSpeed * speedMod;

        float newY = Mathf.Sin(currentBobTimer) * (bobAmount * amountMod);
        float newX = Mathf.Cos(currentBobTimer * 0.5f) * (bobAmount * amountMod * 0.5f); // เพิ่มการส่ายซ้ายขวาเล็กน้อยตอนวิ่ง

        // ใช้ Lerp เพื่อให้การเปลี่ยนจากเดินไปวิ่งนุ่มนวล
        Vector3 bobTarget = initialPosition + new Vector3(newX, newY, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, bobTarget, Time.deltaTime * 10f);
    }
}