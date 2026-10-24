using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events; // เพิ่ม namespace สำหรับใช้งาน UnityEvent

public class PTLoopManager : MonoBehaviour
{
    public static PTLoopManager Instance { get; private set; }

    [Header("Player & Teleport Settings")]
    public Transform playerTransform;
    public Transform startPoint; // จุดเริ่มต้นของทางเดิน
    public Transform endPoint;   // จุดสิ้นสุดทางเดิน (จุดสั่ง Teleport)

    [Header("Playlist Progression")]
    [Tooltip("ดรรชนีลูปปัจจุบัน (เริ่มต้นที่ 0 เหมือนคลิปแรกใน Playlist)")]
    public int currentLoopIndex = 0;

    [Tooltip("เมื่อเล่นจบฉากสุดท้ายใน Playlist จะให้วนกลับไปฉากแรกหรือไม่?")]
    public bool loopPlaylistWhenFinished = true;

    [Header("Notification Settings")]
    public bool showDebugLog = true;

    // โครงสร้างข้อมูลสำหรับแต่ละฉาก/ลูป โดยเปลี่ยนมาใช้ UnityEvent
    [System.Serializable]
    public class LoopStage
    {
        [Tooltip("ชื่อของลูปเพื่อความง่ายในการจัดลำดับ เช่น 'Loop 01 - Dark Corridor'")]
        public string stageName = "New Loop Stage";

        [TextArea(2, 4)]
        [Tooltip("ข้อความแจ้งเตือนเมื่อผู้เล่นก้าวเข้าสู่ลูปนี้")]
        public string customNotification = "Entering next loop stage...";

        [Header("Loop Events (UnityEvent)")]
        [Tooltip("อีเวนต์ที่จะทำงานทันทีเมื่อผู้เล่นเข้าสู่ลูปนี้ (สามารถลากฟังก์ชันมาใส่ได้อิสระ)")]
        public UnityEvent onLoopEnter;
    }

    [Header("Playlist Sequences")]
    [Tooltip("ใส่ลำดับลูปเรียงจากบนลงล่าง (เหมือนรายการวิดีโอใน Playlist)")]
    public List<LoopStage> playlist;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // เริ่มต้นเกม ให้เรียกอีเวนต์ของลูปแรก (Index 0) ทันที
        if (playlist.Count > 0)
        {
            ExecuteStageEvents(playlist[currentLoopIndex]);
        }
    }

    // ฟังก์ชันนี้ถูกเรียกเมื่อผู้เล่นเดินชน Trigger
    public void OnPlayerReachEnd()
    {
        // 1. คำนวณตำแหน่งเพื่อ Teleport ให้สมูท
        Vector3 offset = playerTransform.position - endPoint.position;

        CharacterController cc = playerTransform.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        playerTransform.position = startPoint.position + offset;

        if (cc != null) cc.enabled = true;

        // 2. ขยับไปยังลูปถัดไปใน Playlist
        AdvanceToNextLoop();
    }

    private void AdvanceToNextLoop()
    {
        if (playlist.Count == 0) return;

        // เพิ่ม Index ไปยังฉากถัดไป
        currentLoopIndex++;

        // เช็คว่าเล่นครบทุกฉากใน Playlist แล้วหรือยัง
        if (currentLoopIndex >= playlist.Count)
        {
            if (loopPlaylistWhenFinished)
            {
                currentLoopIndex = 0; // วนกลับไปวิดีโอแรกใน Playlist
            }
            else
            {
                currentLoopIndex = playlist.Count - 1; // ค้างไว้ที่ฉากสุดท้าย
            }
        }

        // 3. ดึงข้อมูลฉากปัจจุบันและรัน UnityEvent
        LoopStage currentStage = playlist[currentLoopIndex];
        ExecuteStageEvents(currentStage);

        // 4. ส่งแจ้งเตือน
        SendNotification(currentStage);
    }

    private void ExecuteStageEvents(LoopStage stage)
    {
        // สั่งทำงาน UnityEvent ทั้งหมดที่ผูกไว้ในลูปนี้
        if (stage.onLoopEnter != null)
        {
            stage.onLoopEnter.Invoke();
        }
    }

    private void SendNotification(LoopStage stage)
    {
        string logMsg = $"▶ [Playlist Loop System] กำลังเล่น: [{currentLoopIndex}] - {stage.stageName}\n" +
                         $"📢 Detail: {stage.customNotification}";

        if (showDebugLog)
        {
            Debug.Log(logMsg);
        }
    }
}