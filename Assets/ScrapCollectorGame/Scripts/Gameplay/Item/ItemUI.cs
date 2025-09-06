using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    [Header("UI refs (assign in prefab)")]
    public Image icon;
    public TextMeshProUGUI amountText;

    private ItemData itemData;
    private int amount = 1;

    public void Setup(ItemData data, int qty = 1)
    {
        itemData = data;
        amount = Mathf.Max(1, qty);

        if (icon != null && data != null)
            icon.sprite = data.itemIcon;

        UpdateAmountUI();
    }

    public ItemData GetItemData() => itemData;
    public int Amount => amount;

    public void AddAmount(int v)
    {
        amount += v;
        UpdateAmountUI();
    }

    private void UpdateAmountUI()
    {
        if (amountText == null) return;

        if (itemData == null)
        {
            amountText.text = "";
            return;
        }

        if (amount > 1)
        {
            amountText.text = amount.ToString();
        }
        else
        {
            // Non-stackable thì luôn hiện 1, stackable có 1 thì ẩn
            if (itemData.isStackable)
                amountText.text = "";
            else
                amountText.text = "";
        }
    }
}
