using UnityEngine;

public class mouseLook : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform ort;

    float xRotation;
    float yRotation;

    public float zRotation;

    // [เพิ่มเข้ามา] ตัวแปรสำหรับเปิด-ปิดการหันกล้อง (เช่น เวลาเปิด UI อ่านโน้ต)
    public bool canLook = true;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        // ถ้าถูกสั่งห้ามหันกล้อง ให้ข้ามการคำนวณเมาส์ไปเลย
        if (!canLook) return;

        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, zRotation);

        if (ort != null)
            ort.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}