using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class Inventory
{
    public List<InventoryItemData> GetInventoryData()
    {
        var inventoryData = new List<InventoryItemData>();
        for (int i = 0; i < GetSlots().Count; i++)
        {
            var slot = GetSlots()[i];
            if (slot.currentItem != null)
            {
                var itemUI = slot.currentItem.GetComponent<ItemUI>();
                if (itemUI != null && itemUI.GetItemData() != null)
                {
                    inventoryData.Add(new InventoryItemData(
                        itemUI.GetItemData().name,
                        itemUI.Amount,
                        i
                    ));
                }
            }
        }
        return inventoryData;
    }

    public void LoadInventoryData(List<InventoryItemData> inventoryData)
    {
        // Bước 1: Luôn dọn dẹp kho đồ trước khi tải dữ liệu mới
        ClearInventory();
        Debug.Log("Inventory cleared for loading.");

        if (inventoryData == null || inventoryData.Count == 0)
        {
            Debug.LogWarning("LoadInventoryData: No data to load or data is null.");
            return;
        }

        Debug.Log($"LoadInventoryData: Attempting to load {inventoryData.Count} items.");

        // Bước 2: Duyệt qua từng vật phẩm đã lưu
        foreach (var item in inventoryData)
        {
            // Bước 2a: Log tên vật phẩm đang được tải
            Debug.Log($"Loading item: {item.itemName} at slot {item.slotIndex} with amount {item.amount}.");

            var itemData = FindItemDataByName(item.itemName);

            // Bước 2b: Log kết quả tìm kiếm dữ liệu vật phẩm
            if (itemData == null)
            {
                Debug.LogError($"LoadInventoryData: Could not find ItemData for '{item.itemName}'. Skipping.");
            }

            // Đảm bảo dữ liệu vật phẩm và vị trí trong kho là hợp lệ
            if (itemData != null && item.slotIndex >= 0 && item.slotIndex < GetSlots().Count)
            {
                // Bước 3: Gọi hàm CreateItemAtSlot để tạo vật phẩm tại đúng vị trí đã lưu
                CreateItemAtSlot(itemData, item.amount, item.slotIndex);
            }
            else
            {
                Debug.LogError($"LoadInventoryData: Invalid slot index or missing ItemData for '{item.itemName}'. Index: {item.slotIndex}");
            }
        }
        Debug.Log($"[DEBUG] Slots occupied after load: {GetSlots().Count(s => s.currentItem != null)}");
    }
}
