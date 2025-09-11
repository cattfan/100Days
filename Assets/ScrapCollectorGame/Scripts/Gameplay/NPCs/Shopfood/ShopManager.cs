using UnityEngine;

public class ShopManager : MonoBehaviour
{
    private Inventory inventoryController;
    private SaveController saveController; // Add this field to fix CS0103

    private void Awake()
    {
        saveController = Object.FindFirstObjectByType<SaveController>();
        inventoryController = Object.FindFirstObjectByType<Inventory>();

        if (inventoryController == null)
            Debug.LogError("ShopManager: Không tìm thấy InventoryController trong scene!");
    }

    public void BuyItem(ShopItem shopItem)
    {
        if (inventoryController == null || shopItem == null)
            return;

        if (saveController.currencyManager.GetCoins() >= shopItem.itemCost)
        {
            // Kiểm tra xem dữ liệu ItemData có tồn tại không
            if (shopItem.itemData != null)
            {

                inventoryController.AddItem(shopItem.itemData, 1);
                ItemPickupUIController.Instance?.ShowItemPickup(shopItem.itemName, shopItem.itemData.itemIcon);
            }
            else
            {
                Debug.LogError("Lỗi: Dữ liệu ItemData của vật phẩm " + shopItem.itemName + " bị thiếu.");
            }
        }
        else
        {
            ItemPickupUIController.Instance?.ShowWarningPopup("Không đủ tiền để mua vật phẩm này!");
        }
    }
}