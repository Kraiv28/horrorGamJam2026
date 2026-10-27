using UnityEngine;
using UnityEngine.Events;
using DoorScript;

[RequireComponent(typeof(Door))]
public class DoorInteractable : MonoBehaviour
{
    private Door doorScript;

    [Header("Requirement Settings")]
    public int requiredItemId = -1;
    public string messageOnMissing = "You need a key to open this door.";
    public bool consumeItemOnSuccess = true;

    [Header("Lock Settings")]
    [Tooltip("ถ้าเป็น true จะไม่สามารถเปิดประตูนี้ได้ (ถูกล็อก)")]
    public bool isLocked = false; // ตัวแปรสำหรับล็อกประตู

    [Header("Event Settings")]
    public UnityEvent onSuccessEvent;

    void Start()
    {
        doorScript = GetComponent<Door>();
    }

    public void Interact(int heldItemId)
    {
        // [เพิ่มเข้ามา] เช็กว่าถ้าประตูถูกล็อก ให้หยุดการทำงานทันที
        if (isLocked)
        {
            Debug.Log("[DoorInteractable] ประตูนี้ถูกล็อกและปิดตายแล้ว!");
            return;
        }

        if (requiredItemId < 0)
        {
            ExecuteOpenDoor();
            return;
        }

        if (heldItemId == requiredItemId)
        {
            if (consumeItemOnSuccess)
            {
                playerInteraction player = FindFirstObjectByType<playerInteraction>();
                if (player != null)
                {
                    player.ConsumeHeldItem();
                }
            }

            if (onSuccessEvent != null)
            {
                onSuccessEvent.Invoke();
            }

            ExecuteOpenDoor();
        }
        else
        {
            Debug.Log("<color=red>Command Console:</color> " + messageOnMissing);
        }
    }

    private void ExecuteOpenDoor()
    {
        if (doorScript != null)
        {
            doorScript.OpenDoor();
        }
    }
}