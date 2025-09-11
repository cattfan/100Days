using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
    public ShopItem itemData;
    public Image itemIconImage;
    public TextMeshProUGUI itemNameText;

    private ShopManager shopManager;

    void Start()
    {
        // Thay thế FindObjectOfType bằng FindAnyObjectByType
        shopManager = FindAnyObjectByType<ShopManager>();
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
                // Chỉ hiển thị tên vật phẩm, loại bỏ giá tiền và dấu gạch ngang
                itemNameText.text = itemData.itemName;
            }
        }
    }

    public void OnItemClick()
    {
        if (itemData != null && shopManager != null)
        {
            shopManager.BuyItem(itemData);
        }
        else
        {
            Debug.LogError("ItemSlotUI: itemData hoặc shopManager bị null!");
        }
    }
}