using UnityEngine;
using UnityEngine.InputSystem;

public class MenuInventoryController : MonoBehaviour
{
    public GameObject MenuCanvas;
    public TabsController tabsController;
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

    private void ToggleInventory()
    {
        if (MenuCanvas == null) return;

        isInventoryOpen = !isInventoryOpen;
        MenuCanvas.SetActive(isInventoryOpen);

        if (isInventoryOpen && tabsController != null)
        {
            tabsController.OpenInventoryTab();
        }
        if (audioManagement != null)
        {
            audioManagement.PlaySFX(audioManagement.OpenMenu);
        }

    }

    public void CloseInventory()
    {
        if (MenuCanvas == null) return;

        isInventoryOpen = false;
        MenuCanvas.SetActive(false);

        if (tabsController != null)
        {
            tabsController.CloseTab();
        }
        if (audioManagement != null)
        {
            audioManagement.PlaySFX(audioManagement.CloseMenu);
        }
    }
}