using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class TransitionDoor : MonoBehaviour
{
    [Header("Door Role Settings")]
    public DoorType doorType = DoorType.EnteringBathroom;

    public enum DoorType
    {
        EnteringBathroom,
        ExitingBathroom
    }

    [Header("Condition Settings")]
    public bool isUnlocked = false;
    public string lockedMessage = "ประตูห้องน้ำถูกล็อก... ต้องทำเงื่อนไขบางอย่างก่อน";

    [Header("Teleport Settings")]
    public Transform teleportTargetPoint;
    [Tooltip("ระยะเวลาที่ผู้เล่นจะหยุดนิ่ง / รออนิเมชั่นภาพดำระหว่างวาร์ป (วินาที)")]
    public float transitionDelay = 0.5f;

    [Header("Audio Settings")]
    [Tooltip("AudioSource สำหรับเล่นเสียงที่ประตู")]
    public AudioSource audioSource;
    [Tooltip("เสียงเมื่อพยายามเปิดประตู แต่ยังไม่ปลดล็อก")]
    public AudioClip lockedSound;
    [Tooltip("เสียงตอนเปิดประตู (เล่นเมื่อเงื่อนไขผ่านและเริ่มคลิกเปิด)")]
    public AudioClip openSound;
    [Tooltip("เสียงตอนปิดประตู (เล่นหลังจากวาร์ปมาถึงฝั่งปลายทางครบเวลาแล้ว)")]
    public AudioClip closeSound;

    [Header("Events")]
    public UnityEvent onDoorOpenedEvent;

    private bool hasTriggered = false;
    private bool isLocallyTeleporting = false;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void InteractWithDoor()
    {
        if (isLocallyTeleporting) return;

        // ถ้าเป็นประตูเข้า แต่ยังไม่ปลดล็อก -> เล่นเสียงล็อก
        if (doorType == DoorType.EnteringBathroom)
        {
            if (!isUnlocked)
            {
                Debug.Log("<color=red>Locked:</color> " + lockedMessage);
                PlaySound(lockedSound);
                return;
            }
        }

        if (!hasTriggered)
        {
            hasTriggered = true;

            if (onDoorOpenedEvent != null)
            {
                onDoorOpenedEvent.Invoke();
            }
        }

        // เล่นเสียง "เปิดประตู" ทันทีที่กดเปิดสำเร็จ
        PlaySound(openSound);

        if (teleportTargetPoint != null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                StartCoroutine(SafeTeleportRoutine(player));
            }
            else
            {
                Debug.LogWarning("[TransitionDoor] ไม่พบ GameObject ที่มี Tag 'Player' ใน Scene!");
            }
        }
        else
        {
            Debug.LogWarning("[TransitionDoor] ยังไม่ได้กำหนดจุด Teleport Target Point!");
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private IEnumerator SafeTeleportRoutine(GameObject player)
    {
        isLocallyTeleporting = true;

        // 1. ปิดการควบคุมและการหันเมาส์
        CharacterController charController = player.GetComponent<CharacterController>();
        MonoBehaviour playerMovement = player.GetComponent<MonoBehaviour>();
        mouseLook playerMouseLook = player.GetComponentInChildren<mouseLook>();

        if (charController != null) charController.enabled = false;
        if (playerMovement != null) playerMovement.enabled = false;
        if (playerMouseLook != null) playerMouseLook.canLook = false;

        // 2. หน่วงเวลารอระหว่างเข้าประตู (ช่วงจอดำ / พักนิ่ง)
        yield return new WaitForSeconds(transitionDelay);

        // 3. ย้ายตำแหน่งและมุมมองของผู้เล่นไปยังจุดหมายปลายทาง
        player.transform.position = teleportTargetPoint.position;
        player.transform.rotation = teleportTargetPoint.rotation;

        yield return null;

        // 4. เล่นเสียง "ปิดประตู" ทันทีที่วาร์ปมาถึงฝั่งปลายทางครบเวลา
        PlaySound(closeSound);

        // 5. เปิดการควบคุมและการหันเมาส์กลับมาปกติ
        if (charController != null) charController.enabled = true;
        if (playerMovement != null) playerMovement.enabled = true;
        if (playerMouseLook != null) playerMouseLook.canLook = true;

        isLocallyTeleporting = false;

        Debug.Log("[TransitionDoor] วาร์ปและเล่นเสียงเปิด-ปิดประตูสำเร็จ!");
    }

    public void UnlockDoor()
    {
        isUnlocked = true;
        Debug.Log("[TransitionDoor] ประตูห้องน้ำถูกปลดล็อกแล้ว!");
    }
}