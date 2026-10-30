using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoaderG : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image fadeImage; // ลาก Image สีดำมาใส่ตรงนี้

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 1.5f; // ระยะเวลาในการเฟด (วินาที)
    [SerializeField] private string gameSceneName = "GameScene"; // ชื่อ Scene ของเกม

    [Header("Unity Events")]
    [Tooltip("อีเวนต์ที่จะทำงานทันทีเมื่อกดปุ่ม (ก่อนเริ่มเฟด)")]
    public UnityEvent onStartFade;

    [Tooltip("อีเวนต์ที่จะทำงานทันทีเมื่อหน้าจอดำสนิท (ก่อนเปลี่ยน Scene)")]
    public UnityEvent onFadeComplete;

    private void Start()
    {
        // เริ่มเกมมาให้ภาพโปร่งใสและปิดการคลิกทะลุ (Raycast Target เป็น false)
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.raycastTarget = false;
        }
    }

    // ฟังก์ชันหลักที่ผูกกับปุ่ม Start
    public void TriggerFadeAndLoad()
    {
        StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        // เปิด Raycast Target เพื่อบล็อกการคลิกซ้ำระหว่างเฟด
        if (fadeImage != null)
        {
            fadeImage.raycastTarget = true;
        }

        // 1. เรียกใช้งาน Unity Event ทันทีที่กดปุ่ม
        onStartFade?.Invoke();

        float elapsedTime = 0f;
        Color startColor = fadeImage != null ? fadeImage.color : Color.black;

        // 2. ค่อยๆ ปรับ Alpha จาก 0 เป็น 1 (ภาพค่อยๆ ดำลง)
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);

            if (fadeImage != null)
            {
                Color c = startColor;
                c.a = alpha;
                fadeImage.color = c;
            }
            yield return null;
        }

        // 3. เมื่อภาพดำสนิทแล้ว เรียกใช้งาน Unity Event ที่ตั้งไว้ตอนท้าย
        onFadeComplete?.Invoke();

        // 4. เปลี่ยน Scene เข้าเกม
        SceneManager.LoadScene(gameSceneName);
    }
}