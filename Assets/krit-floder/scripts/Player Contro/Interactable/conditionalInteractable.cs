using UnityEngine;
using UnityEngine.Events; // สำหรับใช้ UnityEvents

public class conditionalInteractable : MonoBehaviour
{
    [Header("Requirement")]
    public int requiredItemId = 2; // ID ของไอเทมที่ต้องใช้ (เช่น กุญแจ ID 2)
    public string messageOnMissing = "You need a key to open this door.";

    [Header("Events")]
    public UnityEvent onSuccess; // ลากฟังก์ชันที่ต้องการให้ทำงานมาใส่ใน Inspector (เช่น อนิเมชั่นเปิดประตู)

    // ฟังก์ชันที่จะถูกเรียกจาก Player
    public void Interact(int heldItemId)
    {
        if (heldItemId == requiredItemId)
        {
            Debug.Log("Condition Met! Performing Event...");
            onSuccess.Invoke(); // สั่งให้ Event ทำงาน
        }
        else
        {
            // ถ้า ID ไม่ตรง หรือไม่ได้ถืออะไรอยู่ (currentHeldItemId เป็น -1)
            Debug.Log("<color=red>Command Console:</color> " + messageOnMissing);
        }
    }

    public void SetToDisappear()
    {
        gameObject.SetActive(false);
    }
}