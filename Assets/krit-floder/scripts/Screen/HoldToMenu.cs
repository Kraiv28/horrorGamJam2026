using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // ใช้สำหรับสั่งเปิด/ปิด Progress Bar (ถ้ามี)

public class HoldToMenu : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // ชื่อ Scene หน้าเมนูหลัก
    [SerializeField] private float holdDuration = 1.5f;             // เวลาที่ต้องกดค้าง (วินาที)

    [Header("Optional UI (Progress Bar)")]
    [SerializeField] private Image progressBarImage;                // หลอดแสดงเวลา (ถ้าไม่ใช้ให้เว้นว่างไว้ได้)

    private float holdTimer = 0f;
    private bool isHolding = false;

    void Update()
    {
        // ตรวจสอบว่ากำลังกดปุ่ม ESC ค้างไว้หรือไม่
        if (Input.GetKey(KeyCode.Escape))
        {
            if (!isHolding)
            {
                isHolding = true;
                holdTimer = 0f;
            }

            // นับเวลาเพิ่มเรื่อยๆ
            holdTimer += Time.unscaledDeltaTime; // ใช้ unscaledDeltaTime เผื่อเกมถูกหยุดเวลาไว้บางส่วน

            // อัปเดตหลอด UI (ถ้ามีใส่ไว้)
            if (progressBarImage != null)
            {
                progressBarImage.gameObject.SetActive(true);
                progressBarImage.fillAmount = holdTimer / holdDuration;
            }

            // ถ้านับเวลาครบตามกำหนดแล้ว
            if (holdTimer >= holdDuration)
            {
                ReturnToMainMenu();
            }
        }
        else
        {
            // ถ้าปล่อยปุ่ม ESC ก่อนครบเวลา ให้รีเซ็ตค่าทั้งหมด
            if (isHolding)
            {
                ResetHold();
            }
        }
    }

    void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // คืนค่าเวลาปกติก่อนเปลี่ยน Scene
        SceneManager.LoadScene(mainMenuSceneName);
    }

    void ResetHold()
    {
        isHolding = false;
        holdTimer = 0f;

        if (progressBarImage != null)
        {
            progressBarImage.fillAmount = 0f;
            progressBarImage.gameObject.SetActive(false);
        }
    }
}