using UnityEngine;
using UnityEngine.Events;

public class conditionalInteractable : MonoBehaviour
{
    [Header("Requirement")]
    public int requiredItemId = 2; // ID ของไอเทมที่ต้องใช้
    public string messageOnMissing = "You need a key to open this door.";

    [Tooltip("ติ๊กถูกหากต้องการให้ลบไอเทมออกจากมือผู้เล่นทันทีเมื่อใช้ไอเทมถูกต้อง")]
    public bool consumeItemOnSuccess = true;

    [Header("Animation Settings")]
    [Tooltip("ชื่อ State ของแอนิเมชั่นที่ต้องการให้เล่น (เช่น Open)")]
    public string animationStateName = "Open";

    [Header("Event Settings")]
    public UnityEvent onSuccessEvent; // Event เพิ่มเติม (ถ้ามี)

    // ฟังก์ชันที่จะถูกเรียกจาก Player
    public void Interact(int heldItemId)
    {
        if (heldItemId == requiredItemId)
        {
            Debug.Log("Condition Met! Executing action...");

            // 1. ลบไอเทมออกจากมือผู้เล่น (ถ้าเปิดใช้งาน)
            if (consumeItemOnSuccess)
            {
                playerInteraction player = FindFirstObjectByType<playerInteraction>();
                if (player != null)
                {
                    player.ConsumeHeldItem();
                }
            }

            // 2. รัน Unity Event เพิ่มเติม (ถ้ามี)
            if (onSuccessEvent != null)
            {
                onSuccessEvent.Invoke();
            }

            // 3. ตรวจสอบ Animator และ Animation ของตัวเอง
            Animator anim = GetComponent<Animator>();
            if (anim != null && HasState(anim, animationStateName))
            {
                // ถ้ามี Animator และมีชื่อ State แอนิเมชั่นนี้อยู่ -> สั่งเล่นแอนิเมชั่น
                anim.Play(animationStateName, 0, 0f);
                Debug.Log("[ConditionalInteractable] กำลังเล่นแอนิเมชั่นของตัวเอง");
            }
            else
            {
                // ถ้าไม่มี Animator หรือไม่มีแอนิเมชั่น -> ซ่อนตัวเองทันที
                Debug.Log("[ConditionalInteractable] ไม่พบ Animator หรือ Animation State ทำการซ่อน Object นี้");
                gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.Log("<color=red>Command Console:</color> " + messageOnMissing);
        }
    }

    // ฟังก์ชันช่วยตรวจสอบว่า Animator มี State นี้อยู่จริงหรือไม่
    private bool HasState(Animator animator, string stateName)
    {
        if (animator.runtimeAnimatorController == null) return false;
        return animator.HasState(0, Animator.StringToHash(stateName));
    }

    public void SetToDisappear()
    {
        gameObject.SetActive(false);
    }
}