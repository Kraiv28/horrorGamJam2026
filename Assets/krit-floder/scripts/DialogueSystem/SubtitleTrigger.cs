using UnityEngine;

public class SubtitleTrigger : MonoBehaviour
{
    [Header("Subtitle Content")]
    [TextArea(2, 5)]
    [Tooltip("ข้อความที่จะให้แสดงบนจอเมื่อผู้เล่นเดินชน")]
    public string subtitleMessage = "นี่คือข้อความทดสอบระบบไดอาลอคสไตล์ P.T.";
    [Tooltip("ระยะเวลาที่จะค้างข้อความไว้บนจอก่อนจะเลื่อนเก็บ (วินาที)")]
    public float displayDuration = 3.0f;
    [Tooltip("กำหนดสีของข้อความสำหรับ Trigger นี้")]
    public Color subtitleColor = Color.white; // ค่าเริ่มต้นเป็นสีขาว

    [Header("Trigger Settings")]
    [Tooltip("ให้ทำงานแค่ครั้งเดียวแล้วปิดตัวเองหรือไม่?")]
    public bool triggerOnce = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && triggerOnce) return;

        if (other.CompareTag("Player"))
        {
            if (SubtitleUIController.Instance != null)
            {
                // ส่งค่าสี (subtitleColor) ไปด้วย
                SubtitleUIController.Instance.ShowSubtitle(subtitleMessage, displayDuration, subtitleColor);
                hasTriggered = true;

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