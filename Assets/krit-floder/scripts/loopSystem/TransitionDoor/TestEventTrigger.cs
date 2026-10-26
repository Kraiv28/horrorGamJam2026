using UnityEngine;
using UnityEngine.Events;

public class TestEventTrigger : MonoBehaviour
{
    [Header("Test Event Settings")]
    [Tooltip("ลากฟังก์ชันหรือ Event ที่ต้องการให้ทดสอบการทำงานมาใส่ที่นี่")]
    public UnityEvent onTestClicked;

    [Header("Debug Settings")]
    public string debugMessage = "[TestEventTrigger] คลิกวัตถุทดสอบสำเร็จและรัน Unity Event แล้ว!";

    /// <summary>
    /// ฟังก์ชันที่จะถูกเรียกเมื่อผู้เล่นคลิกวัตถุนี้
    /// </summary>
    public void Interact()
    {
        // 1. แสดงข้อความ Debug ใน Console
        Debug.Log(debugMessage);

        // 2. สั่งรัน Unity Event ทั้งหมดที่ตั้งค่าไว้ใน Inspector
        if (onTestClicked != null)
        {
            onTestClicked.Invoke();
        }
    }
}