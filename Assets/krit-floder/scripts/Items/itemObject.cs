using UnityEngine;

public class itemObject : MonoBehaviour
{
    public ItemData itemData; // ลากไฟล์ ItemData ที่สร้างไว้มาใส่

    public ItemData PickUp()
    {
        Destroy(gameObject); // ทำลาย Object บนพื้น
        return itemData;
    }
}