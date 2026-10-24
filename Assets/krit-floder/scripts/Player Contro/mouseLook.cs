using UnityEngine;

public class mouseLook : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform ort;

    float xRotation;
    float yRotation;

    // เปลี่ยนเป็น public เพื่อให้สคริปต์อื่นเข้าถึงง่าย หรือใช้ Property
    public float zRotation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // หมุนกล้องโดยรวมค่า zRotation เข้าไปด้วย
        transform.rotation = Quaternion.Euler(xRotation, yRotation, zRotation);

        if (ort != null)
            ort.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}