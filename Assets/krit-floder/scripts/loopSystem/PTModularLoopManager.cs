using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PTModularLoopManager : MonoBehaviour
{
    public static PTModularLoopManager Instance { get; private set; }

    [Header("Staircase Prefab & Settings")]
    [Tooltip("Prefab ของบันได (ต้องมีจุดเชื่อมต่อ NextPoint ไว้ที่ปลายสุดด้านบนของบันได)")]
    public GameObject stairPrefab;

    [Tooltip("ตำแหน่งเริ่มต้นของบันไดชิ้นแรกสุดใน Scene")]
    public Transform initialSpawnPoint;

    [Tooltip("จำนวนชุดบันไดที่ถูกสร้างพร้อมกันในฉาก")]
    public int maxActiveStairs = 3;

    [Header("Playlist Progression")]
    public int currentLoopIndex = 0;
    public bool loopPlaylistWhenFinished = true;

    [Header("Notification Settings")]
    public bool showDebugLog = true;

    [System.Serializable]
    public class LoopStage
    {
        public string stageName = "Staircase Loop Stage";
        [TextArea(2, 4)]
        public string customNotification = "Walking up higher...";

        [Header("Loop Events (UnityEvent)")]
        public UnityEvent onLoopEnter;
    }

    public List<LoopStage> playlist;

    private Queue<GameObject> activeStairs = new Queue<GameObject>();
    private Transform nextSpawnPoint;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (initialSpawnPoint != null)
        {
            nextSpawnPoint = initialSpawnPoint;
        }
        else
        {
            GameObject tempPoint = new GameObject("DefaultSpawnPoint");
            tempPoint.transform.position = transform.position;
            nextSpawnPoint = tempPoint.transform;
        }

        // สร้างบันไดเริ่มต้นล่วงหน้าตามจำนวนที่กำหนด
        for (int i = 0; i < maxActiveStairs; i++)
        {
            SpawnNewStair();
        }
    }

    // ฟังก์ชันนี้ถูกเรียกเมื่อผู้เล่นเดินขึ้นไปถึง Trigger ปลายบันได
    public void OnPlayerReachTrigger()
    {
        // 1. สร้างบันไดชุดใหม่ต่อขึ้นไปข้างบน
        SpawnNewStair();

        // 2. ลบบันไดชุดเก่าที่สุดทิ้ง (เพื่อเคลียร์แรมและรักษาความยาว)
        if (activeStairs.Count > maxActiveStairs)
        {
            GameObject oldStair = activeStairs.Dequeue();
            if (oldStair != null)
            {
                Destroy(oldStair);
            }
        }

        // 3. ขยับ Playlist และรัน Event ประจำลูป
        AdvanceToNextLoop();
    }

    private void SpawnNewStair()
    {
        if (stairPrefab == null) return;

        // สร้างบันไดใหม่ตามตำแหน่งและองศาของ NextPoint ชิ้นก่อนหน้า
        GameObject newStair = Instantiate(stairPrefab, nextSpawnPoint.position, nextSpawnPoint.rotation);

        activeStairs.Enqueue(newStair);

        // ค้นหาจุดเชื่อมต่อ (NextPoint) ที่ปลายทางออกด้านบนของบันได Prefab
        Transform connector = newStair.transform.Find("NextPoint");
        if (connector != null)
        {
            nextSpawnPoint = connector; // จุดเกิดถัดไปจะถูกพาทยายขึ้นไปตามความสูงและทิศทางของบันได
        }
        else
        {
            Debug.LogWarning("ไม่พบ GameObject ลูกชื่อ 'NextPoint' ที่ปลายบันได กรุณาสร้างไว้ที่จุดสิ้นสุดของ Prefab บันไดด้วยครับ!");
        }
    }

    private void AdvanceToNextLoop()
    {
        if (playlist.Count == 0) return;

        currentLoopIndex++;

        if (currentLoopIndex >= playlist.Count)
        {
            if (loopPlaylistWhenFinished)
            {
                currentLoopIndex = 0;
            }
            else
            {
                currentLoopIndex = playlist.Count - 1;
            }
        }

        LoopStage currentStage = playlist[currentLoopIndex];

        if (currentStage.onLoopEnter != null)
        {
            currentStage.onLoopEnter.Invoke();
        }

        if (showDebugLog)
        {
            Debug.Log($"🧗 [Staircase Loop] สร้างบันไดใหม่ขึ้นไปชั้นบน | กำลังเล่นลูป: [{currentLoopIndex}] - {currentStage.stageName}\n📢 {currentStage.customNotification}");
        }
    }
}