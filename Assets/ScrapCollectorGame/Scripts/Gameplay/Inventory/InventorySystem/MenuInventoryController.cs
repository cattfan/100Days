using UnityEngine;
using UnityEngine.InputSystem;

public class MenuInventoryController : MonoBehaviour
{
    public GameObject MenuCanvas;
    public TabsController tabsController; // tham chiếu tới TabsController
    public InventoryController inventoryController; // Thêm reference tới InventoryController

    private bool isInventoryOpen = false;
    private AudioManagement audioManagement;

    void Start()
    {
        if (MenuCanvas != null)
            MenuCanvas.SetActive(false);
    }

    private void Awake()
    {
        GameObject audioObject = GameObject.FindGameObjectWithTag("Audio");
        if (audioObject != null)
        {
            audioManagement = audioObject.GetComponent<AudioManagement>();
        }
    }

    public void OnOpenMenu(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ToggleInventory();
        }
    }

    // ========== LOGIC ==========
    private void ToggleInventory()
    {
        if (MenuCanvas == null) return;

        // Nếu đang mở và có item đang kéo, không cho phép đóng
        if (isInventoryOpen && inventoryController != null && inventoryController.HasCursorItem())
        {
            Debug.Log("Cannot close inventory while dragging an item!");
            return; // Ngăn không cho đóng inventory
        }

        isInventoryOpen = !isInventoryOpen;
        MenuCanvas.SetActive(isInventoryOpen);

        if (isInventoryOpen && tabsController != null)
        {
            // Luôn mở Inventory tab đầu tiên
            tabsController.OpenInventoryTab();
        }

        // Play sound
        if (audioManagement != null)
        {
            audioManagement.PlaySFX(audioManagement.OpenMenu);
        }
    }

    public void CloseInventory()
    {
        if (MenuCanvas == null) return;

        // Kiểm tra nếu có item đang được kéo - trả về vị trí cũ ngay lập tức
        if (inventoryController != null && inventoryController.HasCursorItem())
        {
            inventoryController.ReturnCursorItemToSlot();
        }

        isInventoryOpen = false;
        MenuCanvas.SetActive(false);

        if (tabsController != null)
        {
            // Đặt toàn bộ tab về inactive khi thoát
            tabsController.CloseTab();
        }

        // Play sound
        if (audioManagement != null)
        {
            audioManagement.PlaySFX(audioManagement.CloseMenu);
        }
    }
}