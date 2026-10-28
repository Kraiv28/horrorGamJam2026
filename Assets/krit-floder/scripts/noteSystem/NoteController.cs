using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class NoteController : MonoBehaviour
{
    [Header("Note Content")]
    [TextArea(3, 10)]
    public string noteTextContent;         // ข้อความเฉพาะของโน้ตชิ้นนี้ (ตั้งค่าแยกกันได้ในแต่ละ Prefab)

    [Header("Input Settings")]
    public KeyCode closeKey = KeyCode.Mouse1; // ปุ่มสำหรับปิดโน้ต (ค่าเริ่มต้น: คลิกขวา)

    [Header("Events & Audio")]
    public UnityEvent onOpenEvent;         // เหตุการณ์เพิ่มเติม เช่น เล่นเสียงเปิดกระดาษ

    // อ้างอิงระบบภายใน (ค้นหาอัตโนมัติ ไม่ต้องลากใส่)
    private static GameObject sharedNoteCanvas;
    private static TMP_Text sharedNoteTextArea;
    private static playerMovementRB playerMovement;

    private static NoteController activeNote;    // เก็บสถานะว่าตอนนี้โน้ตชิ้นไหนกำลังเปิดอยู่
    private bool isOpen = false;

    private void Start()
    {
        // 1. ค้นหา Player อัตโนมัติ (ถ้ายังไม่มี)
        if (playerMovement == null)
        {
            playerMovement = FindObjectOfType<playerMovementRB>();
        }

        // 2. ค้นหา Canvas และ Text จากชื่อ GameObject ใน Scene อัตโนมัติ 
        // (ช่วยให้เราทำระบบ UI โน้ตไว้ที่ Canvas หลักอันเดียว แล้วแชร์ให้ทุก Prefab ใช้ร่วมกันได้)
        if (sharedNoteCanvas == null)
        {
            GameObject canvasObj = GameObject.Find("NoteCanvas"); // <--- ชื่อ GameObject ของ Canvas โน้ตในจอ UI
            if (canvasObj != null)
            {
                sharedNoteCanvas = canvasObj;
                // ปิด Canvas ไว้ก่อนตอนเริ่มเกม
                sharedNoteCanvas.SetActive(false);
            }
        }

        if (sharedNoteTextArea == null && sharedNoteCanvas != null)
        {
            sharedNoteTextArea = sharedNoteCanvas.GetComponentInChildren<TMP_Text>();
        }
    }

    private void Update()
    {
        // ถ้าโน้ตตัวนี้เปิดอยู่ และกดปุ่มปิด ให้ทำการปิด
        if (isOpen && Input.GetKeyDown(closeKey))
        {
            DisableNote();
        }
    }

    public void ShowNote()
    {
        // ถ้ามีโน้ตตัวอื่นเปิดอยู่ ให้ปิดตัวมี่เปิดอยู่ก่อน
        if (activeNote != null && activeNote != this)
        {
            activeNote.DisableNote();
        }

        // ค้นหา UI ซ้ำอีกรอบเผื่อกรณีโหลดฉากใหม่
        if (sharedNoteCanvas == null)
        {
            GameObject canvasObj = GameObject.Find("NoteCanvas");
            if (canvasObj != null)
            {
                sharedNoteCanvas = canvasObj;
                sharedNoteTextArea = sharedNoteCanvas.GetComponentInChildren<TMP_Text>();
            }
        }

        if (sharedNoteCanvas != null && sharedNoteTextArea != null)
        {
            sharedNoteTextArea.text = noteTextContent; // ดึงข้อความของ Prefab นี้มาแสดง
            sharedNoteCanvas.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[NoteController] ไม่พบ 'NoteCanvas' ใน Scene! กรุณาตรวจสอบชื่อ Canvas UI ของคุณ");
        }

        // เล่นเสียงเปิดกระดาษ
        onOpenEvent?.Invoke();

        // ล็อคการเคลื่อนไหวของผู้เล่น
        if (playerMovement != null)
        {
            playerMovement.SetCutsceneSlow(true, 0f);
        }

        isOpen = true;
        activeNote = this;
    }

    public void DisableNote()
    {
        if (sharedNoteCanvas != null)
        {
            sharedNoteCanvas.SetActive(false);
        }

        // คืนค่าการเคลื่อนไหวให้ผู้เล่น
        if (playerMovement != null)
        {
            playerMovement.SetCutsceneSlow(false, 1f);
        }

        isOpen = false;
        if (activeNote == this)
        {
            activeNote = null;
        }
    }
}