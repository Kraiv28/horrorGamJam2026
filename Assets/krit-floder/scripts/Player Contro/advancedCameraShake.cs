using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class advancedCameraShake : MonoBehaviour
{
    private Vector3 originalPos;
    private Quaternion originalRot;

    void Awake()
    {
        // บันทึกค่าเริ่มต้นของตำแหน่งและการหมุน (Local)
        originalPos = transform.localPosition;
        originalRot = transform.localRotation;
    }

    void Update()
    {
        // --- ส่วนทดสอบ: กดปุ่มเพื่อลองสั่น ---

        // แบบที่ 1: ใช้ New Input System (แนะนำ)
        //if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        //{
        //    // ทดสอบสั่นแบบ "ระเบิดลง" (สั่นแรงและนาน)
        //    Shake(0.5f, 0.2f, 3.0f);
        //    Debug.Log("Testing Shake: Explosion Effect");
        //}

        //// แบบที่ 2: ใช้ Legacy Input (เผื่อโปรเจกต์ตั้งค่าแบบ Both)
        //if (Input.GetKeyDown(KeyCode.G))
        //{
        //    // ทดสอบสั่นแบบ "ยิงปืน" (สั่นเบาและเร็ว)
        //    Shake(0.1f, 0.05f, 0.5f);
        //    Debug.Log("Testing Shake: Gunfire Effect");
        //}
    }

    public void Shake(float duration, float positionMagnitude, float rotationMagnitude)
    {
        StopAllCoroutines(); // หยุดการสั่นเก่าก่อนถ้ามีการเรียกซ้ำ
        StartCoroutine(ShakeCoroutine(duration, positionMagnitude, rotationMagnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float posMag, float rotMag)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // 1. คำนวณความแรงที่ค่อยๆ ลดลงตามเวลา (Fade Out)
            // เพื่อให้ช่วงท้ายของการสั่นดูนุ่มนวล ไม่ตัดฉับ
            float damper = 1.0f - (elapsed / duration);

            // 2. สุ่มค่าตำแหน่ง (Position)
            float posX = Random.Range(-5f, 5f) * posMag * damper;
            float posY = Random.Range(-5f, 5f) * posMag * damper;
            Vector3 targetPos = new Vector3(originalPos.x + posX, originalPos.y + posY, originalPos.z);

            // 3. สุ่มค่าการหมุน (Rotation) - ใช้ Euler Angles
            float rotX = Random.Range(-5f, 5f) * rotMag * damper;
            float rotY = Random.Range(-5f, 5f) * rotMag * damper;
            float rotZ = Random.Range(-5f, 5f) * rotMag * damper;
            Quaternion targetRot = originalRot * Quaternion.Euler(rotX, rotY, rotZ);

            // 4. ใช้ SmoothStep หรือ Lerp เพื่อให้การขยับนุ่มนวล (Smooth)
            // แทนที่จะเปลี่ยนค่าทันที เราจะค่อยๆ เคลื่อนเข้าหาจุดสุ่มนั้น
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * 15f);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, Time.deltaTime * 15f);

            elapsed += Time.deltaTime;
            yield return null; // รอเฟรมถัดไป
        }

        // 5. เมื่อจบแล้ว ให้ค่อยๆ คืนค่ากลับสู่ตำแหน่งปกติอย่างนุ่มนวล
        float returnElapsed = 0f;
        while (returnElapsed < 0.1f)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPos, returnElapsed / 0.1f);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, originalRot, returnElapsed / 0.1f);
            returnElapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
        transform.localRotation = originalRot;
    }
}