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

    [Header("Audio Settings")]
    [Tooltip("AudioSource สำหรับเล่นเสียงพิมพ์ข้อความ (ถ้าไม่ใส่ จะสร้างให้อัตโนมัติ)")]
    public AudioSource audioSource;
    [Tooltip("เสียงที่จะเล่นทุกครั้งที่ตัวอักษรถูกพิมพ์ออกมา")]
    public AudioClip typingSound;
    [Range(0f, 1f)]
    public float typingVolume = 0.5f;

    private Vector2 targetPosition;
    private Coroutine activeSubtitleRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // ตรวจสอบหรือสร้าง AudioSource เผื่อไม่ได้กำหนดไว้ใน Inspector
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

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
            subtitleText.color = textColor;
        }

        targetPosition = new Vector2(panelRect.anchoredPosition.x, shownYPos);
        yield return new WaitForSeconds(0.3f);

        foreach (char letter in message.ToCharArray())
        {
            if (subtitleText != null) subtitleText.text += letter;

            // เล่นเสียงพิมพ์เฉพาะเมื่อไม่ใช่ตัวอักษรช่องว่าง (Space) จะได้ดูเป็นธรรมชาติขึ้น
            if (typingSound != null && audioSource != null && !char.IsWhiteSpace(letter))
            {
                audioSource.PlayOneShot(typingSound, typingVolume);
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        yield return new WaitForSeconds(displayDuration);

        targetPosition = new Vector2(panelRect.anchoredPosition.x, hiddenYPos);
        yield return new WaitForSeconds(0.5f);
        if (subtitleText != null) subtitleText.text = "";
    }
}