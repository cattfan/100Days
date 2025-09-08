using UnityEngine;

public class ShopManager : MonoBehaviour
{
<<<<<<< HEAD
    // Access the Singleton instance directly, ensuring consistent access
    private CurrencyManager currencyManager => CurrencyManager.Instance;
    private InventoryController inventoryController;

    private void Awake()
    {
        // This remains the same as InventoryController is likely not a Singleton
        inventoryController = Object.FindFirstObjectByType<InventoryController>();
=======
    private CurrencyManager currencyManager;
    private InventoryManager inventoryController;

    private void Awake()
    {
        currencyManager = Object.FindFirstObjectByType<CurrencyManager>();
        inventoryController = Object.FindFirstObjectByType<InventoryManager>();
>>>>>>> origin/main

        if (inventoryController == null)
            Debug.LogError("ShopManager: Không tìm thấy InventoryController trong scene!");
    }

    public void BuyItem(ShopItem shopItem)
    {
        // Check if the Singleton instance exists before using it
        if (currencyManager == null || inventoryController == null || shopItem == null)
        {
            Debug.LogError("Không thể mua hàng: Thiếu tham chiếu cần thiết.");
            return;
        }

        if (currencyManager.GetCoins() >= shopItem.itemCost)
        {
            if (shopItem.itemData != null)
            {
                currencyManager.SpendCoins(shopItem.itemCost);
                inventoryController.AddItem(shopItem.itemData, 1);
                Debug.Log("Mua " + shopItem.itemName + " thành công!");
            }
            else
            {
                Debug.LogError("Lỗi: Dữ liệu ItemData của vật phẩm " + shopItem.itemName + " bị thiếu.");
            }
        }
        else
        {
            Debug.Log("Không đủ tiền để mua " + shopItem.itemName);
        }
    }
}