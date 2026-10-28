using UnityEngine;
using UnityEngine.UI; // ไอคอน UI ต้องใช้ Namespace นี้

public class playerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRange = 3f;
    public LayerMask itemLayer;
    public LayerMask interactableLayer;
    public LayerMask noteLayer; // เลเยอร์สำหรับโน้ต (เพิ่มใหม่)

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
    [Tooltip("รูปเป้าเล็งเมื่อมองโดนโน้ต (ใช้รูปเดียวกับไอเทมหรือแยกก็ได้)")]
    public Sprite noteHoverPointerSprite; // เพิ่มใหม่

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
        if (pointerUI == null || defaultPointerSprite == null) return;

        if (conditionalAnimationInteractable.IsAnyAnimationPlaying)
        {
            pointerUI.sprite = defaultPointerSprite;
            return;
        }

        RaycastHit hit;
        // รวม noteLayer เข้าไปในการตรวจจับด้วย
        LayerMask combinedMask = itemLayer | interactableLayer | noteLayer;

        if (Physics.Raycast(transform.position, transform.forward, out hit, interactionRange, combinedMask))
        {
            // 1. ถ้ามองเจอไอเทมทั่วไป (itemObject)
            if (hit.collider.TryGetComponent(out itemObject itemObj))
            {
                if (currentHeldItemId == -1 && itemHoverPointerSprite != null)
                {
                    pointerUI.sprite = itemHoverPointerSprite;
                    return;
                }
            }

            // 2. ถ้ามองเจอไอเทมพิเศษ (InstantEventItem)
            if (hit.collider.TryGetComponent(out InstantEventItem instantItem))
            {
                if (itemHoverPointerSprite != null) pointerUI.sprite = itemHoverPointerSprite;
                return;
            }

            // 3. ถ้ามองเจอวัตถุคัตซีน (conditionalAnimationInteractable)
            if (hit.collider.TryGetComponent(out conditionalAnimationInteractable animInteractable))
            {
                if (conditionalAnimHoverSprite != null) pointerUI.sprite = conditionalAnimHoverSprite;
                return;
            }

            // 4. [เพิ่มเข้ามาใหม่] ถ้ามองเจอโน้ต (NoteController)
            if (hit.collider.TryGetComponent(out NoteController noteObj))
            {
                // ถ้ามีรูปแยกให้ใช้รูปโน้ต ถ้าไม่มีให้ใช้รูปไอเทมแทน
                pointerUI.sprite = noteHoverPointerSprite != null ? noteHoverPointerSprite : itemHoverPointerSprite;
                return;
            }
        }

        pointerUI.sprite = defaultPointerSprite;
    }

    void CheckInteraction()
    {
        RaycastHit hit;
        LayerMask combinedMask = itemLayer | interactableLayer | noteLayer;

        // ยิง Raycast เช็กวัตถุ
        if (Physics.Raycast(transform.position, transform.forward, out hit, interactionRange, combinedMask))
        {
            // [เพิ่มเข้ามาใหม่] เช็กโน้ต (เปิดอ่าน)
            if (hit.collider.TryGetComponent(out NoteController noteObj))
            {
                noteObj.ShowNote();
                return;
            }

            // 1. เช็กไอเทมทั่วไป (เก็บเข้ามือ)
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

            // 2. เช็กไอเทมพิเศษ (InstantEventItem)
            if (hit.collider.TryGetComponent(out InstantEventItem instantItemObj))
            {
                instantItemObj.PickUpInstant();
                if (camEffects != null)
                {
                    camEffects.ApplyPickupLean(pickupLeanIntensity);
                }
                return;
            }

            // 3. เช็กวัตถุคัตซีน (conditionalInteractable)
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

            // 4. เช็กประตูและอื่นๆ ต่อตามเดิม...
            if (hit.collider.TryGetComponent(out TransitionDoor doorObj))
            {
                doorObj.InteractWithDoor();
                return;
            }

            if (hit.collider.TryGetComponent(out TestEventTrigger testTrigger))
            {
                testTrigger.Interact();
                return;
            }

            if (hit.collider.TryGetComponent(out DoorInteractable doorInteractable))
            {
                doorInteractable.Interact(currentHeldItemId);
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