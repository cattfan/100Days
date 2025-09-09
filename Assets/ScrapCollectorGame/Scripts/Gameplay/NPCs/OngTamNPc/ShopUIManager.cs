using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopUIManager : MonoBehaviour
{
    [Header("UI References")]
    public Button closeButton;
    public GameObject confirmationDialog;
    public TextMeshProUGUI confirmationText;
    public Button confirmSellButton;
    public Button cancelSellButton;

    [Header("Shop Inventory Panel")]
    public RectTransform shopInventoryPanel;
    public GameObject slotPrefab;
    public GameObject itemUIPrefab;

    [Header("Audio")]
    public AudioManagement audioManagement;

    private InventoryController playerInventory;
    private CurrencyManager playerCurrency;

    private ItemData currentItemToSell;
    private int currentPlayerSlotIndex = -1;
    private int currentItemQuantity = 0;

    private List<Slot> shopSlots = new List<Slot>();

    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseShopUI);
        }

        if (confirmSellButton != null)
        {
            confirmSellButton.onClick.RemoveAllListeners();
            confirmSellButton.onClick.AddListener(ConfirmSell);
        }

        if (cancelSellButton != null)
        {
            cancelSellButton.onClick.RemoveAllListeners();
            cancelSellButton.onClick.AddListener(HideConfirmation);
        }

        if (confirmationDialog != null)
            confirmationDialog.SetActive(false);
        GameObject audioObject = GameObject.FindGameObjectWithTag("Audio");
        if (audioObject != null)
        {
            audioManagement = audioObject.GetComponent<AudioManagement>();
        }
    }

    // Gọi từ ShopNPC
    public void OpenShop(InventoryController inventory, CurrencyManager currency)
    {
        this.playerInventory = inventory;
        this.playerCurrency = currency;

        CloneInventory();
        this.gameObject.SetActive(true);
    }

    private void CloseShopUI()
    {
        ShopNPC shopNPC = FindFirstObjectByType<ShopNPC>();
        if (shopNPC != null) shopNPC.CloseShop();

        // reset confirmation dialog
        if (confirmationDialog != null)
            confirmationDialog.SetActive(false);

        this.gameObject.SetActive(false);

        // reset trạng thái item hiện tại
        currentItemToSell = null;
        currentPlayerSlotIndex = -1;
        currentItemQuantity = 0;
        audioManagement.PlaySFX(audioManagement.CloseMenu);
    }


    /// <summary>
    /// Clone inventory thật sang shop panel
    /// </summary>
    private void CloneInventory()
    {
        // Xóa clone cũ
        foreach (Transform c in shopInventoryPanel)
            Destroy(c.gameObject);
        shopSlots.Clear();

        var slots = playerInventory.GetSlots();
        for (int i = 0; i < slots.Count; i++)
        {
            var slotClone = Instantiate(slotPrefab, shopInventoryPanel).GetComponent<Slot>();
            shopSlots.Add(slotClone);

            if (slots[i].currentItem != null)
            {
                var itemUI = slots[i].currentItem.GetComponent<ItemUI>();
                if (itemUI != null)
                {
                    var itemClone = Instantiate(itemUIPrefab, slotClone.transform).GetComponent<ItemUI>();
                    itemClone.Setup(itemUI.GetItemData(), itemUI.Amount);

                    // Vô hiệu hóa drag/drop trong shop
                    var dragHandler = itemClone.GetComponent<ItemUI>();
                    if (dragHandler != null) dragHandler.enabled = false;

                    slotClone.currentItem = itemClone.gameObject;

                    // Gắn event click => ShowConfirmation
                    int slotIndex = i;
                    var btn = itemClone.gameObject.AddComponent<Button>();
                    btn.onClick.AddListener(() =>
                    {
                        ShowConfirmation(itemUI.GetItemData(), itemUI.Amount, slotIndex);
                    });
                }
            }
        }
    }

    public void ShowConfirmation(ItemData item, int quantity, int slotIndex)
    {
        if (item == null || !item.canSell)
        {
            Debug.Log("This item cannot be sold.");
            return;
        }

        currentItemToSell = item;
        currentPlayerSlotIndex = slotIndex;
        currentItemQuantity = quantity;

        int sellPrice = item.baseSellPrice * quantity;

        if (confirmationDialog != null)
        {
            confirmationDialog.SetActive(true);
            if (confirmationText != null)
                confirmationText.text = $"Sell {quantity} {currentItemToSell.itemName} for {sellPrice} coins?";

            // đưa popup lên top
            confirmationDialog.transform.SetAsLastSibling();
        }

        audioManagement.PlaySFX(audioManagement.Select);
    }



    private void HideConfirmation()
    {
        if (confirmationDialog != null)
            confirmationDialog.SetActive(false);

       audioManagement.PlaySFX(audioManagement.CloseMenu);
    }

    private void ConfirmSell()
    {
        if (currentItemToSell == null || currentPlayerSlotIndex == -1) return;

        int sellPrice = currentItemToSell.baseSellPrice * currentItemQuantity;

        // Thêm tiền cho người chơi
        if (playerCurrency != null)
        {
            playerCurrency.AddCoins(sellPrice);
            Debug.Log($"Added {sellPrice} coins to player.");
        }

        // Xóa item trong inventory thật
        playerInventory.RemoveItemAtSlot(currentPlayerSlotIndex);

        // Xóa item trong clone UI
        if (currentPlayerSlotIndex >= 0 && currentPlayerSlotIndex < shopSlots.Count)
        {
            var slot = shopSlots[currentPlayerSlotIndex];
            if (slot != null && slot.currentItem != null)
            {
                Destroy(slot.currentItem);
                slot.currentItem = null;
            }
        }

        HideConfirmation();

        currentItemToSell = null;
        currentPlayerSlotIndex = -1;
        currentItemQuantity = 0;

       audioManagement.PlaySFX(audioManagement.SellItem);
        Debug.Log($"Successfully sold item for {sellPrice} coins!");
    }

}
