using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemPickupUIController : MonoBehaviour
{
    [Header("Popup Settings")]
    public static ItemPickupUIController Instance { get; private set; }

    public GameObject popupPrefab;
    public int maxPopups = 5;
    public float popupDuration = 3f;

    private readonly Queue<GameObject> activePopups = new Queue<GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[ItemPickupUI] Instance set thành công.");
        }
        else
        {
            Debug.LogError("[ItemPickupUI] Có nhiều hơn 1 instance, sẽ xoá cái thừa.");
            Destroy(gameObject);
        }
    }

    public void ShowItemPickup(string itemName, Sprite itemIcon)
    {
        Debug.Log($"[ItemPickupUI] Gọi ShowItemPickup với itemName = {itemName}, icon = {(itemIcon != null ? itemIcon.name : "NULL")}");

        if (popupPrefab == null)
        {
            Debug.LogError("[ItemPickupUI] popupPrefab chưa được gán trong Inspector!");
            return;
        }

        GameObject newPopup = Instantiate(popupPrefab, transform);
        if (newPopup == null)
        {
            Debug.LogError("[ItemPickupUI] Instantiate popup thất bại!");
            return;
        }

        TMP_Text text = newPopup.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.text = itemName;
            Debug.Log("[ItemPickupUI] Đặt text thành công.");
        }
        else
        {
            Debug.LogError("[ItemPickupUI] Không tìm thấy TMP_Text trong prefab!");
        }

        Transform iconTransform = newPopup.transform.Find("ItemIcon");
        if (iconTransform != null)
        {
            Image itemImage = iconTransform.GetComponent<Image>();
            if (itemImage != null)
            {
                itemImage.sprite = itemIcon;
                Debug.Log("[ItemPickupUI] Đặt sprite thành công.");
            }
            else
            {
                Debug.LogError("[ItemPickupUI] Không tìm thấy Image trên ItemIcon!");
            }
        }
        else
        {
            Debug.LogError("[ItemPickupUI] Không tìm thấy child ItemIcon!");
        }

        activePopups.Enqueue(newPopup);
        Debug.Log($"[ItemPickupUI] Hiện tại có {activePopups.Count} popup đang active.");

        if (activePopups.Count > maxPopups)
        {
            GameObject removed = activePopups.Dequeue();
            Debug.Log("[ItemPickupUI] Xoá popup cũ do vượt quá số lượng.");
            Destroy(removed);
        }

        // Fade out and destroy
        StartCoroutine(FadeOutAndDestroy(newPopup));
    }

    private IEnumerator FadeOutAndDestroy(GameObject popup)
    {
        yield return new WaitForSeconds(popupDuration);
        if (popup == null)
        {
            Debug.LogWarning("[ItemPickupUI] Popup đã bị xoá trước khi fade out.");
            yield break;
        }

        CanvasGroup canvasGroup = popup.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogWarning("[ItemPickupUI] Popup không có CanvasGroup, sẽ xoá ngay.");
            Destroy(popup);
            yield break;
        }

        Debug.Log("[ItemPickupUI] Bắt đầu fade out popup.");
        for (float timePassed = 0f; timePassed < 1f; timePassed += Time.deltaTime)
        {
            if (popup == null) yield break;
            canvasGroup.alpha = 1f - timePassed;
            yield return null;
        }

        Debug.Log("[ItemPickupUI] Xoá popup sau khi fade out.");
        Destroy(popup);
    }
}
