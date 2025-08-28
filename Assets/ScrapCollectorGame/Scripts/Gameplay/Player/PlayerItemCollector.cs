using UnityEngine;

public class PlayerItemCollector : MonoBehaviour
{
    //Private InventoryController inventoryController;

    void Start()
    {
        //inventoryController = FindObjectOfType<InventoryController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Item"))
        {
            // Thay đổi từ Item thành ItemPickup
            ItemPickup itemPickup = collision.gameObject.GetComponent<ItemPickup>();
            if (itemPickup != null)
            {
                // Kiểm tra xem item có sẵn sàng pickup không
                if (itemPickup.CanBePickedUp())
                {
                    // Lấy thông tin item từ ItemData
                    ItemData itemData = itemPickup.itemData;
                    int currentAmount = itemPickup.currentAmount;

                    if (itemData != null)
                    {
                        // 🎯 HIỂN THỊ POPUP UI
                        if (ItemPickupUIController.Instance != null)
                        {
                            string displayText = itemData.itemName;
                            if (currentAmount > 1)
                            {
                                displayText += $" x{currentAmount}";
                            }
                            ItemPickupUIController.Instance.ShowItemPickup(displayText, itemData.itemIcon);
                        }

                        // Add the item to the inventory
                        // inventoryController.AddItem(itemData, currentAmount);

                        // Debug log để kiểm tra
                        Debug.Log($"Đã thu thập: {itemData.itemName} x{currentAmount} (ID: {itemData.itemID})");

                        // Play pickup sound nếu có
                        if (itemPickup.audioManagement != null)
                        {
                            itemPickup.audioManagement.PlaySFX(itemPickup.audioManagement.PickupItem);
                        }

                        // Destroy the item after collection
                        Destroy(collision.gameObject);
                    }
                    else
                    {
                        Debug.LogWarning("ItemData is null on " + collision.gameObject.name);
                    }
                }
                else
                {
                    Debug.Log($"Item {itemPickup.GetItemInfo()} is not ready to be picked up yet!");
                }
            }
            else
            {
                Debug.LogWarning($"GameObject {collision.gameObject.name} has 'Item' tag but no ItemPickup component!");
            }
        }
    }
}