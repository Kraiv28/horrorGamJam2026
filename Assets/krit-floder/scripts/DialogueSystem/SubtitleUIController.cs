using UnityEngine;
using TMPro;
using System.Collections;

public class SubtitleUIController : MonoBehaviour
{
    public static SubtitleUIController Instance;

    [Header("UI References")]
    public RectTransform panelRect;
    public TextMeshProUGUI subtitleText;

    [Header("Animation Settings")]
    public float hiddenYPos = -150f;
    public float shownYPos = 50f;
    public float slideSpeed = 10f;

    [Header("Typing Settings")]
    public float typingSpeed = 0.05f;

    private Vector2 targetPosition;
    private Coroutine activeSubtitleRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (panelRect != null)
        {
            Vector2 pos = panelRect.anchoredPosition;
            pos.y = hiddenYPos;
            panelRect.anchoredPosition = pos;
            targetPosition = pos;
        }

        if (subtitleText != null) subtitleText.text = "";
    }

    private void Update()
    {
        if (panelRect != null)
        {
            Vector2 currentPos = panelRect.anchoredPosition;
            panelRect.anchoredPosition = Vector2.Lerp(currentPos, targetPosition, Time.deltaTime * slideSpeed);
        }
    }

    // เพิ่มพารามิเตอร์ textColor เข้ามา
    public void ShowSubtitle(string message, float displayDuration, Color textColor)
    {
        if (activeSubtitleRoutine != null)
        {
            StopCoroutine(activeSubtitleRoutine);
        }
        activeSubtitleRoutine = StartCoroutine(PlaySubtitleRoutine(message, displayDuration, textColor));
    }

    private IEnumerator PlaySubtitleRoutine(string message, float displayDuration, Color textColor)
    {
        if (subtitleText != null)
        {
            subtitleText.text = "";
            subtitleText.color = textColor; // กำหนดสีของ Text ตามที่ส่งมา
        }

        targetPosition = new Vector2(panelRect.anchoredPosition.x, shownYPos);
        yield return new WaitForSeconds(0.3f);

        foreach (char letter in message.ToCharArray())
        {
            if (subtitleText != null) subtitleText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        yield return new WaitForSeconds(displayDuration);

        targetPosition = new Vector2(panelRect.anchoredPosition.x, hiddenYPos);
        yield return new WaitForSeconds(0.5f);
        if (subtitleText != null) subtitleText.text = "";
    }
}