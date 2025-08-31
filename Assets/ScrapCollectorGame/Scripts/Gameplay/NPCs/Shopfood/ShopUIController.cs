using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ShopUIController : MonoBehaviour
{
    private ShopData shopData;
    public List<ItemSlotUI> itemSlots;

    public void SetShopData(ShopData data)
    {
        shopData = data;
        LoadShopItems();
    }

    public void LoadShopItems()
    {
        foreach (var slot in itemSlots)
        {
            if (slot != null)
                slot.gameObject.SetActive(false);
        }

        if (shopData != null && itemSlots != null)
        {
            for (int i = 0; i < shopData.shopItems.Count && i < itemSlots.Count; i++)
            {
                ShopItem item = shopData.shopItems[i];
                ItemSlotUI itemSlotUI = itemSlots[i];

                if (itemSlotUI != null)
                {
                    itemSlotUI.gameObject.SetActive(true);
                    itemSlotUI.itemData = item;
                    // Gọi hàm UpdateUI để hiển thị dữ liệu mới
                    itemSlotUI.UpdateUI();
                }
            }
        }
    }
}