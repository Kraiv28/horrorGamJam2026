using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Events; // สำคัญ: ใช้สำหรับ UnityEvent

[System.Serializable]
public class DialoguePage
{
    [TextArea(3, 5)]
    public string textContent;          // ข้อความในหน้านี้
    public Color textColor = Color.white; // สีของข้อความในหน้านี้
    public Sprite backgroundSprite;     // ภาพพื้นหลังของหน้านี้

    [Header("Special Settings")]
    [Tooltip("ติ๊กถูกถ้าต้องการให้หยุดพิมพ์กลางคันและรอให้ผู้เล่นกดปุ่มก่อนพิมพ์ต่อ")]
    public bool waitForButtonClick = false;
    [Tooltip("ตำแหน่งตัวอักษรที่จะให้หยุดรอ (นับจาก 0 เช่น พิมพ์ไป 15 ตัวแล้วให้หยุด) ถ้าใส่ 0 หรือเกินความยาวข้อความ ระบบจะหยุดรอเมื่อพิมพ์จบหน้า")]
    public int pauseAtIndex = -1;

    [Header("Unity Event")]
    [Tooltip("Event ที่จะให้ทำงานเมื่อถึงจุดหยุดรอหรือกดปุ่ม")]
    public UnityEvent onPageEvent;
}

public class DialogueManager : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI dialogueText;    // TextMeshPro สำหรับแสดงข้อความ
    public Image bgImageA;                  // ภาพพื้นหลัง A (สำหรับทำ Fade)
    public Image bgImageB;                  // ภาพพื้นหลัง B (สำหรับทำ Fade)
    public Button continueButton;           // ปุ่มสำหรับกดเพื่อให้เล่นข้อความต่อ (กรณีเป็น Text พิเศษ)

    [Header("Audio Settings")]
    public AudioSource audioSource;         // Component สำหรับเล่นเสียง
    public AudioClip typeSound;             // เสียงเอฟเฟกต์ตอนตัวอักษรขึ้น
    [Range(0.1f, 2f)]
    public float soundPitchRandomness = 0.1f; // สุ่มความทุ้ม/แหลมเล็กน้อยให้เสียงไม่น่าเบื่อ

    [Header("Settings")]
    public float typeSpeed = 0.05f;         // ความเร็วในการแสดงทีละคำ/ตัวอักษร
    public float fadeDuration = 0.5f;       // เวลาที่ใช้ในการเฟดภาพ

    [Header("Dialogue Data")]
    public List<DialoguePage> pages;        // รายการหน้าทั้งหมด
    public string nextSceneName;            // ชื่อฉากถัดไปเมื่อจบข้อความ

    private int currentPageIndex = 0;
    private bool isTyping = false;
    private bool isWaitingForClick = false; // สถานะกำลังรอผู้เล่นกดปุ่มพิเศษ
    private bool isFadeActive = false;
    private Coroutine typingCoroutine;
    private bool isImageAActive = true;     // สลับใช้งานระหว่าง Image A และ B

    void Start()
    {
        // === เพิ่มโค้ดส่วนนี้เพื่อปลดล็อกและแสดงเมาส์กลับมาแบบปกติ ===
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // ===============================================

        // ตั้งค่าปุ่มกด (ถ้ามี)
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
            continueButton.onClick.AddListener(OnContinueButtonClicked);
        }

        if (pages.Count > 0)
        {
            // ตั้งค่าภาพแรก
            if (bgImageA != null && pages[0].backgroundSprite != null)
            {
                bgImageA.sprite = pages[0].backgroundSprite;
                bgImageA.color = new Color(1, 1, 1, 1);
            }
            if (bgImageB != null)
            {
                bgImageB.color = new Color(1, 1, 1, 0);
            }

            StartPage(currentPageIndex);
        }
    }

    void Update()
    {
        // ถ้ากำลังรอให้ผู้เล่นกดปุ่มพิเศษ จะไม่ให้คลิกข้ามด้วย Spacebar ปกติในส่วนนี้
        if (isWaitingForClick) return;

        // เมื่อผู้เล่นคลิกเมาส์หรือกด Spacebar
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (isFadeActive) return; // ถ้ากำลังเฟดภาพอยู่ ให้รอ

            if (isTyping)
            {
                // ถ้าข้อความยังพิมพ์ไม่หมด ให้แสดงทั้งหมดทันทีของหน้านั้น
                StopCoroutine(typingCoroutine);
                DialoguePage currentPage = pages[currentPageIndex];
                dialogueText.text = currentPage.textContent;
                isTyping = false;

                // หากหน้านี้ต้องรอคลิกปุ่ม แต่ผู้เล่นกดข้ามด้วย Space/Click ก่อน ให้เปิดปุ่มและเรียก Event ทันที
                if (currentPage.waitForButtonClick)
                {
                    TriggerSpecialEvent(currentPage);
                }
            }
            else
            {
                // ถ้าพิมพ์เสร็จแล้ว ไปหน้าถัดไป
                currentPageIndex++;
                if (currentPageIndex < pages.Count)
                {
                    StartPage(currentPageIndex);
                }
                else
                {
                    // ถ้าหมดทุกหน้าแล้ว เปลี่ยนไปฉากต่อไป
                    LoadNextScene();
                }
            }
        }
    }

    void StartPage(int index)
    {
        DialoguePage page = pages[index];

        // ซ่อนปุ่ม Continue ตอนเริ่มหน้าใหม่เสมอ
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }

        isWaitingForClick = false;

        // กำหนดสีของข้อความตามหน้านั้นๆ
        dialogueText.color = page.textColor;

        // ตรวจสอบว่าภาพเปลี่ยนหรือไม่ เพื่อทำ Fade
        Sprite targetSprite = page.backgroundSprite;
        Sprite currentSprite = isImageAActive ? bgImageA.sprite : bgImageB.sprite;

        if (targetSprite != currentSprite)
        {
            StartCoroutine(FadeAndSwitchBackground(targetSprite));
        }

        // เริ่มพิมพ์ข้อความทีละตัว/คำ
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeTextRoutine(page));
    }

    IEnumerator TypeTextRoutine(DialoguePage page)
    {
        isTyping = true;
        dialogueText.text = "";

        char[] letters = page.textContent.ToCharArray();
        int pauseIndex = page.waitForButtonClick ? (page.pauseAtIndex >= 0 ? page.pauseAtIndex : letters.Length) : -1;

        for (int i = 0; i < letters.Length; i++)
        {
            // ตรวจสอบว่าถึงจุดที่ต้องหยุดรอปุ่มหรือไม่
            if (page.waitForButtonClick && i == pauseIndex)
            {
                isTyping = false;
                isWaitingForClick = true;

                // เรียกใช้ Unity Event ของหน้านี้
                page.onPageEvent?.Invoke();

                // แสดงปุ่มให้ผู้เล่นกด
                if (continueButton != null)
                {
                    continueButton.gameObject.SetActive(true);
                }

                // หยุดการทำงาน Coroutine นี้ไว้ก่อน จนกว่าผู้เล่นจะกดปุ่ม
                yield break;
            }

            dialogueText.text += letters[i];

            // เล่นเสียงเมื่อแสดงตัวอักษร (ไม่เล่นเสียงถ้าเป็นช่องว่าง)
            if (typeSound != null && audioSource != null && !char.IsWhiteSpace(letters[i]))
            {
                audioSource.pitch = 1f + Random.Range(-soundPitchRandomness, soundPitchRandomness);
                audioSource.PlayOneShot(typeSound);
            }

            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
    }

    // ฟังก์ชันนี้จะถูกเรียกเมื่อผู้เล่นกดปุ่ม Continue พิเศษ
    public void OnContinueButtonClicked()
    {
        if (!isWaitingForClick) return;

        // ซ่อนปุ่ม
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }

        isWaitingForClick = false;

        // พิมพ์ข้อความส่วนที่เหลือต่อจนจบ
        DialoguePage currentPage = pages[currentPageIndex];
        StartCoroutine(ResumeTyping(currentPage.textContent));
    }

    IEnumerator ResumeTyping(string fullText)
    {
        isTyping = true;
        int currentLength = dialogueText.text.Length;

        for (int i = currentLength; i < fullText.Length; i++)
        {
            dialogueText.text += fullText[i];

            if (typeSound != null && audioSource != null && !char.IsWhiteSpace(fullText[i]))
            {
                audioSource.pitch = 1f + Random.Range(-soundPitchRandomness, soundPitchRandomness);
                audioSource.PlayOneShot(typeSound);
            }

            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
    }

    void TriggerSpecialEvent(DialoguePage page)
    {
        isWaitingForClick = false;
        page.onPageEvent?.Invoke();
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }
    }

    IEnumerator FadeAndSwitchBackground(Sprite newSprite)
    {
        isFadeActive = true;

        Image activeImage = isImageAActive ? bgImageA : bgImageB;
        Image inactiveImage = isImageAActive ? bgImageB : bgImageA;

        // ตั้งค่าภาพใหม่ให้อยู่ข้างหลังและโปร่งใส
        inactiveImage.sprite = newSprite;
        inactiveImage.color = new Color(1, 1, 1, 0);

        float elapsedTime = 0f;

        // ทำการ Fade In / Fade Out สลับกัน
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);

            inactiveImage.color = new Color(1, 1, 1, alpha);
            activeImage.color = new Color(1, 1, 1, 1f - alpha);

            yield return null;
        }

        // สลับสถานะภาพหลัก
        isImageAActive = !isImageAActive;
        isFadeActive = false;
    }

    void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("ยังไม่ได้ระบุชื่อ Scene ถัดไปใน Inspector!");
        }
    }
}