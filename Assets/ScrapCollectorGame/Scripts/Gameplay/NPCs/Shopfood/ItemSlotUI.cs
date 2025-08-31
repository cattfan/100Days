using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
    public ShopItem itemData;
    public Image itemIconImage;
    public TextMeshProUGUI itemNameText;

    private PlayerInventory playerInventory;

    void Start()
    {
        // Thay thế FindObjectOfType bằng FindAnyObjectByType
        playerInventory = FindAnyObjectByType<PlayerInventory>();
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (itemData != null)
        {
            if (itemIconImage != null && itemData.itemIcon != null)
            {
                itemIconImage.sprite = itemData.itemIcon;
            }
            if (itemNameText != null)
            {
                itemNameText.text = $"{itemData.itemName} - {itemData.itemCost}$";
            }
        }
    }

    // HÀM NÀY SẼ ĐƯỢC GỌI KHI NGƯỜI CHƠI NHẤN VÀO Ô VẬT PHẨM
    public void OnItemClick()
    {
        if (itemData != null && playerInventory != null)
        {
            playerInventory.BuyItem(itemData);
        }
    }
}