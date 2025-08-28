using UnityEngine;

[System.Serializable]
public enum ItemRarity
{
    Common,     // Thường - Trắng
    Uncommon,   // Không thường - Xanh lá
    Rare,       // Hiếm - Xanh dương  
    Epic,       // Sử thi - Tím
    Legendary   // Huyền thoại - Cam/Vàng
}

[CreateAssetMenu(fileName = "New ItemData", menuName = "Items/ItemData", order = 1)]
public class ItemData : ScriptableObject
{
    [Header("Item Information")]
    public string itemName = "New Item";
    [TextArea(3, 5)]
    public string description = "Item description";
    public Sprite itemIcon;
    public int itemID;

    [Header("Rarity & Drop System")]
    public ItemRarity rarity = ItemRarity.Common;
    [Range(0f, 100f)]
    public float dropRate = 50f;           // Tỉ lệ drop (%) - 0-100
    [Range(1, 10)]
    public int minDropAmount = 1;          // Số lượng tối thiểu khi drop
    [Range(1, 10)]
    public int maxDropAmount = 1;          // Số lượng tối đa khi drop

    [Header("Shop Properties")]
    public int baseSellPrice = 10;
    public int baseBuyPrice = 5;
    public bool canSell = true;

    [Header("Stack Properties")]
    public bool isStackable = false;
    public int maxStackSize = 1;

    [Header("Pickup Settings")]
    public float pickupDelay = 999f;       // Thời gian delay trước khi có thể pickup (giây)

    // Phương thức tính giá dựa trên rarity
    public int GetAdjustedSellPrice()
    {
        float multiplier = GetRarityPriceMultiplier();
        return Mathf.RoundToInt(baseSellPrice * multiplier);
    }

    public int GetAdjustedBuyPrice()
    {
        float multiplier = GetRarityPriceMultiplier();
        return Mathf.RoundToInt(baseBuyPrice * multiplier);
    }

    private float GetRarityPriceMultiplier()
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                return 1f;
            case ItemRarity.Uncommon:
                return 1.5f;
            case ItemRarity.Rare:
                return 2f;
            case ItemRarity.Epic:
                return 3f;
            case ItemRarity.Legendary:
                return 5f;
            default:
                return 1f;
        }
    }

    // Phương thức kiểm tra có drop hay không
    public bool ShouldDrop()
    {
        float randomValue = Random.Range(0f, 100f);
        return randomValue <= dropRate;
    }

    // Phương thức random số lượng drop
    public int GetRandomDropAmount()
    {
        return Random.Range(minDropAmount, maxDropAmount + 1);
    }

    // Lấy màu theo rarity
    public Color GetRarityColor()
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                return Color.white;
            case ItemRarity.Uncommon:
                return Color.green;
            case ItemRarity.Rare:
                return Color.blue;
            case ItemRarity.Epic:
                return Color.magenta;
            case ItemRarity.Legendary:
                return new Color(1f, 0.5f, 0f); // Orange
            default:
                return Color.white;
        }
    }
}