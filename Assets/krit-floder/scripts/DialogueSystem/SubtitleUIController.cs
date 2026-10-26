using UnityEngine;
using TMPro;
using System.Collections;

public class SubtitleUIController : MonoBehaviour
{
    public static SubtitleUIController Instance;

    [Header("UI References")]
    [Tooltip("Panel หรือ Background ของกล่องข้อความ Dialogue")]
    public RectTransform panelRect;
    [Tooltip("TextMeshPro สำหรับแสดงข้อความ")]
    public TextMeshProUGUI subtitleText;

    [Header("Animation Settings")]
    [Tooltip("ตำแหน่ง Y ตอนซ่อนกล่องข้อความ (อยู่ด้านล่างจอหรือนอกจอ)")]
    public float hiddenYPos = -150f;
    [Tooltip("ตำแหน่ง Y ตอนแสดงกล่องข้อความขึ้นมา")]
    public float shownYPos = 50f;
    [Tooltip("ความเร็วในการสไลด์กล่องข้อความขึ้น/ลง")]
    public float slideSpeed = 10f;

    [Header("Typing Settings")]
    [Tooltip("ความเร็วในการพิมพ์ข้อความทีละตัวอักษร (วินาทีต่อตัว)")]
    public float typingSpeed = 0.05f;

    private Vector2 targetPosition;
    private Coroutine activeSubtitleRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (panelRect != null)
        {
            // ตั้งค่าเริ่มต้นให้ซ่อนกล่องข้อความลงไปด้านล่าง
            Vector2 pos = panelRect.anchoredPosition;
            pos.y = hiddenYPos;
            panelRect.anchoredPosition = pos;
            targetPosition = pos;
        }

        if (subtitleText != null) subtitleText.text = "";
    }

    private void Update()
    {
        // ทำอนิเมชั่นเลื่อนกล่องข้อความแบบ Smooth (Lerp)
        if (panelRect != null)
        {
            Vector2 currentPos = panelRect.anchoredPosition;
            panelRect.anchoredPosition = Vector2.Lerp(currentPos, targetPosition, Time.deltaTime * slideSpeed);
        }
    }

    /// <summary>
    /// ฟังก์ชันสั่งแสดงซับไตเติลตามสไตล์ที่ต้องการ
    /// </summary>
    public void ShowSubtitle(string message, float displayDuration)
    {
        if (activeSubtitleRoutine != null)
        {
            StopCoroutine(activeSubtitleRoutine);
        }
        activeSubtitleRoutine = StartCoroutine(PlaySubtitleRoutine(message, displayDuration));
    }

    private IEnumerator PlaySubtitleRoutine(string message, float displayDuration)
    {
        // 1. เคลียร์ข้อความเก่า และสั่งสไลด์กล่องข้อความขึ้นมา
        if (subtitleText != null) subtitleText.text = "";
        targetPosition = new Vector2(panelRect.anchoredPosition.x, shownYPos);

        // รอให้กล่องสไลด์ขึ้นมาสักครู่ (0.3 วินาที)
        yield return new WaitForSeconds(0.3f);

        // 2. เริ่มพิมพ์ข้อความทีละตัวอักษร (Typewriter Effect)
        foreach (char letter in message.ToCharArray())
        {
            if (subtitleText != null) subtitleText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        // 3. พิมพ์ครบแล้ว รอค้างไว้ตามเวลาที่กำหนด (Display Duration)
        yield return new WaitForSeconds(displayDuration);

        // 4. สั่งสไลด์กล่องข้อความเก็บลงด้านล่าง
        targetPosition = new Vector2(panelRect.anchoredPosition.x, hiddenYPos);

        // รอจนกว่ากล่องจะลงไปเก็บมิด แล้วเคลียร์ข้อความทิ้ง
        yield return new WaitForSeconds(0.5f);
        if (subtitleText != null) subtitleText.text = "";
    }
}