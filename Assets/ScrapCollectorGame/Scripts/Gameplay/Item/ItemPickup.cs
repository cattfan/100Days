// ItemPickup.cs: Xử lý logic chính của việc nhặt item
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Data")]
    public ItemData itemData;

    [Header("Current State")]
    public int currentAmount = 1;

    private bool canBePickedUp = false;
    private float spawnTime;

    // Components
    private ItemPickupVisuals visualEffects;
    private ItemPickupAudio audioHandler;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        visualEffects = GetComponent<ItemPickupVisuals>();
        audioHandler = GetComponent<ItemPickupAudio>();

        // Initialize components
        if (visualEffects == null)
            visualEffects = gameObject.AddComponent<ItemPickupVisuals>();
        if (audioHandler == null)
            audioHandler = gameObject.AddComponent<ItemPickupAudio>();
    }

    void Start()
    {
        InitializeItem();
    }

    void Update()
    {
        CheckPickupDelay();
    }

    private void InitializeItem()
    {
        if (itemData == null)
        {
            Debug.LogError($"ItemData is missing on {gameObject.name}!");
            return;
        }

        SetupItemProperties();
        SetupCollider();
        StartPickupDelay();
    }

    private void SetupItemProperties()
    {
        // Setup sprite icon
        if (itemData.itemIcon != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = itemData.itemIcon;
        }

        // Set tag
        if (!gameObject.CompareTag("Item"))
        {
            gameObject.tag = "Item";
        }

        // Set stack amount
        if (itemData.isStackable)
        {
            currentAmount = Mathf.Clamp(currentAmount, 1, itemData.maxStackSize);
        }
        else
        {
            currentAmount = 1;
        }
    }

    private void SetupCollider()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
        }
        col.isTrigger = true;
    }

    private void StartPickupDelay()
    {
        spawnTime = Time.time;
        canBePickedUp = false;
        visualEffects?.StartDelayEffect(itemData.pickupDelay);
    }

    private void CheckPickupDelay()
    {
        if (itemData != null && !canBePickedUp && Time.time >= spawnTime + itemData.pickupDelay)
        {
            EnablePickup();
        }
    }

    private void EnablePickup()
    {
        canBePickedUp = true;
        visualEffects?.ShowReadyEffect();
        Debug.Log($"{itemData?.itemName ?? "Unknown Item"} is now ready to be picked up!");
    }

    public void EnablePickupNow()
    {
        canBePickedUp = true;
        visualEffects?.StopAllEffects();
        Debug.Log($"{itemData?.itemName ?? "Unknown Item"} can now be picked up immediately!");
    }

    public bool CanBePickedUp()
    {
        return canBePickedUp;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && canBePickedUp)
        {
            Pickup(other.gameObject);
        }
        else if (other.CompareTag("Player") && !canBePickedUp)
        {
            Debug.Log($"Item {itemData?.itemName ?? "Unknown"} is not ready to be picked up yet!");
        }
    }

    private void Pickup(GameObject player)
    {
        if (itemData == null) return;

        Debug.Log($"Player đã pickup {itemData.itemName} x{currentAmount}!");

        // 🎵 Play pickup sound trước
       

        // ✅ Thử thêm trực tiếp vào inventory - KHÔNG kiểm tra trước
        InventoryManager inv = Object.FindFirstObjectByType<InventoryManager>();
        if (inv != null)
        {
            bool added = inv.AddItem(itemData, currentAmount);

            if (added)
            {
                // 🟢 Nếu add thành công thì Destroy
                Debug.Log($"✅ Successfully added {itemData.itemName} to inventory!");
                audioHandler?.PlayPickupSound();
                Destroy(gameObject);
            }
            else
            {
                // ❌ Nếu add thất bại
                Debug.Log("❌ Failed to add item to inventory!");
                audioHandler?.PlayCannotPickupSound();
                // KHÔNG destroy item
            }
        }
        else
        {
            Debug.LogWarning("⚠️ InventoryManager not found!");
        }
    }


    public string GetItemInfo()
    {
        if (itemData == null) return "Unknown Item";

        string info = $"{itemData.itemName}";
        if (itemData.isStackable && currentAmount > 1)
        {
            info += $" x{currentAmount}";
        }
        return info;
    }

    public int GetSellPrice()
    {
        if (itemData == null) return 0;
        return itemData.GetAdjustedSellPrice() * currentAmount;
    }

    public int GetBuyPrice()
    {
        if (itemData == null) return 0;
        return itemData.GetAdjustedBuyPrice() * currentAmount;
    }
}