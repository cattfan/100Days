using UnityEngine;

public class ShopManager : MonoBehaviour
{
    private Inventory inventoryController;
    private SaveController saveController;

    public GameObject shopUI;

    public NpcShopInteraction npcShopInteraction;

    private void Awake()
    {
        saveController = Object.FindFirstObjectByType<SaveController>();
        inventoryController = Object.FindFirstObjectByType<Inventory>();

        if (inventoryController == null)
        {
            Debug.LogError("ShopManager: Không tìm thấy InventoryController trong scene!");
        }

        if (saveController == null)
        {
            Debug.LogError("ShopManager: Không tìm thấy SaveController trong scene!");
        }

        npcShopInteraction = Object.FindFirstObjectByType<NpcShopInteraction>();
        if (npcShopInteraction == null)
        {
            Debug.LogError("ShopManager: Không tìm thấy NpcShopInteraction trong scene!");
        }
    }

    public void BuyItem(ShopItem shopItem)
    {
        if (inventoryController == null || saveController == null || shopItem == null)
        {
            Debug.LogError("ShopManager: Thiếu tham chiếu cần thiết để mua vật phẩm.");
            return;
        }

        if (saveController.currencyManager.GetCoins() >= shopItem.itemCost)
        {
            if (shopItem.itemData != null)
            {
                bool added = inventoryController.AddItem(shopItem.itemData, 1);
                if (added)
                {
                    saveController.currencyManager.AddCoins(-shopItem.itemCost);
                    ItemPickupUIController.Instance?.ShowItemPickup(shopItem.itemName, shopItem.itemData.itemIcon);
                    Debug.Log($"✅ Successfully bought {shopItem.itemName}!");
                }
                else
                {
                    Debug.Log("❌ Failed to buy item - inventory full!");
                }
            }
        }
        else
        {
            ItemPickupUIController.Instance?.ShowWarningPopup("Không đủ tiền để mua vật phẩm này!");
        }
    }

    public void CloseShop()
    {
        if (shopUI != null)
        {
            shopUI.SetActive(false);

            if (npcShopInteraction != null)
            {
                npcShopInteraction.CloseShop();
            }
        }
        else
        {
            Debug.LogError("ShopManager: Chưa gán GameObject của giao diện shop.");
        }
    }
}