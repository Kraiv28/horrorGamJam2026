using UnityEngine;
using UnityEngine.Events;

public class TriggerEventTrigger : MonoBehaviour
{
    [Header("Tag ที่อนุญาตให้ทริกเกอร์ (ปล่อยว่างถ้าให้ทุกอย่างทริกเกอร์ได้)")]
    [SerializeField] private string targetTag = "Player";

    [Header("ตั้งค่าว่าจะให้ทำงานครั้งเดียวแล้วปิดไหม")]
    [SerializeField] private bool triggerOnce = false;

    [Header("Unity Event ที่ต้องการเรียกใช้งาน")]
    [SerializeField] private UnityEvent onTriggerEntered;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && hasTriggered) return;

        // ตรวจสอบ Tag ถ้ามีการระบุไว้
        if (string.IsNullOrEmpty(targetTag) || other.CompareTag(targetTag))
        {
            // เรียกใช้ Unity Event ทั้งหมดที่ผูกไว้
            onTriggerEntered?.Invoke();

            if (triggerOnce)
            {
                hasTriggered = true;
            }
        }
    }
}