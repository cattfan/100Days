using UnityEngine;

[CreateAssetMenu(fileName = "New Shop Item", menuName = "Shop/Shop Item")]
public class ShopItem : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    public int itemCost;
    public ItemData itemData; // Cần có dòng này
                              // Trong file ItemData.cs

    // Trong file ItemData.cs

    [Header("Consumption Properties")]
    public bool canBeConsumed = false;
    public float staminaRestoreAmount = 0f;
}