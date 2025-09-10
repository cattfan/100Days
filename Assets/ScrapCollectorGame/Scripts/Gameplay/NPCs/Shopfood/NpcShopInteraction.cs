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
        playerInput = FindAnyObjectByType<PlayerInput>();

        if (shopCanvas != null)
            shopCanvas.SetActive(false);
    }

    // Hàm gọi khi player bấm phím (qua NPCDetector)
    public void Interact()
    {
        if (!isShopOpen)
            OpenShop();
        else
            CloseShop();
    }

    // ✅ Luôn cho phép tương tác (dù shop mở hay tắt)
    public bool CanInteract()
    {
        return shopCanvas != null;
    }

    private void OpenShop()
    {
        if (shopCanvas != null)
        {
            shopCanvas.SetActive(true);
            isShopOpen = true;
        }
    }


    public void CloseShop()
    {
        if (shopCanvas != null)
        {
            shopCanvas.SetActive(false);
            isShopOpen = false;
        }
    }
}