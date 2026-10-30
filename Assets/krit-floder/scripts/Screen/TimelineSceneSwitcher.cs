using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement; // สำคัญสำหรับการเปลี่ยนฉาก

public class TimelineSceneSwitcher : MonoBehaviour
{
    [Header("Timeline Component")]
    [SerializeField] private PlayableDirector playableDirector;

    [Header("Scene Settings")]
    [SerializeField] private string targetSceneName; // ชื่อฉากที่ต้องการเปลี่ยนไป

    private void Awake()
    {
        // ถ้าไม่ได้ลากใส่ใน Inspector ให้หาก좌อัตโนมัติใน GameObject เดียวกัน
        if (playableDirector == null)
        {
            playableDirector = GetComponent<PlayableDirector>();
        }
    }

    private void OnEnable()
    {
        if (playableDirector != null)
        {
            // Subscribe เหตุการณ์ตอนที่ Timeline เล่นจบ
            playableDirector.stopped += OnTimelineStopped;
        }
    }

    private void OnDisable()
    {
        if (playableDirector != null)
        {
            // Unsubscribe เพื่อป้องกัน Memory Leak
            playableDirector.stopped -= OnTimelineStopped;
        }
    }

    private void OnTimelineStopped(PlayableDirector director)
    {
        // ตรวจสอบว่าเป็น Director ตัวที่เราต้องการไหม (เผื่อมีหลายอัน)
        if (director == playableDirector)
        {
            SwitchScene();
        }
    }

    public void SwitchScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            Debug.Log($"Timeline finished. Switching to scene: {targetSceneName}");
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogWarning("Target Scene Name is not assigned!");
        }
    }
}