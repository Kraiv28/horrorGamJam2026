using UnityEngine;
using UnityEngine.UI; // สำหรับ UI Image
using UnityEngine.Events;
using System.Collections;

public class SeparateTransitionDoor : MonoBehaviour
{
    public enum DoorType { Entrance, Exit }

    [Header("Door Role Settings")]
    [Tooltip("เลือกประเภทของประตู: Entrance = ประตูทางเข้าห้อง, Exit = ประตูทางออกห้อง")]
    public DoorType doorType;

    [Header("Teleport Target")]
    [Tooltip("ถ้าเป็นประตูเข้า: จุดที่จะวาปผู้เล่นไปในห้อง / ถ้าเป็นประตูออก: จุดที่จะวาปผู้เล่นกลับมาข้างนอก")]
    public Transform targetPosition;

    [Header("Condition (Only for Entrance)")]
    [Tooltip("เงื่อนไขถูกต้องหรือไม่ (ใช้เฉพาะประตูทางเข้า)")]
    public bool isConditionMet = false;
    [Tooltip("Unity Event จะถูกเรียกเมื่อพยายามเข้าแต่เงื่อนไขยังไม่ผ่าน")]
    public UnityEvent onConditionFailed;
    [Tooltip("Unity Event จะถูกเรียกเมื่อเงื่อนไขถูกต้องและสามารถเข้าได้แล้ว")]
    public UnityEvent onConditionPassed;

    [Header("Delay Settings")]
    [Tooltip("เวลาดีเลย์ก่อนวาป (วินาที)")]
    public float delayDuration = 2.5f;

    [Header("Screen Fade Settings")]
    [Tooltip("ลาก UI Image สีดำมาใส่ตรงนี้")]
    public Image fadeImage;
    [Tooltip("เวลาที่ใช้ในการเฟดมืดลง (ควรน้อยกว่าหรือเท่ากับ delayDuration)")]
    public float fadeOutTime = 0.5f;
    [Tooltip("เวลาที่ใช้ในการเฟดสว่างกลับมา")]
    public float fadeInTime = 0.5f;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    [Tooltip("1. เสียงตอนคลิกประตูแต่เงื่อนไขยังไม่ถูกต้อง (เสียงประตูล็อค) - ใช้เฉพาะประตูเข้า")]
    public AudioClip lockedSound;
    [Tooltip("2. เสียงเปิดประตูก่อนเริ่ม Delay")]
    public AudioClip openDoorSound;
    [Tooltip("3. เสียงปิดประตู เมื่อหมด Delay และวาปเสร็จ")]
    public AudioClip closeDoorSound;

    // เปลี่ยนจาก static เป็นตัวแปรธรรมดา เพื่อแยกอิสระแต่ละประตู และป้องกันค้างข้ามบาน
    private bool isTransitioning = false;

    // ป้องกันกรณี Script ถูกปิดใช้งานกลางคันแล้วตัวแปรค้าง
    private void OnDisable()
    {
        isTransitioning = false;
    }

    /// <summary>
    /// ฟังก์ชันหลักที่จะถูกเรียกจาก playerInteraction เมื่อผู้เล่นคลิกที่ประตู
    /// </summary>
    public void InteractWithDoor()
    {
        if (isTransitioning) return;

        if (doorType == DoorType.Entrance)
        {
            // === ระบบประตูขาเข้า ===
            if (!isConditionMet)
            {
                if (audioSource != null && lockedSound != null)
                {
                    audioSource.PlayOneShot(lockedSound);
                }
                onConditionFailed?.Invoke();
                Debug.Log("[TransitionDoor] ประตูล็อค! เงื่อนไขยังไม่ถูกต้อง");
                return;
            }

            onConditionPassed?.Invoke();

            if (targetPosition != null)
            {
                StartCoroutine(TransitionRoutine(targetPosition.position));
            }
            else
            {
                Debug.LogWarning("[TransitionDoor] ยังไม่ได้กำหนด Target Position ให้ประตูเข้า!");
            }
        }
        else if (doorType == DoorType.Exit)
        {
            // === ระบบประตูขาออก (ไม่ต้องเช็คเงื่อนไข) ===
            if (targetPosition != null)
            {
                StartCoroutine(TransitionRoutine(targetPosition.position));
            }
            else
            {
                Debug.LogWarning("[TransitionDoor] ยังไม่ได้กำหนด Target Position ให้ประตูออก!");
            }
        }
    }

    /// <summary>
    /// Coroutine จัดการเรื่อง Delay, การเฟดหน้าจอมืดด้วย Image, ล็อกการขยับ/หัน, เสียง และการวาป
    /// </summary>
    IEnumerator TransitionRoutine(Vector3 destination)
    {
        isTransitioning = true;

        // 1. เล่นเสียงเปิดประตูก่อนเริ่ม Delay
        if (audioSource != null && openDoorSound != null)
        {
            audioSource.PlayOneShot(openDoorSound);
        }

        // 2. ล็อกการขยับและหันของผู้เล่นทันทีในช่วง Delay
        playerMovementRB movementScript = FindObjectOfType<playerMovementRB>();
        mouseLook lookScript = null;
        GameObject playerObj = null;

        if (movementScript != null)
        {
            playerObj = movementScript.gameObject;
            lookScript = playerObj.GetComponentInChildren<mouseLook>();
            movementScript.enabled = false; // ปิดการเดิน
        }

        if (lookScript != null)
        {
            lookScript.canLook = false; // ปิดการหันกล้อง
        }

        if (playerObj != null && playerObj.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 3. เริ่มทำการ Fade หน้าจอให้มืดลงผ่าน Image.color.a
        float currentDelayTimer = 0f;
        if (fadeImage != null)
        {
            Color startColor = fadeImage.color;
            float startAlpha = startColor.a;

            while (currentDelayTimer < fadeOutTime)
            {
                currentDelayTimer += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, 1f, currentDelayTimer / fadeOutTime);

                // กำหนดค่า Alpha ใหม่โดยคงค่าสีเดิมไว้
                fadeImage.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }

            // ให้มั่นใจว่ามืดสนิท
            fadeImage.color = new Color(startColor.r, startColor.g, startColor.b, 1f);
        }

        // รอเวลา Delay ส่วนที่เหลือ
        float remainingDelay = delayDuration - fadeOutTime;
        if (remainingDelay > 0)
        {
            yield return new WaitForSeconds(remainingDelay);
        }

        // 4. วาปเฉพาะ Object ของผู้เล่นไปยังตำแหน่งเป้าหมาย
        if (playerObj != null)
        {
            CharacterController charController = playerObj.GetComponent<CharacterController>();
            if (charController != null)
            {
                charController.enabled = false;
            }

            playerObj.transform.position = destination;

            if (charController != null)
            {
                charController.enabled = true;
            }
        }

        // 5. เล่นเสียงปิดประตูหลังจากวาปเสร็จ
        if (audioSource != null && closeDoorSound != null)
        {
            audioSource.PlayOneShot(closeDoorSound);
        }

        // 6. ค่อยๆ เฟดหน้าจอกลับมาสว่างปกติ (Fade In)
        if (fadeImage != null)
        {
            Color startColor = fadeImage.color;
            float fadeTimer = 0f;

            while (fadeTimer < fadeInTime)
            {
                fadeTimer += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, fadeTimer / fadeInTime);

                fadeImage.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }

            // ให้มั่นใจว่าใสสนิท
            fadeImage.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
        }

        // 7. คืนค่าการควบคุมให้ผู้เล่นเดินและหันได้ปกติ
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
    /// ฟังก์ชันสำหรับให้ Unity Event ด้านนอกสั่งเปลี่ยนค่าเงื่อนไขให้ถูกต้อง (ใช้กับประตูเข้า)
    /// </summary>
    public void SetConditionMet(bool status)
    {
        isConditionMet = status;

        if (status)
        {
            onConditionPassed?.Invoke();
        }
    }
}