using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemUI : MonoBehaviour, IPointerClickHandler
{
    // Cần khai báo các biến này ở đây để chúng có thể được truy cập bởi tất cả các phương thức.
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

    // Getter cho biến amount
    public int Amount
    {
        get { return amount; }
    }

    public void Setup(ItemData data, int count)
    {
        itemData = data;
        amount = count;

        if (itemIcon != null && data.itemIcon != null)
        {
            itemIcon.sprite = data.itemIcon;
            itemIcon.color = Color.white;
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
            Destroy(gameObject);
        }
    }

    private void UpdateAmountText()
    {
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
            Debug.LogError("ItemData is null. Cannot use item.");
            return;
        }

        if (itemData.isFood)
        {
            // Fix lỗi bằng cách sử dụng FindAnyObjectByType
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