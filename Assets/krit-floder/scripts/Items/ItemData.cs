using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public int itemId;
    public string itemName;
    public GameObject itemPrefab; // โมเดลที่จะให้มาโผล่ในมือ
}