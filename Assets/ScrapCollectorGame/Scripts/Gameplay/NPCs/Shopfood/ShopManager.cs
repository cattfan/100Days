using UnityEngine;

public class ShopManager : MonoBehaviour
{
    private CurrencyManager currencyManager;
    private InventoryManager inventoryController;

    private void Awake()
    {
        currencyManager = Object.FindFirstObjectByType<CurrencyManager>();
        inventoryController = Object.FindFirstObjectByType<InventoryManager>();

        if (currencyManager == null)
            Debug.LogError("ShopManager: Không tìm thấy CurrencyManager trong scene!");
        if (inventoryController == null)
            Debug.LogError("ShopManager: Không tìm thấy InventoryController trong scene!");
    }

    public void BuyItem(ShopItem shopItem)
    {
        if (currencyManager == null || inventoryController == null || shopItem == null)
            return;

        if (currencyManager.GetCoins() >= shopItem.itemCost)
        {
            // Kiểm tra xem dữ liệu ItemData có tồn tại không
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