using UnityEngine;
using UnityEngine.InputSystem;

public class NpcShopInteraction : MonoBehaviour, IInteractable
{
    private bool isShopOpen = false;

    [Header("UI")]
    public GameObject shopCanvas;

    private PlayerInput playerInput;

    private void Awake()
    {
        // Tìm PlayerInput trong scene.
        // Lưu ý: FindAnyObjectByType() có thể tốn tài nguyên, nên chỉ dùng trong Awake hoặc Start.
        playerInput = FindAnyObjectByType<PlayerInput>();

        // Đảm bảo giao diện shop ban đầu được tắt.
        if (shopCanvas != null)
            shopCanvas.SetActive(false);
    }

    // Hàm gọi khi người chơi bấm phím (qua NPCDetector)
    public void Interact()
    {
        if (!isShopOpen)
        {
            OpenShop();
        }
        else
        {
            // Nếu shop đã mở, không làm gì cả.
            // Việc đóng shop sẽ do một nút trên UI hoặc một phím khác đảm nhiệm.
            Debug.Log("Shop is already open.");
        }
    }

    // Luôn cho phép tương tác nếu có giao diện shop.
    public bool CanInteract()
    {
        return shopCanvas != null;
    }

    // Mở giao diện shop và cập nhật trạng thái.
    private void OpenShop()
    {
        if (shopCanvas != null)
        {
            shopCanvas.SetActive(true);
            isShopOpen = true;
        }
    }

    // Bổ sung phương thức này để đóng shop và cập nhật trạng thái.
    // Phương thức này sẽ được gọi từ nút "Đóng" trên giao diện shop.
    public void CloseShop()
    {
        if (shopCanvas != null)
        {
            shopCanvas.SetActive(false);
            isShopOpen = false;
        }
    }
}