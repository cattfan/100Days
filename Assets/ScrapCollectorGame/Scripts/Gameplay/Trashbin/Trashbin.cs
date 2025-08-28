using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Trashbin : MonoBehaviour, IInteractable
{
    public bool isChecked { get; private set; }
    public string TrashbinName { get; private set; }
    public GameObject FailInteractIcon;

    [Header("Stamina Settings")]
    public float staminaCost = 10f;
    private ThanhTheLucplayer playerStamina; // tham chiếu player

    [Header("Item System")]
    public ItemData[] itemDataList;
    public GameObject itemPickupPrefab;

    [Header("Spawn Settings")]
    public float spawnChance = 0.8f;
    public int minItems = 1;
    public int maxItems = 3;
    public float spawnRadius = 1.5f;
    public Vector3 spawnOffset = Vector3.down;

    [Header("Visual Settings")]
    public Sprite CheckedBin;
    public Sprite UncheckedBin;

    [Header("Reset Settings")]
    public float resetTime = 60f;
    public bool showResetTimer = true;

    private Sprite originalSprite;
    private Coroutine resetCoroutine;

    [Header("Music")]
    public AudioManagement audioManagement;

    private void Awake()
    {
        audioManagement = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManagement>();
    }

    void Start()
    {
        TrashbinName ??= Global_Helper.GenerateUniqueID(gameObject);

        FailInteractIcon.SetActive(false);

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalSprite = spriteRenderer.sprite;
        }
        if (UncheckedBin == null)
        {
            UncheckedBin = originalSprite;
        }

        // 🔥 Lấy stamina player 1 lần duy nhất
        playerStamina = FindFirstObjectByType<ThanhTheLucplayer>();

        ValidateItemSetup();
    }

    private void ValidateItemSetup()
    {
        if (itemDataList == null || itemDataList.Length == 0)
        {
            Debug.LogError($"Trashbin '{TrashbinName}': No ItemData configured! Please assign ItemData assets to itemDataList.");
        }
    }

    public void Interact()
    {
        if (!CanInteract()) return;

        // Trừ thể lực
        if (playerStamina != null)
        {
            playerStamina.TruTheLuc(staminaCost);
        }

        CheckTrashbin();
    }

    public bool CanInteract()
    {
        if (isChecked) return false;

        if (playerStamina != null && playerStamina.luongtheluchientai < staminaCost)
        {
            Debug.Log("Không đủ thể lực để nhặt trashbin!");
            return false;
        }

        return true;
    }
    private void CheckTrashbin()
    {
        SetChecked(true);// Set the trashbin as checked

        // Bắt đầu countdown để reset
        StartResetTimer();

        // Random chance to spawn items
        float randomValue = Random.Range(0f, 1f);
        Debug.Log($"Random value: {randomValue}, Spawn chance: {spawnChance}");

        if (randomValue <= spawnChance)
        {
            Debug.Log("SUCCESS - Spawning items!");
            if (audioManagement != null)
                audioManagement.PlaySFX(audioManagement.SuccessTrashbinInteract);
            SpawnRandomItems();
        }
        else
        {
            Debug.Log("FAIL - No items found!");
            if (audioManagement != null)
                audioManagement.PlaySFX(audioManagement.FailTrashbinInteract);
            ShowFailIcon();
        }
    }

    private void ShowFailIcon()
    {
        Debug.Log("Showing fail icon...");
        if (FailInteractIcon != null)
        {
            FailInteractIcon.SetActive(true);
            Debug.Log($"FailInteractIcon active state: {FailInteractIcon.activeInHierarchy}");
            StartCoroutine(HideFailIconAfterDelay(1f));
        }
        else
        {
            Debug.LogError("FailInteractIcon is null!");
        }
    }

    private IEnumerator HideFailIconAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        FailInteractIcon.SetActive(false);
    }

    private void SpawnRandomItems()
    {
        // Sử dụng ItemData system
        if (itemDataList != null && itemDataList.Length > 0)
        {
            SpawnItemsWithItemData();
        }
        else
        {
            Debug.LogWarning("No items to spawn from trashbin!");
            ShowFailIcon();
            return;
        }
    }

    // Spawn items sử dụng ItemData system
    private void SpawnItemsWithItemData()
    {
        // Random số lượng item sẽ spawn
        int itemCount = Random.Range(minItems, maxItems + 1);
        int actualSpawnedCount = 0;

        for (int i = 0; i < itemCount; i++)
        {
            // Random chọn 1 ItemData
            int randomIndex = Random.Range(0, itemDataList.Length);
            ItemData selectedItemData = itemDataList[randomIndex];

            if (selectedItemData != null)
            {
                // Tạo item sử dụng ItemPickup system
                GameObject droppedItem = ItemPickup.CreateDrop(selectedItemData, transform.position, itemPickupPrefab);

                if (droppedItem != null)
                {
                    actualSpawnedCount++;

                    // Random vị trí đích để item bay tới
                    Vector3 randomOffset = Random.insideUnitCircle * spawnRadius;
                    Vector3 targetPosition = transform.position + spawnOffset + new Vector3(randomOffset.x, randomOffset.y, 0);

                    // Bắt đầu animation bay ra với delay nhỏ cho mỗi item
                    StartCoroutine(ItemFlyOutAnimation(droppedItem, targetPosition, actualSpawnedCount * 0.1f));

                    Debug.Log($"Spawned {selectedItemData.itemName} from trashbin!");
                }
                else
                {
                    Debug.LogWarning($"Failed to create item: {selectedItemData.itemName}");
                }
            }
            else
            {
                Debug.LogWarning($"ItemData at index {randomIndex} is null!");
            }
        }

        // Nếu không spawn được item nào thì hiển thị fail icon
        if (actualSpawnedCount == 0)
        {
            Debug.Log("No items were actually spawned - showing fail icon");
            ShowFailIcon();
        }
        else
        {
            Debug.Log($"Successfully spawned {actualSpawnedCount} items!");
        }
    }

    // Animation item bay ra từ trashbin với hiệu ứng đẹp hơn
    private IEnumerator ItemFlyOutAnimation(GameObject item, Vector3 targetPosition, float delay = 0f)
    {
        if (item == null) yield break;

        yield return new WaitForSeconds(delay);

        if (item == null) yield break; // check sau delay

        Vector3 startPosition = transform.position;
        Vector3 originalScale = item.transform.localScale;

        item.transform.localScale = Vector3.zero;

        float flyTime = 0.5f;
        float elapsedTime = 0f;

        Vector3 midPoint = Vector3.Lerp(startPosition, targetPosition, 0.5f) + Vector3.up * 2f;

        while (elapsedTime < flyTime)
        {
            if (item == null) yield break; // <- quan trọng

            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / flyTime;
            float easedProgress = EaseOutQuad(progress);

            Vector3 currentPos = QuadraticBezier(startPosition, midPoint, targetPosition, easedProgress);
            item.transform.position = currentPos;

            float scaleProgress = Mathf.Clamp01(progress / 0.3f);
            item.transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, EaseOutBack(scaleProgress));

            item.transform.Rotate(0, 0, Time.deltaTime * 180f);

            yield return null;
        }

        if (item != null) // check trước khi gọi bounce
        {
            item.transform.position = targetPosition;
            item.transform.localScale = originalScale;
            StartCoroutine(ItemLandingBounce(item));
        }
    }

    private IEnumerator ItemLandingBounce(GameObject item)
    {
        if (item == null) yield break;

        Vector3 originalScale = item.transform.localScale;
        float bounceTime = 0.2f;
        float elapsedTime = 0f;

        while (elapsedTime < bounceTime)
        {
            if (item == null) yield break;

            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / bounceTime;

            float bounceScale = 1f + Mathf.Sin(progress * Mathf.PI) * 0.2f;
            item.transform.localScale = originalScale * bounceScale;

            yield return null;
        }

        if (item != null)
        {
            item.transform.localScale = originalScale;

            // Bật pickup sau khi bounce xong
            ItemPickup itemPickup = item.GetComponent<ItemPickup>();
            if (itemPickup != null)
            {
                itemPickup.EnablePickupNow();
            }
        }
    }

    // Hàm tính đường cong Bezier bậc 2 (3 điểm)
    private Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1f - t;
        return u * u * p0 + 2f * u * t * p1 + t * t * p2;
    }

    // Easing functions để animation mượt hơn
    private float EaseOutQuad(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private void SetChecked(bool value)
    {
        isChecked = value;
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            if (isChecked && CheckedBin != null)
            {
                spriteRenderer.sprite = CheckedBin;
                Debug.Log("Trashbin checked - sprite changed!");
            }
            else if (!isChecked && UncheckedBin != null)
            {
                spriteRenderer.sprite = UncheckedBin;
                Debug.Log("Trashbin reset - sprite restored!");
            }
        }
    }

    // Bắt đầu timer reset
    private void StartResetTimer()
    {
        // Dừng timer cũ nếu có
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
        }

        // Bắt đầu timer mới
        resetCoroutine = StartCoroutine(ResetTrashbinTimer());
    }

    // Coroutine đếm ngược và reset trashbin
    private IEnumerator ResetTrashbinTimer()
    {
        float timeRemaining = resetTime;

        while (timeRemaining > 0)
        {
            // Hiển thị timer trong console (optional)
            if (showResetTimer && Mathf.FloorToInt(timeRemaining) % 10 == 0 && timeRemaining == Mathf.Floor(timeRemaining))
            {
                Debug.Log($"Trashbin '{TrashbinName}' will reset in {Mathf.FloorToInt(timeRemaining)} seconds");
            }

            timeRemaining -= Time.deltaTime;
            yield return null;
        }

        // Reset trashbin về trạng thái ban đầu
        ResetTrashbin();
    }

    // Reset trashbin về trạng thái có thể tương tác
    private void ResetTrashbin()
    {
        SetChecked(false);
        resetCoroutine = null;
        Debug.Log($"Trashbin '{TrashbinName}' has been reset and is ready to be searched again!");
    }

    // Phương thức public để reset manual (có thể gọi từ code khác)
    public void ForceReset()
    {
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
        }
        ResetTrashbin();
    }

    // Lấy thời gian còn lại để reset
    public float GetTimeUntilReset()
    {
        if (resetCoroutine == null || !isChecked)
            return 0f;

        // Tính thời gian còn lại (chỉ estimate)
        return resetTime; // Có thể cải thiện để tính chính xác hơn
    }
}