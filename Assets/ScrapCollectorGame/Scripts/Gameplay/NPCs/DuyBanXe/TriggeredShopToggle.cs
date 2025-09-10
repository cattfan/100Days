using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class TriggeredShopToggle : MonoBehaviour
{
    [Header("UI Panel bán xe")]
    public GameObject shopPanel;

    [Header("Script điều khiển Player (tắt khi UI bật)")]
    public MonoBehaviour playerMoveScript;

    private bool inZone = false;

    void Start()
    {
        // Đảm bảo panel luôn tắt khi game bắt đầu
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            inZone = true;
            Debug.Log("[ShopToggle] Enter DuyBanXe zone");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            inZone = false;
            // Khi rời vùng, tắt panel nếu đang mở
            if (shopPanel != null && shopPanel.activeSelf)
                shopPanel.SetActive(false);
            // Bật lại điều khiển
            if (playerMoveScript != null && !playerMoveScript.enabled)
                playerMoveScript.enabled = true;

            Debug.Log("[ShopToggle] Exit DuyBanXe zone");
        }
    }

    void Update()
    {
        if (inZone && Keyboard.current?.eKey.wasPressedThisFrame == true)
        {
            if (shopPanel == null)
            {
                Debug.LogWarning("[ShopToggle] shopPanel chưa gán!");
                return;
            }

            bool nowOn = !shopPanel.activeSelf;
            shopPanel.SetActive(nowOn);
            Debug.Log($"[ShopToggle] shopPanel active = {nowOn}");

            // Tắt/bật control của player qua script move
            if (playerMoveScript != null)
            {
                playerMoveScript.enabled = !nowOn;
                Debug.Log($"[ShopToggle] PlayerMoveScript enabled = {playerMoveScript.enabled}");
            }
        }
    }
}
