using UnityEngine;
using System.Collections;
using TMPro; // Make sure to include this

public class ShopNPC : MonoBehaviour, IInteractable
{
    [Header("NPC Settings")]
    public string npcName = "Shop Keeper";
    public float interactionRange = 2f;

    [Header("UI References")]
    public GameObject shopUI;
    public ShopUIManager shopUIManager;

    [Header("Audio")]
    public AudioManagement audioManagement;

    // Add these lines to handle player detection directly in this script
    private GameObject playerObject;
    private InventoryController playerInventory;
    private CurrencyManager playerCurrency;


    private void Start()
    {
        if (shopUI != null)
            shopUI.SetActive(false);

        if (shopUIManager == null && shopUI != null)
            shopUIManager = shopUI.GetComponent<ShopUIManager>();

        // Lấy từ GameController thay vì Player
        GameObject gameController = GameObject.Find("GameController");
        if (gameController != null)
        {
            playerInventory = gameController.GetComponent<InventoryController>();
            playerCurrency = gameController.GetComponent<CurrencyManager>();
        }

        if (playerInventory == null)
            Debug.LogWarning("ShopNPC: Could not find InventoryController!");
        if (playerCurrency == null)
            Debug.LogWarning("ShopNPC: Could not find CurrencyManager!");
    }


    public void Interact()
    {
        OpenShop();
    }

    // This method is now handled by the NPCDetector and simply returns true if the UI is not already active
    public bool CanInteract()
    {
        return shopUI != null && !shopUI.activeSelf;
    }

    private void OpenShop()
    {
        Debug.Log($"shopUI={shopUI}, shopUIManager={shopUIManager}, playerInventory={playerInventory}, playerCurrency={playerCurrency}");

        if (shopUI != null && shopUIManager != null)
        {
            shopUI.SetActive(true);
            shopUIManager.OpenShop(playerInventory, playerCurrency);
        }
        else
        {
            Debug.LogError("OpenShop failed: some references are null!");
        }
    }


    public void CloseShop()
    {
        if (shopUI != null)
        {
            shopUI.SetActive(false);
            // Time.timeScale is no longer a concern since we didn't freeze it
        }
    }
}