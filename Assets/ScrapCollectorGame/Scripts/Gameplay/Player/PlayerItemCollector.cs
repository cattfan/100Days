<<<<<<< HEAD
﻿using UnityEngine;
using System.Collections.Generic;

public class PlayerItemCollector : MonoBehaviour
{
    // Sử dụng Singleton nếu InventoryController là Singleton
    // private InventoryController inventoryController => InventoryController.Instance;
    private InventoryController inventoryController;

    void Start()
    {
        // Giữ lại cách này nếu InventoryController không phải là Singleton
        inventoryController = FindAnyObjectByType<InventoryController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Item"))
        {
            ItemPickup itemPickup = collision.gameObject.GetComponent<ItemPickup>();
            if (itemPickup != null)
            {
                if (itemPickup.CanBePickedUp())
                {
                    ItemData itemData = itemPickup.itemData;
                    int currentAmount = itemPickup.currentAmount;

                    if (itemData != null)
                    {
                        if (ItemPickupUIController.Instance != null)
                        {
                            string displayText = itemData.itemName;
                            if (currentAmount > 1)
                            {
                                displayText += $" x{currentAmount}";
                            }
                            ItemPickupUIController.Instance.ShowItemPickup(displayText, itemData.itemIcon);
                        }

                        // Đảm bảo inventoryController không null trước khi gọi
                        if (inventoryController != null)
                        {
                            inventoryController.AddItem(itemData, currentAmount);
                        }
                        else
                        {
                            Debug.LogError("PlayerItemCollector: InventoryController is null!");
                        }

                        Debug.Log($"Đã thu thập: {itemData.itemName} x{currentAmount} (ID: {itemData.itemID})");

                        if (itemPickup.audioManagement != null)
                        {
                            itemPickup.audioManagement.PlaySFX(itemPickup.audioManagement.PickupItem);
                        }

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
=======
﻿
>>>>>>> origin/main
