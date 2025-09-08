using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    public List<ShopItem> inventory;

    // Không cần CurrencyManager ở đây, vì logic mua hàng sẽ nằm ở ShopManager
    // private CurrencyManager currencyManager;

    void Awake()
    {
        // Loại bỏ dòng này vì không còn cần thiết
        // currencyManager = FindAnyObjectByType<CurrencyManager>();
    }

    // Xóa hoàn toàn hàm BuyItem này
    // public void BuyItem(ShopItem itemToBuy)
    // {
    //     ...
    // }
}