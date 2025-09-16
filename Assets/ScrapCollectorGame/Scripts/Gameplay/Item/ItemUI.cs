using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemUI : MonoBehaviour, IPointerClickHandler
{
    private ItemData itemData;
    private int amount;

    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI amountText;

    public ItemData GetItemData() => itemData;

    public int Amount => amount;

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
        if (amount < 0) amount = 0;
        UpdateAmountText();

        if (amount == 0) Destroy(gameObject);
    }

    private void UpdateAmountText()
    {
        if (amountText != null)
            amountText.text = amount > 1 ? amount.ToString() : "";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
            UseItem();
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
            // ✅ Sử dụng TryEat để kiểm tra hồi chiêu & đầy thể lực
            ThanhTheLucplayer playerStamina = FindAnyObjectByType<ThanhTheLucplayer>();

            if (playerStamina != null)
            {
                bool eaten = playerStamina.TryEat(itemData.staminaRestoreAmount);
                if (eaten)
                {
                    Debug.Log($"Đã dùng {itemData.itemName}. Hồi {itemData.staminaRestoreAmount} thể lực.");
                    AddAmount(-1);
                }
                // Nếu không ăn được, TryEat đã hiển thị popup cảnh báo.
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
