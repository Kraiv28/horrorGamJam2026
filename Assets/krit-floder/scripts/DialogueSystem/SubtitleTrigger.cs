using UnityEngine;

public class SubtitleTrigger : MonoBehaviour
{
    [Header("Subtitle Content")]
    [TextArea(2, 5)]
    [Tooltip("ข้อความที่จะให้แสดงบนจอเมื่อผู้เล่นเดินชน")]
    public string subtitleMessage = "นี่คือข้อความทดสอบระบบไดอาลอคสไตล์ P.T.";
    [Tooltip("ระยะเวลาที่จะค้างข้อความไว้บนจอก่อนจะเลื่อนเก็บ (วินาที)")]
    public float displayDuration = 3.0f;

    [Header("Trigger Settings")]
    [Tooltip("ให้ทำงานแค่ครั้งเดียวแล้วปิดตัวเองหรือไม่?")]
    public bool triggerOnce = true;

    private bool hasTriggered = false; // แก้ไขชื่อตัวแปรให้ถูกต้อง

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && triggerOnce) return;

        // เช็กว่าเป็นผู้เล่นเดินมาชนหรือไม่ (ใช้ Tag 'Player')
        if (other.CompareTag("Player"))
        {
            if (SubtitleUIController.Instance != null)
            {
                SubtitleUIController.Instance.ShowSubtitle(subtitleMessage, displayDuration);
                hasTriggered = true;

                // ถ้าตั้งค่าให้ทำงานครั้งเดียว สามารถปิด Collider หรือทำลาย GameObject นี้ทิ้งได้หลังเล่นจบ
                if (triggerOnce)
                {
                    GetComponent<Collider>().enabled = false;
                }
            }
            else
            {
                Debug.LogWarning("[SubtitleTrigger] ไม่พบ SubtitleUIController ใน Scene!");
            }
        }
    }
}