using UnityEngine;

public class QuitGameButton : MonoBehaviour
{
    public void QuitGame()
    {
        Debug.Log("กำลังออกจากเกม..."); // จะแสดงข้อความนี้ใน Console ตอนทดสอบใน Editor

        // คำสั่งปิดเกม (ใช้ได้กับ Build เป็น Application จริง เช่น PC, Android, iOS)
        Application.Quit();

        // ถ้ารันอยู่ใน Unity Editor คำสั่ง Application.Quit() จะไม่ปิดโปรแกรม 
        // เราจึงใช้บรรทัดนี้เพื่อให้หยุด Play Mode ใน Editor ด้วย (เวลาเทสจะได้รู้ว่าโค้ดทำงาน)
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}