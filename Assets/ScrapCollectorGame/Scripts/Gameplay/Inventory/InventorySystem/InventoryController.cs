using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InventoryController : MonoBehaviour
{
    [Header("Inventory Setup")]
    [SerializeField] public RectTransform inventoryPanel;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private GameObject itemUIPrefab;
    [SerializeField] private int slotCount = 18;


    [Header("Start Items (optional)")]
    public ItemData[] startItems;

    private List<Slot> slots = new List<Slot>();
    private ItemUI cursorItem;
    private float lastClickTime = 0f;
    public bool shopMode = false; // <- flag shop mode

    private void Awake()
    {
        if (!inventoryPanel) Debug.LogError("InventoryController: missing inventoryPanel");
        if (!slotPrefab) Debug.LogError("InventoryController: missing slotPrefab");
        if (!itemUIPrefab) Debug.LogError("InventoryController: missing itemUIPrefab");
    }

    private void Start()
    {
        BuildSlots();

        if (startItems != null && startItems.Length > 0)
        {
            foreach (var data in startItems)
            {
                if (data != null) AddItem(data, 1);
            }
        }
    }
    private void Update()
    {
        if (cursorItem != null)
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                inventoryPanel.root as RectTransform,
                Mouse.current.position.ReadValue(),
                null,
                out pos
            );
            cursorItem.GetComponent<RectTransform>().anchoredPosition = pos;
        }
    }

    private void BuildSlots()
    {
        foreach (Transform c in inventoryPanel) Destroy(c.gameObject);
        slots.Clear();

        for (int i = 0; i < slotCount; i++)
        {
            var go = Instantiate(slotPrefab, inventoryPanel);
            var slotComponent = go.GetComponent<Slot>();
            if (!slotComponent)
                Debug.LogError("slotPrefab must have a Slot component!");
            slots.Add(slotComponent);
        }
    }

    public bool AddItem(ItemData data, int amount = 1)
    {
        if (data == null)
        {
            Debug.LogError("AddItem: Item data is null!");
            return false;
        }

        Debug.Log("AddItem: Attempting to add item " + data.name);

        if (data.isStackable)
        {
            foreach (var s in slots)
            {
                if (s.currentItem != null)
                {
                    var ui = s.currentItem.GetComponent<ItemUI>();
                    if (ui != null && ui.GetItemData() == data)
                    {
                        ui.AddAmount(amount);
                        Debug.Log("AddItem: Added to existing stack.");
                        return true;
                    }
                }
            }
        }

        var emptySlot = slots.FirstOrDefault(s => s.currentItem == null);

        if (emptySlot == null)
        {
            Debug.LogWarning("Inventory full!");
            return false;
        }

        if (itemUIPrefab == null)
        {
            Debug.LogError("AddItem: itemUIPrefab not assigned!");
            return false;
        }

        var itemGO = Instantiate(itemUIPrefab, emptySlot.transform);
        var rt = itemGO.GetComponent<RectTransform>();
        if (rt != null) { rt.anchoredPosition = Vector2.zero; rt.localScale = Vector3.one; }

        var uiComp = itemGO.GetComponent<ItemUI>();
        if (!uiComp) { Debug.LogError("itemUIPrefab missing ItemUI!"); return false; }

        uiComp.Setup(data, amount);
        emptySlot.currentItem = itemGO;
        Debug.Log("AddItem: Added new item to empty slot.");
        return true;
    }

    public void RemoveItem(ItemData data, int amount)
    {
        // Find the first slot with the item
        var slotToRemoveFrom = slots.FirstOrDefault(s => s.currentItem != null && s.currentItem.GetComponent<ItemUI>().GetItemData() == data);

        if (slotToRemoveFrom != null)
        {
            var itemUI = slotToRemoveFrom.currentItem.GetComponent<ItemUI>();
            itemUI.AddAmount(-amount);

            if (itemUI.Amount <= 0)
            {
                Destroy(slotToRemoveFrom.currentItem);
                slotToRemoveFrom.currentItem = null;
            }
        }
    }

    // New public method to remove an item by slot index
    public void RemoveItemAtSlot(int index)
    {
        if (index >= 0 && index < slots.Count)
        {
            var slot = slots[index];
            if (slot.currentItem != null)
            {
                Destroy(slot.currentItem);
                slot.currentItem = null;
            }
        }
    }

    public void HandleSlotClick(Slot slot, PointerEventData.InputButton button)
    {
        if (button == PointerEventData.InputButton.Left)
        {
            HandleLeftClick(slot);
        }
        else if (button == PointerEventData.InputButton.Right)
        {
            HandleRightClick(slot);
        }
    }

    private void HandleLeftClick(Slot slot)
    {
        if (cursorItem == null)
        {
            if (slot.currentItem != null)
            {
                cursorItem = slot.currentItem.GetComponent<ItemUI>();
                cursorItem.transform.SetParent(inventoryPanel.root, true);
                slot.currentItem = null;
            }
        }
        else
        {
            if (slot.currentItem == null)
            {
                slot.currentItem = cursorItem.gameObject;
                cursorItem.transform.SetParent(slot.transform);
                cursorItem.transform.localPosition = Vector3.zero;
                cursorItem = null;
            }
            else
            {
                var slotUI = slot.currentItem.GetComponent<ItemUI>();
                if (slotUI.GetItemData() == cursorItem.GetItemData() && slotUI.GetItemData().isStackable)
                {
                    int moveAmount = cursorItem.Amount;
                    slotUI.AddAmount(moveAmount);
                    Destroy(cursorItem.gameObject);
                    cursorItem = null;
                }
                else
                {
                    var temp = slot.currentItem;
                    slot.currentItem = cursorItem.gameObject;

                    cursorItem.transform.SetParent(slot.transform);
                    cursorItem.transform.localPosition = Vector3.zero;

                    cursorItem = temp.GetComponent<ItemUI>();
                    cursorItem.transform.SetParent(transform);
                }
            }
        }
    }

    private void HandleRightClick(Slot slot)
    {
        if (cursorItem == null)
        {
            if (slot.currentItem != null)
            {
                var slotUI = slot.currentItem.GetComponent<ItemUI>();
                if (slotUI.Amount > 1)
                {
                    int half = slotUI.Amount / 2;
                    slotUI.AddAmount(-half);

                    var clone = Instantiate(itemUIPrefab, transform).GetComponent<ItemUI>();
                    clone.Setup(slotUI.GetItemData(), half);
                    cursorItem = clone;
                }
                else
                {
                    cursorItem = slotUI;
                    slot.currentItem = null;
                    cursorItem.transform.SetParent(inventoryPanel.root, true);
                }
            }
        }
        else
        {
            if (slot.currentItem == null)
            {
                var clone = Instantiate(itemUIPrefab, slot.transform).GetComponent<ItemUI>();
                clone.Setup(cursorItem.GetItemData(), 1);
                slot.currentItem = clone.gameObject;

                cursorItem.AddAmount(-1);
                if (cursorItem.Amount <= 0) { Destroy(cursorItem.gameObject); cursorItem = null; }
            }
        }
    }

    public List<Slot> GetSlots()
    {
        return slots;
    }

    public RectTransform GetInventoryPanel()
    {
        return inventoryPanel;
    }
    public void OnSlotClicked(Slot slot)
    {
        if (slot == null || slot.currentItem == null) return;

        var itemUI = slot.currentItem.GetComponent<ItemUI>();
        if (itemUI == null) return;

        if (shopMode)
        {
            var shopUI = FindFirstObjectByType<ShopUIManager>();
            if (shopUI != null)
            {
                int slotIndex = GetSlots().IndexOf(slot);
                shopUI.ShowConfirmation(itemUI.GetItemData(), itemUI.Amount, slotIndex);
            }
        }
        else
        {
            // luôn coi như Left click
            HandleSlotClick(slot, PointerEventData.InputButton.Left);
        }
    }

}