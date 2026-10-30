using UnityEngine;
using UnityEngine.Events;

public class ItemDropBox : MonoBehaviour
{
    [Header("Box Settings")]
    [Tooltip("กำหนด ID ของ Item ที่กล่องนี้ต้องการ")]
    public int targetItemId;

    [Tooltip("จำนวนชิ้นที่ต้องการทั้งหมด")]
    public int requiredAmount = 3;

    [Header("Current Status")]
    [Tooltip("จำนวนปัจจุบันที่ใส่เข้าไปแล้ว (อ่านอย่างเดียวเพื่อเช็กค่า)")]
    [SerializeField] private int currentAmount = 0;

    [Header("Events")]
    [Tooltip("เหตุการณ์ที่จะเกิดขึ้นเมื่อใส่ไอเทมครบตามจำนวนแล้ว")]
    public UnityEvent onCompleteEvent;

    private bool isCompleted = false;

    // ใช้ OnTriggerEnter เพื่อเช็กว่ามี Item ตกลงมาโดนกล่องหรือ Trigger ของกล่อง
    private void OnTriggerEnter(Collider other)
    {
        if (isCompleted) return;

        // เช็กว่าวัตถุที่ตกลงมามีสคริปต์ itemObject หรือไม่
        if (other.TryGetComponent(out itemObject itemObj))
        {
            if (itemObj.itemData != null && itemObj.itemData.itemId == targetItemId)
            {
                currentAmount++;
                Debug.Log($"[ItemDropBox] ใส่ไอเทมสำเร็จ: {itemObj.itemData.itemName} ({currentAmount}/{requiredAmount})");

                // ทำลายไอเทมชิ้นนั้นทิ้ง (หรือจะเก็บเข้ากล่องก็ได้ แต่นี่ทำลายเพื่อเคลียร์ฉาก)
                Destroy(other.gameObject);

                // ตรวจสอบว่าครบจำนวนหรือยัง
                CheckCompletion();
            }
            else
            {
                Debug.Log("[ItemDropBox] ไอเทมนี้ไม่ใช่สิ่งที่กล่องต้องการ!");
            }
        }
    }

    void CheckCompletion()
    {
        if (currentAmount >= requiredAmount)
        {
            isCompleted = true;
            Debug.Log("[ItemDropBox] ใส่ไอเทมครบตามจำนวนแล้ว! ทำการเรียกใช้ Unity Event");

            // สั่งทำงาน Unity Event ที่ตั้งค่าไว้ใน Inspector
            onCompleteEvent?.Invoke();
        }
    }
}