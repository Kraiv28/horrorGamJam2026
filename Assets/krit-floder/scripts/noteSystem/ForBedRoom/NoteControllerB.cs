using UnityEngine;

public class NoteControllerB : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("ลาก Canvas หรือ Panel ของ Note UI มาใส่ตรงนี้")]
    public GameObject noteUI;

    [Header("Player References")]
    [Tooltip("ลาก GameObject ของ Player (ที่มี Rigidbody หรือ Script ควบคุมการเคลื่อนที่) มาใส่")]
    public MonoBehaviour playerMovementScript; // รองรับ playerMovementRB

    [Tooltip("ลาก Component mouseLook ของกล้องมาใส่")]
    public mouseLook playerMouseLook;

    private bool isReading = false;

    private void Start()
    {
        // ซ่อน Note UI ไว้ตั้งแต่เริ่มเกม
        if (noteUI != null)
        {
            noteUI.SetActive(false);
        }
    }

    private void Update()
    {
        // ถ้ากำลังอ่านอยู่ และผู้เล่นคลิกขวา (Mouse 1) ให้ปิด Note
        if (isReading && Input.GetKeyDown(KeyCode.Mouse1))
        {
            HideNote();
        }
    }

    /// <summary>
    /// ฟังก์ชันเรียกเพื่อเปิดหน้าต่าง Note
    /// </summary>
    public void ShowNote()
    {
        if (noteUI == null) return;

        isReading = true;
        noteUI.SetActive(true);

        // 1. ปิดการเคลื่อนที่ของผู้เล่น
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false;
        }

        // 2. ปิดการหันกล้อง
        if (playerMouseLook != null)
        {
            playerMouseLook.canLook = false;
        }

        // 3. ปลดล็อกและแสดงเมาส์เพื่อให้ผู้เล่นเห็นหน้าจอ UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("[Note] เปิดโน้ตสำเร็จ (ล็อกการเคลื่อนที่และการหันหน้าจอ)");
    }

    /// <summary>
    /// ฟังก์ชันเรียกเพื่อปิดหน้าต่าง Note และคืนค่าการควบคุม
    /// </summary>
    public void HideNote()
    {
        if (noteUI == null) return;

        isReading = false;
        noteUI.SetActive(false);

        // 1. เปิดการเคลื่อนที่ของผู้เล่นกลับมา
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
        }

        // 2. เปิดการหันกล้องกลับมา
        if (playerMouseLook != null)
        {
            playerMouseLook.canLook = true;
        }

        // 3. ซ่อนและล็อกเมาส์กลับไปเป็นรูปแบบเล่นเกม
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("[Note] ปิดโน้ตสำเร็จ (คืนค่าการควบคุมทั้งหมด)");
    }
}