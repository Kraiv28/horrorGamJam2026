using UnityEngine;
using UnityEngine.Events;

public class InstantEventItem : MonoBehaviour
{
    [Header("Item Info")]
    public string itemName = "Special Event Item";

    [Header("Audio Settings")]
    public AudioClip pickupSound;
    public AudioSource audioSource;

    [Header("Event Settings")]
    [Tooltip("เลือกฟังก์ชันสำเร็จรูปด้านล่าง หรือเพิ่ม Event อื่นๆ ที่ไม่ต้องอ้างอิง Scene เก่า")]
    public UnityEvent onPickedUpEvent;

    [Header("Auto-Link Manager (Prefab Friendly)")]
    [Tooltip("ติ๊กถูกถ้าต้องการให้ไอเทมนี้ไปสั่งเปลี่ยน Prefab ของ PTModularLoopManager ใน Scene ใหม่ให้อัตโนมัติ")]
    public bool autoTriggerLoopManager = true;
    [Tooltip("ลาก WinPrefab หรือ Prefab ปลายทางที่ต้องการเปลี่ยนมาใส่ตรงนี้")]
    public GameObject targetOverridePrefab;

    [Header("Post-Pickup Action")]
    public bool destroyOnPickup = true;

    public void PickUpInstant()
    {
        // 1. เล่นเสียง
        PlayPickupSound();

        // 2. ถ้าเปิดใช้งานระบบ Auto-Link จะทำการค้นหา Manager ใน Scene ใหม่และสั่งเปลี่ยน Prefab ทันที
        if (autoTriggerLoopManager)
        {
            PTModularLoopManager loopManager = FindFirstObjectByType<PTModularLoopManager>();
            if (loopManager != null && targetOverridePrefab != null)
            {
                loopManager.ForceOverridePrefab(targetOverridePrefab);
                Debug.Log($"[InstantEventItem] ค้นพบ PTModularLoopManager ใน Scene ใหม่ และเปลี่ยน Prefab เป็น {targetOverridePrefab.name} สำเร็จ!");
            }
            else
            {
                Debug.LogWarning("[InstantEventItem] ไม่พบ PTModularLoopManager หรือยังไม่ได้กำหนด targetOverridePrefab ใน Prefab!");
            }
        }

        // 3. รัน Unity Event อื่นๆ เพิ่มเติม (ถ้ามี)
        if (onPickedUpEvent != null)
        {
            onPickedUpEvent.Invoke();
        }

        // 4. ลบไอเทมทิ้งหลังเก็บ
        if (destroyOnPickup)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void PlayPickupSound()
    {
        if (pickupSound == null) return;

        if (audioSource != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
        else
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
    }
}