using UnityEngine;
using UnityEngine.UI; // ไอคอน UI ต้องใช้ Namespace นี้

public class playerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRange = 3f;
    public LayerMask itemLayer;
    public LayerMask interactableLayer;

    [Header("Hold Settings")]
    public Transform itemHolder;
    public int currentHeldItemId = -1;
    private ItemData currentHeldItemData;
    private GameObject currentItemModel;

    [Header("Effects")]
    public cameraEffects camEffects;
    public float pickupLeanIntensity = 1f;

    [Header("Mouse Pointer UI Settings")]
    [Tooltip("ลาก Object 'Image' ตรงกลางหน้าจอมาใส่ช่องนี้")]
    public Image pointerUI;
    [Tooltip("รูปเป้าเล็งปกติ (ตอนที่ไม่ได้มองอะไรเลย)")]
    public Sprite defaultPointerSprite;
    [Tooltip("รูปเป้าเล็งที่จะเปลี่ยนไป เมื่อผู้เล่นหันไปชี้ ไอเทมที่เก็บขึ้นมาได้")]
    public Sprite itemHoverPointerSprite;
    [Tooltip("รูปเป้าเล็งที่จะเปลี่ยนไป เมื่อผู้เล่นหันไปชี้ วัตถุคัตซีนที่ต้องการไอเทมในการเปิดใช้งาน")]
    public Sprite conditionalAnimHoverSprite;

    void Update()
    {
        // ตรวจสอบสายตาผู้เล่นตลอดเวลาเพื่ออัปเดตสถานะเป้าเมาส์ (Pointer)
        UpdateMousePointerUI();

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            CheckInteraction();
        }

        // เช็คการกดปุ่ม Q เพื่อโยนของ
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // ดักจับ: ถ้าคัตซีนกำลังเล่นอยู่ จะ return ทันที ไม่ยอมให้โยนของ
            if (conditionalAnimationInteractable.IsAnyAnimationPlaying)
            {
                Debug.LogWarning("[Player Interaction] คัตซีนกำลังทำงาน ห้ามโยนของเด็ดขาด!");
                return;
            }

            DropItem();
        }
    }

    /// <summary>
    /// ฟังก์ชันเช็กสายตาผู้เล่นแบบ Real-time เพื่อเปลี่ยนรูป Pointer ตามประเภทวัตถุ
    /// </summary>
    void UpdateMousePointerUI()
    {
        // ป้องกัน Error หากยังไม่ได้ใส่ข้อมูล UI ใน Inspector
        if (pointerUI == null || defaultPointerSprite == null || itemHoverPointerSprite == null || conditionalAnimHoverSprite == null) return;

        // --- เงื่อนไขที่เพิ่มเข้ามาใหม่ ---
        // ถ้าคัตซีนหรือแอนิเมชั่นกำลังทำงานอยู่ ให้บังคับเป้าเล็งเป็นแบบปกติทันที ไม่ต้องคำนวณ Raycast ด้านล่างต่อ
        if (conditionalAnimationInteractable.IsAnyAnimationPlaying)
        {
            pointerUI.sprite = defaultPointerSprite;
            return;
        }

        RaycastHit hit;

        // ผสม LayerMask ทั้งไอเทมทั่วไปและวัตถุอินเตอร์แอคเข้าด้วยกันเพื่อยิงสายตาไปเช็กทีเดียว
        LayerMask combinedMask = itemLayer | interactableLayer;

        if (Physics.Raycast(transform.position, transform.forward, out hit, interactionRange, combinedMask))
        {
            // เคสที่ 1: มองไปเจอไอเทมที่เก็บได้ (เช่น ชะแลง, กุญแจที่วางอยู่บนพื้น)
            if (hit.collider.TryGetComponent(out itemObject itemObj))
            {
                // เปลี่ยนเป้าก็ต่อเมื่อในมือเรายังว่างอยู่
                if (currentHeldItemId == -1)
                {
                    pointerUI.sprite = itemHoverPointerSprite;
                    return; // คืนค่าออกทันทีเมื่อตรงเงื่อนไข
                }
            }

            // เคสที่ 2: มองไปเจอวัตถุคัตซีนที่ต้องการไอเทม (conditionalAnimationInteractable)
            if (hit.collider.TryGetComponent(out conditionalAnimationInteractable animInteractable))
            {
                // เปลี่ยนเป้าเป็นรูปสำหรับจุดคัตซีนทันที (โชว์เพื่อให้รู้ว่าตรงนี้กดใช้ของได้)
                pointerUI.sprite = conditionalAnimHoverSprite;
                return; // คืนค่าออกทันทีเมื่อตรงเงื่อนไข
            }
        }

        // เคสที่ 3: ถ้าส่องไปไม่เจออะไรเลย หรือเจอสิ่งที่ไม่เกี่ยวข้องกัน -> คืนค่าเป้าจุดกลมสีขาวปกติ
        pointerUI.sprite = defaultPointerSprite;
    }

    void CheckInteraction()
    {
        RaycastHit hit;
        LayerMask combinedMask = itemLayer | interactableLayer;

        if (Physics.Raycast(transform.position, transform.forward, out hit, interactionRange, combinedMask))
        {
            if (hit.collider.TryGetComponent(out itemObject itemObj))
            {
                if (currentHeldItemId != -1)
                {
                    Debug.Log("[Player Interaction] ในมือมีของอยู่แล้ว!");
                    return;
                }

                HoldItem(itemObj.PickUp());
                return;
            }

            if (hit.collider.TryGetComponent(out conditionalInteractable conditionalObj))
            {
                conditionalObj.Interact(currentHeldItemId);
                return;
            }

            if (hit.collider.TryGetComponent(out conditionalAnimationInteractable animInteractable))
            {
                animInteractable.Interact(currentHeldItemId, this);
                return;
            }
        }
    }

    void HoldItem(ItemData data)
    {
        if (data == null) return;

        currentHeldItemData = data;
        currentHeldItemId = data.itemId;

        if (currentItemModel != null) Destroy(currentItemModel);

        if (data.itemPrefab != null)
        {
            currentItemModel = Instantiate(data.itemPrefab, itemHolder);
            currentItemModel.transform.localPosition = Vector3.zero;
            currentItemModel.transform.localRotation = Quaternion.identity;

            // ตอนถือบนมือ สั่งปิดระบบฟิสิกส์ชั่วคราวเพื่อไม่ให้ร่วง
            if (currentItemModel.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
            foreach (var col in currentItemModel.GetComponentsInChildren<Collider>()) col.enabled = false;
        }

        if (camEffects != null)
        {
            camEffects.ApplyPickupLean(pickupLeanIntensity);
        }
    }

    public void DropItem()
    {
        if (currentHeldItemId == -1 || currentHeldItemData == null) return;

        Debug.Log($"[Interact] โยนไอเทม: {currentHeldItemData.itemName}");

        if (currentHeldItemData.itemPrefab != null)
        {
            Vector3 spawnPosition = transform.position + transform.forward * 1.2f + transform.up * 0.5f;
            GameObject droppedItem = Instantiate(currentHeldItemData.itemPrefab, spawnPosition, Quaternion.identity);

            if (droppedItem.TryGetComponent(out Rigidbody rb))
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.AddForce(transform.forward * 4f, ForceMode.Impulse);
            }
            else
            {
                Rigidbody newRb = droppedItem.AddComponent<Rigidbody>();
                newRb.isKinematic = false;
                newRb.useGravity = true;
                newRb.AddForce(transform.forward * 4f, ForceMode.Impulse);
            }

            foreach (var col in droppedItem.GetComponentsInChildren<Collider>())
            {
                col.enabled = true;
            }

            if (!droppedItem.TryGetComponent(out itemObject io))
            {
                itemObject newIo = droppedItem.AddComponent<itemObject>();
                newIo.itemData = currentHeldItemData;
            }
        }

        ConsumeHeldItem();
    }

    public void ConsumeHeldItem()
    {
        if (currentItemModel != null) Destroy(currentItemModel);
        currentHeldItemId = -1;
        currentHeldItemData = null;
    }
}