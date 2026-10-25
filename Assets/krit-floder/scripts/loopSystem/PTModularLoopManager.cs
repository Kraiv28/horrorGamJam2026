using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PTModularLoopManager : MonoBehaviour
{
    public static PTModularLoopManager Instance { get; private set; }

    [Header("Staircase Prefab & Settings")]
    [Tooltip("Prefab ของบันไดปกติ (ค่าเริ่มต้น)")]
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

    // เพิ่มตัวแปรเช็กสถานะการล็อกที่ด้านบนของคลาส PTModularLoopManager
    private bool lockPlaylistOverride = false;
    [Header("Prefab ถาวรหลังทำเงื่อนไขสำเร็จ")]
    public GameObject globalWinPrefab; // หรือเก็บค่าจากที่ส่งมา

    [System.Serializable]
    public class LoopStage
    {
        public string stageName = "Staircase Loop Stage";

        [Header("Custom Prefab for this Loop")]
        [Tooltip("ถ้าเว้นว่างไว้ จะใช้ Stair Prefab หลัก แต่ถ้าใส่ Prefab ตรงนี้ ลูปนี้จะใช้ Prefab นี้แทนทันที")]
        public GameObject customStairPrefab;

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

        for (int i = 0; i < maxActiveStairs; i++)
        {
            SpawnNewStair();
        }
    }

    public void OnPlayerReachTrigger()
    {
        AdvanceToNextLoop();
        SpawnNewStair();

        if (activeStairs.Count > maxActiveStairs)
        {
            GameObject oldStair = activeStairs.Dequeue();
            if (oldStair != null)
            {
                Destroy(oldStair);
            }
        }
    }

    private void SpawnNewStair()
    {
        GameObject prefabToSpawn = stairPrefab;
        if (playlist.Count > 0 && currentLoopIndex < playlist.Count)
        {
            GameObject stageCustomPrefab = playlist[currentLoopIndex].customStairPrefab;
            if (stageCustomPrefab != null)
            {
                prefabToSpawn = stageCustomPrefab;
            }
        }

        if (prefabToSpawn == null) return;

        GameObject newStair = Instantiate(prefabToSpawn, nextSpawnPoint.position, nextSpawnPoint.rotation);
        activeStairs.Enqueue(newStair);

        Transform connector = newStair.transform.Find("NextPoint");
        if (connector != null)
        {
            nextSpawnPoint = connector;
        }
        else
        {
            Debug.LogWarning($"[PT System] ไม่พบ GameObject ลูกชื่อ 'NextPoint' ที่ปลาย Prefab ของลูป [{currentLoopIndex}] กรุณาสร้างไว้ด้วยครับ!");
        }
    }

    private void AdvanceToNextLoop()
    {
        // ถ้าถูกล็อกด้วยระบบ Win/End แล้ว จะไม่ให้ขยับไปลูปอื่นใน Playlist อีก
        if (lockPlaylistOverride) return;

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
            Debug.Log($"🧗 [Staircase Loop] เข้าสู่ลูป: [{currentLoopIndex}] - {currentStage.stageName}\n📢 {currentStage.customNotification}");
        }
    }

    // ==========================================
    // ฟังก์ชันสำหรับเรียกผ่าน UnityEvent (เช่น จาก InstantEventItem)
    // เพื่อเปลี่ยน Prefab ของลูปปัจจุบันทันที
    // ==========================================
    // ฟังก์ชันสำหรับล็อกและเปลี่ยน Prefab ถาวร
    public void ForceOverridePrefab(GameObject newPrefabToUse)
    {
        if (newPrefabToUse != null)
        {
            // 1. สั่งล็อกไม่ให้ Playlist ทำงานต่อ
            lockPlaylistOverride = true;

            // 2. บังคับเปลี่ยน Custom Prefab ของลูปปัจจุบันและลูปต่อๆ ไปให้ใช้ Win/End Prefab นี้ทั้งหมด
            if (playlist.Count > 0 && currentLoopIndex < playlist.Count)
            {
                playlist[currentLoopIndex].customStairPrefab = newPrefabToUse;
            }

            // เพิ่มเติม: กำหนดให้ stairPrefab หลักกลายเป็น Prefab นี้ด้วย เพื่อให้เวลาสร้างชิ้นใหม่ต่อๆ ไปใช้ตัวนี้ยาวๆ
            stairPrefab = newPrefabToUse;

            if (showDebugLog)
            {
                Debug.Log($"🎉 [PT System] ล็อกระบบ Playlist สำเร็จ! ตั้งแต่ลูปนี้เป็นต้นไปจะสร้าง Prefab '{newPrefabToUse.name}' เท่านั้น");
            }
        }
    }
}