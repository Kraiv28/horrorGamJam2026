using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class conditionalAnimationInteractable : MonoBehaviour
{
    public enum ActivationMode
    {
        RequirementBased, // ตรวจสอบ Item ID ในมือ
        TriggerBased      // ทำงานทันทีเมื่อโดน Trigger (เช่น ฟังก์ชันภายนอกเรียกใช้ หรือเดินชน)
    }

    // [เพิ่มเข้ามา] ตัวเลือกสำหรับจัดการ Object หลังจบกระบวนการทั้งหมด
    public enum PostSequenceAction
    {
        KeepAlive,         // เก็บไว้เหมือนเดิม
        DestroyGameObject, // ลบ GameObject นี้ทิ้งทันที
        DisableComponent   // ปิดการทำงานของสคริปต์นี้ (ไม่สามารถ Interact ซ้ำได้)
    }

    [System.Serializable]
    public struct AnimationTargetData
    {
        [Tooltip("ตัวควบคุม Animator ที่ต้องการสั่งเล่น")]
        public Animator animator;
        [Tooltip("ชื่อกล่อง State ใน Animator ของชิ้นนี้ที่ต้องการให้เล่น")]
        public string animationStateName;

        [Tooltip("Event ที่จะทำงานทันที 'พร้อมกับ' เริ่มเล่นแอนิเมชั่นนี้")]
        public UnityEvent onAnimationStart;
    }

    // ตัวแปร Static กลางสำหรับใช้ล็อกการโยนของชั่วคราว
    public static bool IsAnyAnimationPlaying { get; private set; } = false;

    [Header("[0] Mode Setting")]
    [Tooltip("เลือกโหมดการทำงาน: Requirement = เช็คไอเทม, Trigger = ทำงานทันทีเมื่อถูกกระตุ้น")]
    public ActivationMode activationMode = ActivationMode.RequirementBased;

    // [เพิ่มเข้ามา] การตั้งค่าหลังจบงาน
    [Tooltip("จัดการกับวัตถุนี้อย่างไรหลังจากจบกระบวนการคัตซีนทั้งหมดแล้ว")]
    public PostSequenceAction postActionMode = PostSequenceAction.KeepAlive;

    [Header("[1] Requirement Settings (ใช้เฉพาะโหมด RequirementBased)")]
    public int requiredItemId = 2;
    public string messageOnMissing = "You need a specific item to activate this.";
    [Tooltip("ติ๊กถูกหากต้องการให้ลบไอเทมออกจากมือผู้เล่นทันทีเมื่อส่งไอเทมถูกต้อง")]
    public bool consumeItemOnSuccess = true;

    [Header("[2] Item Hiding Settings (ซ่อนและเปิดคืนหลังจบ)")]
    [Tooltip("ลาก Object ไอเทมประจำฉาก ที่ต้องการให้ซ่อนตอนเล่นคัตซีน และเปิดกลับมาเมื่อแอนิเมชั่นจบลง")]
    public GameObject itemToHideDuringAnimation;

    [Header("[3] Player Slowness Settings")]
    public bool slowPlayerDown = true;
    public float slowedSpeed = 1f;

    [Header("[4] Animation Target Settings")]
    [Tooltip("วัตถุหลักของแอนิเมชั่นคัตซีน ที่จะถูกซ่อนตอนเริ่มเกม -> เปิดตอนเล่น -> และซ่อนอีกครั้งตอนจบ")]
    public GameObject animationTargetObject;

    [Header("[5] Camera Settings")]
    public bool useCameraSwitch = true;
    public GameObject mainCamera;
    public GameObject animationCamera;

    [Header("[6] Multiple Animation Settings (เล่นพร้อมกันกี่ตัวก็ได้)")]
    [Tooltip("กดเพิ่มจำนวน (Size) เพื่อใส่ Animator, ชื่อแอนิเมชั่น และ Event ที่ต้องการให้ทำงานพร้อมกันได้")]
    public List<AnimationTargetData> animationTargets = new List<AnimationTargetData>();

    [Header("[7] Audio & Events Settings")]
    public AudioSource audioSource1;
    public AudioSource audioSource2;
    [Tooltip("Event รวมที่จะทำงานเมื่อ 'ทุกแอนิเมชั่น' ในระบบนี้เล่นจบลงทั้งหมดแล้ว")]
    public UnityEvent onSuccessEvent;

    private bool isPlaying = false;
    private playerMovementRB playerMovement;

    void Start()
    {
        ResetSystem();
    }

    /// <summary>
    /// ฟังก์ชันหลักสำหรับการ Interact (รองรับทั้งส่งไอเทมมาตรวจ หรือเรียกแบบ Trigger)
    /// </summary>
    public void Interact(int heldItemId, playerInteraction player = null)
    {
        if (isPlaying) return;

        // แยกคิดตามโหมดที่เลือกไว้ใน Inspector
        if (activationMode == ActivationMode.RequirementBased)
        {
            // โหมดเดิม: เช็คไอเทม
            if (heldItemId == requiredItemId)
            {
                ExecuteSequence(player);
            }
            else
            {
                Debug.Log($"<color=red>[Interact] Item ID ไม่ถูกต้อง!</color> {messageOnMissing}");
            }
        }
        else if (activationMode == ActivationMode.TriggerBased)
        {
            // โหมดใหม่: ทำงานทันที ไม่สนใจไอเทม
            ExecuteSequence(player);
        }
    }

    /// <summary>
    /// สำหรับใช้ภายนอกเรียกสั่งงานแบบ Trigger ตรงๆ โดยไม่ต้องส่ง Item ID (เช่น จาก Unity Event, Collider Trigger)
    /// </summary>
    public void OnTriggerActivate()
    {
        if (isPlaying) return;

        // ค้นหาผู้เล่นอัตโนมัติ (ถ้ามี) สำหรับใช้ดึงคอมโพเนนต์ลดความเร็ว
        playerInteraction player = FindFirstObjectByType<playerInteraction>();
        ExecuteSequence(player);
    }

    /// <summary>
    /// ฟังก์ชันรวบรวมคำสั่งเตรียมตัวก่อนเข้า Coroutine
    /// </summary>
    private void ExecuteSequence(playerInteraction player)
    {
        Debug.Log($"[Interact] เริ่มทำงานคัตซีนในโหมด: {activationMode}");

        // หักไอเทม (เฉพาะโหมดที่ต้องใช้ไอเทมและสั่งให้หัก)
        if (activationMode == ActivationMode.RequirementBased && consumeItemOnSuccess && player != null)
        {
            player.ConsumeHeldItem();
        }

        // ลดความเร็วผู้เล่น
        if (slowPlayerDown)
        {
            if (player != null) playerMovement = player.GetComponent<playerMovementRB>();
            if (playerMovement == null) playerMovement = FindFirstObjectByType<playerMovementRB>();

            if (playerMovement != null)
            {
                playerMovement.SetCutsceneSlow(true, slowedSpeed);
            }
        }

        StartCoroutine(PlaySequenceRoutine());
    }

    private IEnumerator PlaySequenceRoutine()
    {
        isPlaying = true;
        IsAnyAnimationPlaying = true;

        if (animationTargetObject != null) animationTargetObject.SetActive(true);
        if (itemToHideDuringAnimation != null) itemToHideDuringAnimation.SetActive(false);

        if (useCameraSwitch)
        {
            if (mainCamera != null) mainCamera.SetActive(false);
            if (animationCamera != null) animationCamera.SetActive(true);
        }

        foreach (var target in animationTargets)
        {
            if (target.animator != null)
            {
                target.animator.enabled = true;
                for (int i = 0; i < target.animator.layerCount; i++)
                {
                    if (target.animator.HasState(i, Animator.StringToHash(target.animationStateName)))
                    {
                        target.animator.Play(target.animationStateName, i, 0f);
                    }
                }
            }

            if (target.onAnimationStart != null)
            {
                target.onAnimationStart.Invoke();
            }
        }

        yield return null;

        int defaultLayer = LayerMask.NameToLayer("Default");
        foreach (var target in animationTargets)
        {
            if (target.animator != null)
            {
                SetLayerRecursively(target.animator.gameObject, defaultLayer);
            }
        }

        float longestAnimationLength = 0f;
        foreach (var target in animationTargets)
        {
            if (target.animator != null)
            {
                float currentLength = target.animator.GetCurrentAnimatorStateInfo(0).length;
                if (currentLength > longestAnimationLength)
                {
                    longestAnimationLength = currentLength;
                }
            }
        }

        if (longestAnimationLength <= 0f) longestAnimationLength = 2.0f;

        yield return new WaitForSeconds(longestAnimationLength);

        foreach (var target in animationTargets)
        {
            if (target.animator != null) target.animator.enabled = false;
        }

        if (useCameraSwitch)
        {
            if (mainCamera != null) mainCamera.SetActive(true);
            if (animationCamera != null) animationCamera.SetActive(false);
        }

        if (itemToHideDuringAnimation != null) itemToHideDuringAnimation.SetActive(true);
        if (animationTargetObject != null) animationTargetObject.SetActive(false);

        PlayRequiredSounds();

        if (onSuccessEvent != null)
        {
            onSuccessEvent.Invoke();
        }

        RestorePlayerSpeed();
        isPlaying = false;
        IsAnyAnimationPlaying = false;

        // [เพิ่มเข้ามา] ประมวลผลสิ่งที่ต้องการทำหลังจบกระบวนการทั้งหมด
        HandlePostSequenceAction();
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    private void RestorePlayerSpeed()
    {
        if (playerMovement != null)
        {
            playerMovement.SetCutsceneSlow(false);
            playerMovement = null;
        }
    }

    private void PlayRequiredSounds()
    {
        if (audioSource1 != null && audioSource1.clip != null) audioSource1.Play();
        if (audioSource2 != null && audioSource2.clip != null) audioSource2.Play();
    }

    // [เพิ่มเข้ามา] ฟังก์ชันสำหรับจัดการ Object หรือ Script หลังเล่นเสร็จ
    private void HandlePostSequenceAction()
    {
        switch (postActionMode)
        {
            case PostSequenceAction.DestroyGameObject:
                Debug.Log($"[PostAction] ทำลาย GameObject: {gameObject.name} เรียบร้อยแล้ว");
                Destroy(gameObject);
                break;

            case PostSequenceAction.DisableComponent:
                Debug.Log($"[PostAction] ปิดใช้งานสคริปต์บน: {gameObject.name} เรียบร้อยแล้ว (ไม่สามารถกดซ้ำได้)");
                this.enabled = false;
                break;

            case PostSequenceAction.KeepAlive:
            default:
                // ไม่ทำอะไรเพิ่มเติม ปล่อยให้ระบบอยู่ตามปกติ
                break;
        }
    }

    public void ResetSystem()
    {
        if (useCameraSwitch)
        {
            if (mainCamera != null) mainCamera.SetActive(true);
            if (animationCamera != null) animationCamera.SetActive(false);
        }

        if (animationTargetObject != null) animationTargetObject.SetActive(false);
        if (itemToHideDuringAnimation != null) itemToHideDuringAnimation.SetActive(true);

        foreach (var target in animationTargets)
        {
            if (target.animator != null) target.animator.enabled = false;
        }

        RestorePlayerSpeed();
        isPlaying = false;
        IsAnyAnimationPlaying = false;
    }
}