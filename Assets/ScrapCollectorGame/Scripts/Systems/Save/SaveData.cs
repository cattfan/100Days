using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class InventoryItemData
{
    public string itemName;        // Tên item để identify
    public int amount;             // Số lượng
    public int slotIndex;          // Vị trí slot trong inventory

    public InventoryItemData(string itemName, int amount, int slotIndex)
    {
        this.itemName = itemName;
        this.amount = amount;
        this.slotIndex = slotIndex;
    }
}

[System.Serializable]
public class SaveData
{
    public string playerName;      // TÊN PLAYER - THÊM MỚI
    public Vector3 playerPosition;
    public Quaternion playerRotation;
    public float playerEnergy;
    public int playerCurrency;
    public List<InventoryItemData> inventoryItems;

    // Constructor với inventory và playerName
    public SaveData(string playerName, Vector3 playerPosition, Quaternion playerRotation,
                   float playerEnergy, int playerCurrency, List<InventoryItemData> inventoryItems)
    {
        this.playerName = string.IsNullOrEmpty(playerName) ? "Player" : playerName;
        this.playerPosition = playerPosition;
        this.playerRotation = playerRotation;
        this.playerEnergy = playerEnergy;
        this.playerCurrency = playerCurrency;
        this.inventoryItems = inventoryItems ?? new List<InventoryItemData>();
    }

    // Constructor mặc định cho JsonUtility
    public SaveData()
    {
        playerName = "Player";
        playerPosition = Vector3.zero;
        playerRotation = Quaternion.identity;
        playerEnergy = 100f;
        playerCurrency = 0;
        inventoryItems = new List<InventoryItemData>();
    }
}