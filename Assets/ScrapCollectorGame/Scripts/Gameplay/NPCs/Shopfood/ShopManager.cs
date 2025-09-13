using UnityEngine;

public class ShopManager : MonoBehaviour
{
    private Inventory inventoryController;
    private SaveController saveController;

    // Biến để chứa GameObject của giao diện shop
    public GameObject shopUI;

    // Tham chiếu đến script NpcShopInteraction để có thể đóng shop từ đây.
    public NpcShopInteraction npcShopInteraction;

    private void Awake()
    {
        // Tìm và gán các controller cần thiết
        saveController = Object.FindFirstObjectByType<SaveController>();
        inventoryController = Object.FindFirstObjectByType<Inventory>();

        // Kiểm tra xem các controller đã được tìm thấy chưa để tránh lỗi null reference
        if (inventoryController == null)
        {
            Debug.LogError("ShopManager: Không tìm thấy InventoryController trong scene!");
        }

        if (saveController == null)
        {
            Debug.LogError("ShopManager: Không tìm thấy SaveController trong scene!");
        }

        // Lấy tham chiếu đến NpcShopInteraction
        npcShopInteraction = Object.FindFirstObjectByType<NpcShopInteraction>();
        if (npcShopInteraction == null)
        {
            Debug.LogError("ShopManager: Không tìm thấy NpcShopInteraction trong scene!");
        }
    }

    public void BuyItem(ShopItem shopItem)
    {
        // Kiểm tra các tham chiếu để tránh lỗi
        if (inventoryController == null || saveController == null || shopItem == null)
        {
            Debug.LogError("ShopManager: Thiếu tham chiếu cần thiết để mua vật phẩm.");
            return;
        }

        // Kiểm tra xem người chơi có đủ tiền không
        if (saveController.currencyManager.GetCoins() >= shopItem.itemCost)
        {
            if (shopItem.itemData != null)
            {
                // Trừ tiền của người chơi
                saveController.currencyManager.AddCoins(-shopItem.itemCost);

                // Thêm vật phẩm vào kho đồ
                inventoryController.AddItem(shopItem.itemData, 1);

                // Hiển thị thông báo đã mua
                ItemPickupUIController.Instance?.ShowItemPickup(shopItem.itemName, shopItem.itemData.itemIcon);
            }
            else
            {
                Debug.LogError("Lỗi: Dữ liệu ItemData của vật phẩm " + shopItem.itemName + " bị thiếu.");
            }
        }
        else
        {
            // Hiển thị thông báo không đủ tiền
            ItemPickupUIController.Instance?.ShowWarningPopup("Không đủ tiền để mua vật phẩm này!");
        }
    }

    // Phương thức đóng shop
    public void CloseShop()
    {
        if (shopUI != null)
        {
            shopUI.SetActive(false);

            // Bổ sung: Gọi phương thức CloseShop() trong NpcShopInteraction để đặt lại trạng thái
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