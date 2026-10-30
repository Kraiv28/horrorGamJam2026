using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TeleportController : MonoBehaviour
{
    [Header("UI Settings")]
    [Tooltip("ลาก Panel สีดำใน Canvas มาใส่ตรงนี้")]
    public Image fadePanel;
    [Tooltip("ความเร็วในการเฟด (ค่าน้อยยิ่งเร็ว)")]
    public float fadeSpeed = 1.5f;

    [Header("Teleport Settings")]
    [Tooltip("ตำแหน่งที่จะให้ผู้เล่นวาปไป")]
    public Transform targetLocation;

    private bool isTeleporting = false;

    // ฟังก์ชันนี้สามารถเรียกใช้ผ่านปุ่ม UI, Trigger หรือคลิกเมาส์ก็ได้
    public void TriggerTeleport()
    {
        if (!isTeleporting)
        {
            StartCoroutine(TeleportRoutine());
        }
    }

    private IEnumerator TeleportRoutine()
    {
        isTeleporting = true;

        // 1. ค่อยๆ เฟดภาพเป็นสีดำ (Fade In)
        yield return StartCoroutine(FadeRoutine(0f, 1f));

        // 2. ทำการย้ายตำแหน่งผู้เล่นขณะที่จอเป็นสีดำสนิท
        // สมมติว่าสคริปต์นี้ติดอยู่กับ Player หรืออ้างอิงผ่าน Tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && targetLocation != null)
        {
            // ถ้าใช้ CharacterController ต้องปิดชั่วคราวก่อนย้ายตำแหน่งเพื่อป้องกันบั๊ก
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.transform.position = targetLocation.position;

            if (cc != null) cc.enabled = true;
        }
        else
        {
            Debug.LogWarning("ไม่พบ Player (เช็ค Tag ว่าชื่อ Player หรือยัง) หรือยังไม่ได้กำหนด Target Location");
        }

        // รอสักครู่ (ถ้าต้องการ)
        yield return new WaitForSeconds(0.2f);

        // 3. ค่อยๆ เฟดภาพกลับมาสว่างปกติ (Fade Out)
        yield return StartCoroutine(FadeRoutine(1f, 0f));

        isTeleporting = bool.FalseString == "True"; // หรือ isTeleporting = false;
        isTeleporting = false;
    }

    private IEnumerator FadeRoutine(float startAlpha, float endAlpha)
    {
        if (fadePanel == null) yield break;

        float elapsedTime = 0f;
        Color color = fadePanel.color;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * fadeSpeed;
            color.a = Mathf.Lerp(startAlpha, endAlpha, elapsedTime);
            fadePanel.color = color;
            yield return null;
        }

        color.a = endAlpha;
        fadePanel.color = color;
    }
}