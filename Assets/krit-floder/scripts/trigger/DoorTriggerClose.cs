using UnityEngine;
using DoorScript;

public class DoorTriggerClose : MonoBehaviour
{
    [Header("Door Settings")]
    [Tooltip("ลาก GameObject ประตูที่มีสคริปต์ Door มาใส่ช่องนี้")]
    public Door targetDoor;

    [Header("Trigger Settings")]
    [Tooltip("ติ๊กถูกหากต้องการให้ Trigger นี้ทำงานเพียงครั้งเดียวเท่านั้น")]
    public bool triggerOnce = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (triggerOnce && hasTriggered) return;

            CloseAndLockDoor();
            hasTriggered = true;
            Debug.Log("[DoorTriggerClose] ประตูถูกปิดและล็อกถาวรแล้ว");
        }
    }

    private void CloseAndLockDoor()
    {
        if (targetDoor == null)
        {
            Debug.LogWarning("[DoorTriggerClose] ยังไม่ได้ใส่ Target Door ใน Inspector!");
            return;
        }

        // 1. สั่งปิดประตูทันที
        targetDoor.open = false;

        // 2. สั่งล็อกประตูผ่านตัวแปร isLocked
        if (targetDoor.TryGetComponent(out DoorInteractable doorInteractable))
        {
            doorInteractable.isLocked = true; // ล็อกไม่ให้เปิดได้อีก
        }
    }
}