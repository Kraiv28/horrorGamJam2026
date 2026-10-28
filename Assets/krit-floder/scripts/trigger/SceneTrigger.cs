using UnityEngine;

public class SceneTrigger : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("ชื่อ Scene ปลายทางที่ต้องการเปลี่ยนไป (พิมพ์ชื่อให้ตรงเป๊ะ)")]
    public string targetSceneName;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // เช็กว่าเป็นผู้เล่น (Player) และยังไม่เคยทำงาน
        if (!triggered && other.CompareTag("Player"))
        {
            // ค้นหา SceneLoader ที่อยู่ใน Scene เองอัตโนมัติ
            SceneLoader loader = FindFirstObjectByType<SceneLoader>();

            if (loader != null)
            {
                triggered = true; // ป้องกันการชนซ้ำหลายรอบ
                loader.FadeAndSwitchScene(targetSceneName);
            }
            else
            {
                Debug.LogError("[SceneTrigger] ไม่พบ SceneLoader ใน Scene! กรุณาสร้าง SceneLoader ไว้ใน Scene ด้วยครับ");
            }
        }
    }
}