using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Trashbin : MonoBehaviour, IInteractable
{
    public bool isChecked { get; private set; }
    public string TrashbinName { get; private set; }
    public GameObject FailInteractIcon;

    [Header("Stamina Settings")]
    public float staminaCost = 10f;
    private PlayerStamina playerStamina; // 👉 đổi sang PlayerStamina

    [Header("Item System")]
    public ItemData[] itemDataList;
    public GameObject itemPickupPrefab;

    [Header("Spawn Settings")]
    public float spawnChance = 0.999f;
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

        // 🔥 Lấy stamina player
        playerStamina = FindFirstObjectByType<PlayerStamina>();

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
            playerStamina.ReduceStamina(staminaCost);
        }

        CheckTrashbin();
    }

    public bool CanInteract()
    {
        if (isChecked) return false;

        if (playerStamina != null && playerStamina.currentStamina < staminaCost)
        {
            Debug.Log("Không đủ thể lực để nhặt trashbin!");
            return false;
        }

        return true;
    }

    private void CheckTrashbin()
    {
        SetChecked(true);
        StartResetTimer();

        float randomValue = Random.Range(0f, 1f);

        if (randomValue <= spawnChance)
        {
<<<<<<< HEAD
=======
            Debug.Log($"[SUCCESS by chance] - Spawning items! Random={randomValue}, SpawnChance={spawnChance}");
>>>>>>> origin/main
            if (audioManagement != null)
                audioManagement.PlaySFX(audioManagement.SuccessTrashbinInteract);
            SpawnRandomItems();
        }
        else
        {
<<<<<<< HEAD
=======
            Debug.Log($"[FAIL by chance] - No items found! Random={randomValue}, SpawnChance={spawnChance}");
>>>>>>> origin/main
            if (audioManagement != null)
                audioManagement.PlaySFX(audioManagement.FailTrashbinInteract);
            ShowFailIcon();
        }
    }

    private void ShowFailIcon()
    {
        if (FailInteractIcon != null)
        {
            FailInteractIcon.SetActive(true);
            StartCoroutine(HideFailIconAfterDelay(1f));
        }
    }

    private IEnumerator HideFailIconAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        FailInteractIcon.SetActive(false);
    }

    private void SpawnRandomItems()
    {
        if (itemDataList != null && itemDataList.Length > 0)
        {
            SpawnItemsWithItemData();
        }
        else
        {
            ShowFailIcon();
            return;
        }
    }

    private void SpawnItemsWithItemData()
    {
<<<<<<< HEAD
        int itemCount = Random.Range(minItems, maxItems + 1);
=======
        // Trộn (shuffle) danh sách itemData để đảm bảo ngẫu nhiên và không trùng
        List<ItemData> shuffledList = itemDataList.OrderBy(x => Random.value).ToList();

        // Random số lượng item sẽ spawn, giới hạn tối đa bằng số item có trong list
        int itemCount = Random.Range(minItems, Mathf.Min(maxItems + 1, shuffledList.Count));
>>>>>>> origin/main
        int actualSpawnedCount = 0;

        for (int i = 0; i < itemCount; i++)
        {
<<<<<<< HEAD
            int randomIndex = Random.Range(0, itemDataList.Length);
            ItemData selectedItemData = itemDataList[randomIndex];

            if (selectedItemData != null)
            {
                GameObject droppedItem = ItemPickup.CreateDrop(selectedItemData, transform.position, itemPickupPrefab);
=======
            ItemData selectedItemData = shuffledList[i];

            if (selectedItemData != null)
            {
                // Tạo item sử dụng ItemPickup system
                GameObject droppedItem = ItemDropFactory.CreateDrop(selectedItemData, transform.position, itemPickupPrefab);
>>>>>>> origin/main

                if (droppedItem != null)
                {
                    actualSpawnedCount++;
                    Vector3 randomOffset = Random.insideUnitCircle * spawnRadius;
                    Vector3 targetPosition = transform.position + spawnOffset + new Vector3(randomOffset.x, randomOffset.y, 0);

                    StartCoroutine(ItemFlyOutAnimation(droppedItem, targetPosition, actualSpawnedCount * 0.1f));
                }
<<<<<<< HEAD
=======
                else
                {
                    Debug.Log("[FAIL by item spawn] No items were actually spawned - showing fail icon");
                }
            }
            else
            {
                Debug.LogWarning($"ItemData at index {i} is null!");
>>>>>>> origin/main
            }
        }

        if (actualSpawnedCount == 0)
        {
            ShowFailIcon();
        }
    }

<<<<<<< HEAD
=======

    // Animation item bay ra từ trashbin với hiệu ứng đẹp hơn
>>>>>>> origin/main
    private IEnumerator ItemFlyOutAnimation(GameObject item, Vector3 targetPosition, float delay = 0f)
    {
        if (item == null) yield break;

        yield return new WaitForSeconds(delay);
        if (item == null) yield break;

        Vector3 startPosition = transform.position;
        Vector3 originalScale = item.transform.localScale;

        item.transform.localScale = Vector3.zero;

        float flyTime = 0.5f;
        float elapsedTime = 0f;

        Vector3 midPoint = Vector3.Lerp(startPosition, targetPosition, 0.5f) + Vector3.up * 2f;

        while (elapsedTime < flyTime)
        {
            if (item == null) yield break;

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

        if (item != null)
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
            ItemPickup itemPickup = item.GetComponent<ItemPickup>();
            if (itemPickup != null)
            {
                itemPickup.EnablePickupNow();
            }
        }
    }

    private Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1f - t;
        return u * u * p0 + 2f * u * t * p1 + t * t * p2;
    }

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
            }
            else if (!isChecked && UncheckedBin != null)
            {
                spriteRenderer.sprite = UncheckedBin;
            }
        }
    }

    private void StartResetTimer()
    {
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
        }

        resetCoroutine = StartCoroutine(ResetTrashbinTimer());
    }

    private IEnumerator ResetTrashbinTimer()
    {
        float timeRemaining = resetTime;

        while (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            yield return null;
        }

        ResetTrashbin();
    }

    private void ResetTrashbin()
    {
        SetChecked(false);
        resetCoroutine = null;
    }

    public void ForceReset()
    {
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
        }
        ResetTrashbin();
    }

    public float GetTimeUntilReset()
    {
        if (resetCoroutine == null || !isChecked)
            return 0f;

        return resetTime;
    }
}
