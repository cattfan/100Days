using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class Inventory : MonoBehaviour
{
    [Header("Inventory Setup")]
    [SerializeField] public RectTransform inventoryPanel;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private GameObject itemUIPrefab;
    [SerializeField] private int slotCount = 18;

    [Header("All Available Items")]
    public ItemData[] allItems;

    [Header("Start Items (optional)")]
    public ItemData[] startItems;

    private List<Slot> slots = new List<Slot>();

    private void Awake()
    {
        if (!inventoryPanel) Debug.LogError("Inventory: missing inventoryPanel");
        if (!slotPrefab) Debug.LogError("Inventory: missing slotPrefab");
        if (!itemUIPrefab) Debug.LogError("Inventory: missing itemUIPrefab");

        BuildSlots();
    }

    private void Start()
    {
        var saveController = FindFirstObjectByType<SaveController>();
        if (saveController == null || !saveController.HasSaveFile(saveController.GetPlayerName()))
        {
            if (startItems != null && startItems.Length > 0)
            {
                foreach (var data in startItems)
                {
                    if (data != null) AddItem(data, 1);
                }
            }
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

    public List<Slot> GetSlots() => slots;
    public RectTransform GetInventoryPanel() => inventoryPanel;
    public ItemData FindItemDataByName(string itemName) =>
        allItems?.FirstOrDefault(item => item != null && item.name == itemName);
}
