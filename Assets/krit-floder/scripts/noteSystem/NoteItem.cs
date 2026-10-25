using UnityEngine;

public class NoteItem : MonoBehaviour
{
    [Header("Note Content")]
    [TextArea(5, 10)]
    [Tooltip("ข้อความที่จะให้แสดงบนกระดาษโน้ตเวลาเปิดอ่าน")]
    public string noteContent = "เนื้อหาในกระดาษโน้ต...";

    [Header("Note Image (Optional)")]
    [Tooltip("ลากรูปภาพ Sprite จาก Asset มาใส่ที่นี่ (ถ้ามี) ถ้าไม่ใส่จะซ่อนช่องรูปภาพ")]
    public Sprite noteImageSprite; // <--- เพิ่มตัวแปรเก็บรูปภาพประจำโน้ตชิ้นนี้

    [Header("Audio Settings")]
    [Tooltip("เสียงกระดาษตอนหยิบขึ้นมาอ่าน (ถ้ามี)")]
    public AudioClip readSound;

    public void ReadNote()
    {
        if (readSound != null)
        {
            AudioSource.PlayClipAtPoint(readSound, transform.position);
        }

        NoteUIController uiController = FindFirstObjectByType<NoteUIController>(FindObjectsInactive.Include);

        if (uiController != null)
        {
            // ส่งทั้งข้อความและรูปภาพไปเปิดที่ UI
            uiController.OpenNote(noteContent, noteImageSprite);
            Debug.Log("[NoteItem] เปิดอ่านโน้ตสำเร็จ");
        }
        else
        {
            Debug.LogWarning("[NoteItem] ไม่พบ NoteUIController ใน Scene!");
        }
    }
}