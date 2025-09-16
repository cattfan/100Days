using UnityEngine;
using UnityEngine.InputSystem;

public class GachaNPC : MonoBehaviour, IInteractable
{
    [Header("NPC Settings")]
    public string npcName = "Gacha Keeper";
    public float interactionRange = 2f;

    [Header("UI References")]
    public GameObject DialogUi;    // UI màn hình Gacha
    public GameObject EnterNumberUI;
    private PlayerInput playerInput;

    private void Start()
    {

        if (DialogUi != null)
            DialogUi.SetActive(false);
        if (EnterNumberUI != null)
            EnterNumberUI.SetActive(false);
        playerInput = FindFirstObjectByType<PlayerInput>();
    }

    public void Interact()
    {

        OpenDialog();
    }

    public bool CanInteract()
    {
        // Chỉ cho phép mở nếu cả 2 UI đang tắt
        bool DialogActive = DialogUi != null && DialogUi.activeSelf;
        return !(DialogActive);
    }

    private void OpenDialog()
    {
        Debug.Log($"OpenGacha called:gachaScreenUI={DialogUi}");

        if (DialogUi != null)
            DialogUi.SetActive(true);
        if (playerInput != null)
            playerInput.enabled = false;
    }

    public void CloseDialog()
    {
        if (DialogUi != null)
            DialogUi.SetActive(false);
        if (playerInput != null)
            playerInput.enabled = true;
        if (playerInput != null)
            playerInput.enabled = true;
    }

    public void OpenEnterNumberUI()
    {
        SaveController saveController = FindFirstObjectByType<SaveController>();
        if (saveController != null && saveController.currencyManager != null)
        {
            int coint = saveController.currencyManager.GetCoins();
            if (coint < 100)
            {
                ItemPickupUIController.Instance.ShowWarningPopup("Bạn cần ít nhất 100 xu để quay Gacha!");
                DialogUi.SetActive(false);
                playerInput.enabled = true;
                return;
            }
            else
            {
                saveController.currencyManager.SpendCoins(100);
            }
        }
        else
        {
            Debug.LogError("Could not find SaveController or its CurrencyManager!");
        }
        if (EnterNumberUI != null)
            DialogUi.SetActive(false);
        EnterNumberUI.SetActive(true);
        if (playerInput != null)
            playerInput.enabled = false;
    }
}