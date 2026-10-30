using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    // ตัวแปรเช็คสถานะการหยุดเกม
    public static bool isGamePaused = false;

    // อ้างอิงถึง UI Panel ของ Pause Menu
    public GameObject pauseMenuUI;

    void Update()
    {
        // เช็คการกดปุ่ม ESC (หรือปุ่ม P)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isGamePaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    // ฟังก์ชันกลับมาเล่นเกมต่อ
    public void Resume()
    {
        pauseMenuUI.SetActive(false); // ซ่อน UI
        Time.timeScale = 1f;          // คืนค่าเวลาเกมปกติ
        isGamePaused = false;
    }

    // ฟังก์ชันหยุดเกม
    void Pause()
    {
        pauseMenuUI.SetActive(true);  // แสดง UI
        Time.timeScale = 0f;          // หยุดเวลาเกม (การเคลื่อนที่/ฟิสิกส์หยุดทำงาน)
        isGamePaused = true;
    }

    // ฟังก์ชันกลับไปยังหน้าหลัก (Main Menu)
    public void LoadMenu()
    {
        Time.timeScale = 1f; // ต้องคืนเวลาเป็นปกติก่อนเปลี่ยน Scene
        SceneManager.LoadScene("MainMenu"); // ใส่ชื่อ Scene หน้าหลักของคุณตรงนี้
    }

    // ฟังก์ชันออกจากเกม
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit(); // ทำงานเฉพาะเมื่อ Build ออกเป็นตัวเกมจริง (.exe / .apk)
    }
}