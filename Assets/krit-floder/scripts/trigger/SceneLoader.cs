using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    [Header("UI Reference")]
    [Tooltip("ถ้าเว้นว่างไว้ ระบบจะค้นหา Image Panel สีดำให้อัตโนมัติ")]
    public Image fadePanel;

    [Header("Settings")]
    [Tooltip("ระยะเวลาในการ Fade ภาพให้ดำสนิท (วินาที)")]
    public float fadeDuration = 1.0f;

    void Start()
    {
        // ถ้าไม่ได้ลากใส่ไว้ใน Inspector ให้พยายามหาอัตโนมัติ
        if (fadePanel == null)
        {
            fadePanel = GetComponentInChildren<Image>();
        }

        // ตั้งค่าเริ่มต้นให้หน้าจอใส
        if (fadePanel != null)
        {
            Color c = fadePanel.color;
            c.a = 0f;
            fadePanel.color = c;
            fadePanel.gameObject.SetActive(true);
        }
    }

    public void FadeAndSwitchScene(string sceneName)
    {
        StartCoroutine(FadeRoutine(sceneName));
    }

    IEnumerator FadeRoutine(string sceneName)
    {
        float timer = 0f;
        Color startColor = fadePanel != null ? fadePanel.color : Color.black;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / fadeDuration);

            if (fadePanel != null)
            {
                fadePanel.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            }

            yield return null;
        }

        // โหลด Scene ปลายทางเมื่อ Fade ดำสนิทแล้ว
        SceneManager.LoadScene(sceneName);
    }
}