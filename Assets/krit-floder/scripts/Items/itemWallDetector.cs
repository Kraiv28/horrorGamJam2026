using UnityEngine;

public class itemWallDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    public Transform raycastOrigin;    // ลาก WallCheckPoint มาใส่
    public float detectionRange = 0.5f; // ระยะที่เริ่มตรวจจับกำแพง
    public LayerMask wallLayer;        // เลือก Layer ที่เป็นกำแพง (เช่น Default)

    [Header("Movement Settings")]
    public Vector3 pullBackOffset = new Vector3(0, -0.2f, -0.4f); // ทิศทางที่จะหดมือกลับ (ลงและถอยหลัง)
    public float smoothSpeed = 10f;    // ความนุ่มนวลในการขยับ

    private Vector3 initialLocalPos;   // ตำแหน่งเริ่มต้นของ ItemHolder

    void Start()
    {
        // บันทึกตำแหน่งปกติไว้
        initialLocalPos = transform.localPosition;
    }

    void Update()
    {
        CheckForWall();
    }

    void CheckForWall()
    {
        RaycastHit hit;
        Vector3 targetPos;

        // ยิง Raycast ไปข้างหน้าจากจุดที่กำหนด
        if (Physics.Raycast(raycastOrigin.position, raycastOrigin.forward, out hit, detectionRange, wallLayer))
        {
            // ถ้าชนกำแพง: คำนวณตำแหน่งเป้าหมาย (ถอยหลังกลับ)
            // ยิ่งเข้าใกล้กำแพงมาก ยิ่งถอยกลับมาก
            float hitDistance = hit.distance;
            float strength = 1f - (hitDistance / detectionRange);

            targetPos = initialLocalPos + (pullBackOffset * strength);

            // วาดเส้น Debug ใน Scene view เพื่อดูระยะ (สีแดงคือชน)
            Debug.DrawRay(raycastOrigin.position, raycastOrigin.forward * detectionRange, Color.red);
        }
        else
        {
            // ถ้าไม่ชน: กลับไปตำแหน่งเดิม
            targetPos = initialLocalPos;

            // สีเขียวคือไม่ชน
            Debug.DrawRay(raycastOrigin.position, raycastOrigin.forward * detectionRange, Color.green);
        }

        // ขยับ ItemHolder อย่างนุ่มนวลด้วย Lerp
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * smoothSpeed);
    }
}