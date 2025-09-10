using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemUI : MonoBehaviour, IPointerClickHandler
{
    private ItemData itemData;
    private int amount;

    // Sử dụng [SerializeField] để gán trong Unity Inspector.
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI amountText;

    // Getter để các script khác có thể truy cập ItemData
    public ItemData GetItemData()
    {
        return itemData;
    }

    public int Amount
    {
        get { return amount; }
    }

    public void Setup(ItemData data, int count)
    {
        itemData = data;
        amount = count;

        // KIỂM TRA NULL TRƯỚC KHI SỬ DỤNG
        if (itemIcon != null && data.itemIcon != null)
        {
            itemIcon.sprite = data.itemIcon;
            itemIcon.color = data.GetRarityColor();
        }
        else
        {
            Debug.LogError("itemIcon hoặc itemData.itemIcon không được gán trong Inspector!");
        }

        UpdateAmountText();
    }

    public void AddAmount(int value)
    {
        amount += value;
        if (amount < 0)
        {
            amount = 0;
        }

        UpdateAmountText();

        if (amount == 0)
        {
            // Tìm InventoryManager và yêu cầu xóa vật phẩm khỏi slot
            InventoryManager invManager = FindAnyObjectByType<InventoryManager>();
            if (invManager != null)
            {
                // Tìm slot chứa item này và xóa
                // (Lưu ý: Cách này có thể không hiệu quả nếu có nhiều inventory.
                // Một cách khác là truyền trực tiếp slot vào đây.)
                // Để đơn giản, ta chỉ cần Destroy GameObject này.
                // Các InventoryManager tốt hơn sẽ tự cập nhật.
            }

            Destroy(gameObject);
        }
    }

    private void UpdateAmountText()
    {
        // KIỂM TRA NULL TRƯỚC KHI SỬ DỤNG
        if (amountText != null)
        {
            amountText.text = amount > 1 ? amount.ToString() : "";
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            UseItem();
        }
    }

    private void UseItem()
    {
        if (itemData == null)
        {
            Debug.LogError("ItemData is null. Không thể sử dụng vật phẩm.");
            return;
        }

        if (itemData.isFood)
        {
            ThanhTheLucplayer playerStamina = FindAnyObjectByType<ThanhTheLucplayer>();

            if (playerStamina != null)
            {
                playerStamina.AddEnergy(itemData.staminaRestoreAmount);
                Debug.Log($"Đã dùng {itemData.itemName}. Hồi {itemData.staminaRestoreAmount} thể lực.");

                AddAmount(-1);
            }
            else
            {
                Debug.LogWarning("Không tìm thấy ThanhTheLucplayer trong Scene!");
            }
        }
        else
        {
            Debug.Log("Vật phẩm này không thể ăn được.");
        }
    }
}