using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class TransitionDoor : MonoBehaviour
{
    [Header("Position Settings")]
    [Tooltip("จุดที่ผู้เล่นจะถูกวาปเข้าไป (เช่น ตำแหน่งข้างในห้อง)")]
    public Transform roomPosition;
    [Tooltip("จุดที่ผู้เล่นจะถูกวาปกลับออกมา (หน้าประตูเดิม)")]
    public Transform outsidePosition;

    [Header("Condition Settings")]
    [Tooltip("เงื่อนไขถูกต้องหรือไม่ (สามารถผูกค่าผ่าน Unity Event ได้)")]
    public bool isConditionMet = false;
    [Tooltip("Unity Event จะถูกเรียกเมื่อผู้เล่นพยายามเปิดแต่เงื่อนไขยังไม่ผ่าน")]
    public UnityEvent onConditionFailed;

    [Header("Delay Settings")]
    [Tooltip("เวลาดีเลย์ก่อนวาปเข้า/ออก (วินาที)")]
    public float delayDuration = 2.5f;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    [Tooltip("1. เสียงตอนคลิกประตูแต่เงื่อนไขยังไม่ถูกต้อง (เสียงประตูล็อค)")]
    public AudioClip lockedSound;
    [Tooltip("2. เสียงเปิดประตูก่อนเริ่ม Delay")]
    public AudioClip openDoorSound;
    [Tooltip("3. เสียงปิดประตู เมื่อหมด Delay และวาปเสร็จ")]
    public AudioClip closeDoorSound;

    // สถานะเช็คว่าผู้เล่นอยู่ในห้องหรือยัง
    private bool isPlayerInside = false;
    private bool isTransitioning = false;

    /// <summary>
    /// ฟังก์ชันหลักที่จะถูกเรียกจาก playerInteraction เมื่อผู้เล่นคลิกที่ประตู
    /// </summary>
    public void InteractWithDoor()
    {
        if (isTransitioning) return;

        if (!isPlayerInside)
        {
            // === ขาเข้าห้อง ===
            if (!isConditionMet)
            {
                // เงื่อนไขยังไม่ถูกต้อง -> เล่นเสียงล็อค และเรียก Event แจ้งเตือน
                if (audioSource != null && lockedSound != null)
                {
                    audioSource.PlayOneShot(lockedSound);
                }
                onConditionFailed?.Invoke();
                Debug.Log("[TransitionDoor] ประตูล็อค! เงื่อนไขยังไม่ถูกต้อง");
                return;
            }

            // เงื่อนไขถูกต้อง -> เริ่มกระบวนการเข้าห้อง
            StartCoroutine(TransitionRoutine(roomPosition.position, true));
        }
        else
        {
            // === ขาออกจากห้อง ===
            StartCoroutine(TransitionRoutine(outsidePosition.position, false));
        }
    }

    /// <summary>
    /// Coroutine จัดการเรื่อง Delay, ล็อกการขยับ/หัน, เสียง และการวาปเฉพาะ PlayerOBJ
    /// </summary>
    IEnumerator TransitionRoutine(Vector3 targetPosition, bool enteringRoom)
    {
        isTransitioning = true;

        // 1. เล่นเสียงเปิดประตูก่อนเริ่ม Delay
        if (audioSource != null && openDoorSound != null)
        {
            audioSource.PlayOneShot(openDoorSound);
        }

        // 2. ค้นหาผู้เล่นจากสคริปต์ movement โดยตรง (ชัวร์ที่สุด ไม่ต้องพึ่ง Tag)
        playerMovementRB movementScript = FindObjectOfType<playerMovementRB>();
        mouseLook lookScript = null;
        GameObject playerObj = null;

        if (movementScript != null)
        {
            playerObj = movementScript.gameObject;
            lookScript = playerObj.GetComponentInChildren<mouseLook>();

            // ปิดการเคลื่อนที่ทันที
            movementScript.enabled = false;
        }

        // ปิดการหันกล้องทันที
        if (lookScript != null)
        {
            lookScript.canLook = false;
        }

        if (playerObj != null && playerObj.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 3. รอเวลา Delay ตามที่กำหนด (ผู้เล่นจะขยับและหันไม่ได้ในช่วงนี้)
        yield return new WaitForSeconds(delayDuration);

        // 4. ทำการวาปเฉพาะ Object ของผู้เล่นไปที่ตำแหน่งเป้าหมาย
        if (playerObj != null)
        {
            CharacterController charController = playerObj.GetComponent<CharacterController>();
            if (charController != null)
            {
                charController.enabled = false;
            }

            // วาปเฉพาะ PlayerOBJ หลัก
            playerObj.transform.position = targetPosition;

            if (charController != null)
            {
                charController.enabled = true;
            }
        }

        // สลับสถานะเข้า/ออก
        isPlayerInside = enteringRoom;

        // 5. เล่นเสียงปิดประตูหลังจากวาปเสร็จ
        if (audioSource != null && closeDoorSound != null)
        {
            audioSource.PlayOneShot(closeDoorSound);
        }

        // 6. คืนค่าการเคลื่อนไหวและการหันกล้องให้ผู้เล่น
        if (movementScript != null)
        {
            movementScript.enabled = true;
        }

        if (lookScript != null)
        {
            lookScript.canLook = true;
        }

        isTransitioning = false;
    }

    /// <summary>
    /// ฟังก์ชันสำหรับให้ Unity Event ด้านนอกสั่งเปลี่ยนค่าเงื่อนไขให้ถูกต้อง (isConditionMet = true)
    /// </summary>
    public void SetConditionMet(bool status)
    {
        isConditionMet = status;
    }
}