// ItemPickup.cs: Xử lý việc nhặt vật phẩm trên thế giới
using System.Collections;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Data")]
    public ItemData itemData;

    [Header("Current State")]
    public int currentAmount = 1;

    private bool canBePickedUp = false;      // Flag kiểm tra có thể pickup không
    private float spawnTime;                 // Thời gian item được spawn

    // Public method để kiểm tra có thể pickup không
    public bool CanBePickedUp()
    {
        return canBePickedUp;
    }

    // Public method để enable pickup ngay lập tức (dùng cho trashbin)
    public void EnablePickupNow()
    {
        canBePickedUp = true;

        // Stop pickup delay effect nếu đang chạy
        StopAllCoroutines();

        // Khôi phục màu bình thường
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        }

        Debug.Log($"{itemData?.itemName ?? "Unknown Item"} can now be picked up immediately!");
    }
    private SpriteRenderer spriteRenderer;

    [Header("Audio")]
    public AudioManagement audioManagement;

    private void Awake()
    {
        GameObject audioObject = GameObject.FindGameObjectWithTag("Audio");
        if (audioObject != null)
        {
            audioManagement = audioObject.GetComponent<AudioManagement>();
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (itemData == null)
        {
            Debug.LogError($"ItemData is missing on {gameObject.name}!");
            return;
        }

        // Setup sprite icon
        if (itemData.itemIcon != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = itemData.itemIcon;
        }

        // Lưu thời gian spawn
        spawnTime = Time.time;
        canBePickedUp = false;

        // Đảm bảo object có tag "Item"
        if (!gameObject.CompareTag("Item"))
        {
            gameObject.tag = "Item";
        }

        // Đảm bảo có Collider2D và set là Trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
        }
        col.isTrigger = true;

        // Set stack amount
        if (itemData.isStackable)
        {
            currentAmount = Mathf.Clamp(currentAmount, 1, itemData.maxStackSize);
        }
        else
        {
            currentAmount = 1;
        }

        // Bắt đầu hiệu ứng delay
        StartPickupDelayEffect();
    }

    void Update()
    {
        // Kiểm tra thời gian delay
        if (itemData != null && !canBePickedUp && Time.time >= spawnTime + itemData.pickupDelay)
        {
            EnablePickup();
        }
    }

    // Tạo item drop từ ItemData
    public static GameObject CreateDrop(ItemData data, Vector3 position, GameObject itemPrefab = null)
    {
        if (!data.ShouldDrop())
        {
            return null; // Không drop
        }

        GameObject droppedItem;

        if (itemPrefab != null)
        {
            droppedItem = Instantiate(itemPrefab, position, Quaternion.identity);
        }
        else
        {
            // Tạo GameObject mới với các component cần thiết
            droppedItem = new GameObject($"Item_{data.itemName}");
            droppedItem.transform.position = position;
            droppedItem.AddComponent<SpriteRenderer>();
            droppedItem.AddComponent<CircleCollider2D>();
            droppedItem.AddComponent<ItemPickup>();
        }

        ItemPickup itemComponent = droppedItem.GetComponent<ItemPickup>();
        if (itemComponent != null)
        {
            itemComponent.itemData = data;

            if (data.isStackable)
            {
                itemComponent.currentAmount = data.GetRandomDropAmount();
            }
            else
            {
                itemComponent.currentAmount = 1;
            }

            Debug.Log($"Đã drop {data.itemName} x{itemComponent.currentAmount}");
        }

        return droppedItem;
    }

    // Bắt đầu hiệu ứng delay (item nhấp nháy hoặc có màu khác)
    private void StartPickupDelayEffect()
    {
        if (itemData.pickupDelay > 0)
        {
            StartCoroutine(PickupDelayEffect());
        }
        else
        {
            EnablePickup();
        }
    }

    // Hiệu ứng visual trong thời gian delay
    private IEnumerator PickupDelayEffect()
    {
        if (spriteRenderer == null || itemData == null) yield break;

        Color originalColor = spriteRenderer.color;
        Color delayColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f); // Trong suốt hơn

        float elapsed = 0f;

        while (elapsed < itemData.pickupDelay)
        {
            // Hiệu ứng nhấp nháy
            float alpha = 0.3f + 0.7f * (0.5f + 0.5f * Mathf.Sin(elapsed * 8f)); // Nhấp nháy từ 0.3 đến 1.0
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Khôi phục màu gốc khi hết delay
        spriteRenderer.color = originalColor;
    }

    // Kích hoạt khả năng pickup
    private void EnablePickup()
    {
        canBePickedUp = true;

        // Hiệu ứng khi item sẵn sàng pickup (optional)
        StartCoroutine(ReadyToPickupEffect());

        Debug.Log($"{itemData?.itemName ?? "Unknown Item"} is now ready to be picked up!");
    }

    // Hiệu ứng nhỏ khi item sẵn sàng pickup
    private IEnumerator ReadyToPickupEffect()
    {
        if (spriteRenderer == null) yield break;

        // Hiệu ứng sáng lên một chút
        Color originalColor = spriteRenderer.color;
        Color brightColor = originalColor * 1.3f;
        brightColor.a = originalColor.a;

        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            spriteRenderer.color = Color.Lerp(brightColor, originalColor, progress);
            yield return null;
        }

        spriteRenderer.color = originalColor;
    }

    // Kiểm tra va chạm với player
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && canBePickedUp)
        {
            Pickup(other.gameObject);
        }
        else if (other.CompareTag("Player") && !canBePickedUp)
        {
            Debug.Log($"Item {itemData?.itemName ?? "Unknown"} is not ready to be picked up yet!");
            // Có thể thêm hiệu ứng visual hoặc sound để báo hiệu item chưa sẵn sàng
            ShowNotReadyEffect();
        }
    }

    // Hiệu ứng khi player cố pickup item chưa sẵn sàng
    private void ShowNotReadyEffect()
    {
        // Có thể thêm hiệu ứng rung hoặc đổi màu tạm thời
        StartCoroutine(NotReadyShakeEffect());
    }

    private IEnumerator NotReadyShakeEffect()
    {
        Vector3 originalPosition = transform.position;
        float shakeTime = 0.2f;
        float elapsed = 0f;

        while (elapsed < shakeTime)
        {
            Vector3 randomOffset = Random.insideUnitCircle * 0.1f;
            transform.position = originalPosition + randomOffset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
    }

    // Phương thức pickup item
    private void Pickup(GameObject player)
    {
        if (itemData == null) return;

        Debug.Log($"Player đã pickup {itemData.itemName} x{currentAmount}!");

        // Hiển thị popup UI
        if (ItemPickupUIController.Instance != null)
        {
            ItemPickupUIController.Instance.ShowItemPickup(itemData.itemName, itemData.itemIcon);
        }

        // Play pickup sound
        if (audioManagement != null)
        {
            audioManagement.PlaySFX(audioManagement.PickupItem);
        }

        // ✅ Thêm vào inventory
        InventoryController inv = FindObjectOfType<InventoryController>();
        if (inv != null)
        {
            inv.AddItem(itemData, currentAmount);
        }
        else
        {
            Debug.LogWarning("InventoryController not found!");
        }

        // Xóa object item ngoài world
        Destroy(gameObject);
    }


    // Phương thức set item data (để dùng khi spawn item)
    public void SetItemData(ItemData data, int amount = 1)
    {
        itemData = data;
        currentAmount = amount;

        if (spriteRenderer != null && data.itemIcon != null)
        {
            spriteRenderer.sprite = data.itemIcon;
        }

        // Clamp amount
        if (data.isStackable)
        {
            currentAmount = Mathf.Clamp(currentAmount, 1, data.maxStackSize);
        }
        else
        {
            currentAmount = 1;
        }

        // Update pickup delay từ ItemData
        spawnTime = Time.time;
        canBePickedUp = false;

        if (data.pickupDelay > 0)
        {
            StartPickupDelayEffect();
        }
        else
        {
            EnablePickup();
        }
    }

    // Get thông tin item
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
