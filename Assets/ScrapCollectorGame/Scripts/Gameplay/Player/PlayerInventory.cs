using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    public List<ShopItem> inventory;

    private CurrencyManager currencyManager;

    void Awake()
    {
        // Thay thế FindObjectOfType bằng FindAnyObjectByType để code của bạn được cập nhật
        currencyManager = FindAnyObjectByType<CurrencyManager>();
    }

    public void BuyItem(ShopItem itemToBuy)
    {
        if (currencyManager == null)
        {
            Debug.LogError("CurrencyManager not found in the scene.");
            return;
        }

        // Kiểm tra xem người chơi có đủ tiền hay không
        if (currencyManager.GetCoins() >= itemToBuy.itemCost)
        {
            // Trừ tiền bằng cách gọi hàm SpendCoins() từ CurrencyManager
            currencyManager.SpendCoins(itemToBuy.itemCost);

            // Thêm vật phẩm vào kho đồ của người chơi
            inventory.Add(itemToBuy);

            Debug.Log($"Đã mua {itemToBuy.itemName}. Tiền còn lại: {currencyManager.GetCoins()}");
        }
        else
        {
            Debug.Log("Không đủ tiền để mua!");
        }
    }
}