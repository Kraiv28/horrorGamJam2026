using UnityEngine;
using UnityEngine.UI; // สำคัญ: ต้องใช้ UnityEngine.UI เพื่อควบคุม Image Component
using TMPro;

public class NoteUIController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject noteCanvasGroup;
    public TMP_Text noteTextComponent;

    [Header("Image UI Reference")]
    [Tooltip("ลาก GameObject 'NoteImage' ใน Hierarchy มาใส่ช่องนี้")]
    public Image noteImageComponent; // <--- ช่องสำหรับควบคุม UI Image

    private bool isReading = false;

    void Start()
    {
        if (noteCanvasGroup != null)
        {
            noteCanvasGroup.SetActive(false);
        }
    }

    void Update()
    {
        if (isReading && (Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.Mouse0)))
        {
            CloseNote();
        }
    }

    // ปรับฟังก์ชัน OpenNote ให้รับค่า Sprite เพิ่มเข้ามาด้วย
    public void OpenNote(string content, Sprite noteSprite)
    {
        if (noteCanvasGroup != null && noteTextComponent != null)
        {
            noteTextComponent.text = content;

            // จัดการเรื่องรูปภาพ
            if (noteImageComponent != null)
            {
                if (noteSprite != null)
                {
                    noteImageComponent.gameObject.SetActive(true); // เปิดใช้งาน Image Object
                    noteImageComponent.sprite = noteSprite;       // เปลี่ยนรูปตามที่ส่งมา
                }
                else
                {
                    noteImageComponent.gameObject.SetActive(false); // ถ้าโน้ตแผ่นไหนไม่มีรูป ให้ซ่อนกรอบรูปไปเลย
                }
            }

            noteCanvasGroup.SetActive(true);
            isReading = true;

            mouseLook playerCam = FindFirstObjectByType<mouseLook>();
            if (playerCam != null)
            {
                playerCam.canLook = false;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void CloseNote()
    {
        if (noteCanvasGroup != null)
        {
            noteCanvasGroup.SetActive(false);
            isReading = false;

            mouseLook playerCam = FindFirstObjectByType<mouseLook>();
            if (playerCam != null)
            {
                playerCam.canLook = true;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}