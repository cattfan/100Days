using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ShopUIController : MonoBehaviour
{
    // Biến này nhận dữ liệu từ NPC
    private ShopData shopData;

    // Public list để kéo 7 ô vật phẩm đã có sẵn vào
    public List<ItemSlotUI> itemSlots;

    public void SetShopData(ShopData data)
    {
        shopData = data;
        LoadShopItems();
    }

    public void LoadShopItems()
    {
        // Vô hiệu hóa tất cả các ô vật phẩm trước
        foreach (var slot in itemSlots)
        {
            if (slot != null)
                slot.gameObject.SetActive(false);
        }

        // Gán dữ liệu và bật các ô vật phẩm cần thiết
        if (shopData != null && itemSlots != null)
        {
            for (int i = 0; i < shopData.shopItems.Count && i < itemSlots.Count; i++)
            {
                ShopItem item = shopData.shopItems[i];
                ItemSlotUI itemSlotUI = itemSlots[i];

                if (itemSlotUI != null)
                {
                    itemSlotUI.gameObject.SetActive(true); // Bật ô vật phẩm lên
                    itemSlotUI.itemData = item; // Gán dữ liệu vật phẩm
                    itemSlotUI.UpdateUI(); // Cập nhật UI
                }
            }
        }
    }
}