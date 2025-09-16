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
public class CarData
{
    public string carId;           // Unique identifier for the car
    public bool isUnlocked;        // Whether the car is purchased/unlocked
    public Vector3 carPosition;    // Car's position in the world
    public Quaternion carRotation; // Car's rotation

    public CarData(string carId, bool isUnlocked, Vector3 carPosition, Quaternion carRotation)
    {
        this.carId = carId;
        this.isUnlocked = isUnlocked;
        this.carPosition = carPosition;
        this.carRotation = carRotation;
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
    public List<CarData> carData;  // Car ownership and position data

    // Constructor với inventory và playerName
    public SaveData(string playerName, Vector3 playerPosition, Quaternion playerRotation,
                   float playerEnergy, int playerCurrency, List<InventoryItemData> inventoryItems, List<CarData> carData = null)
    {
        this.playerName = string.IsNullOrEmpty(playerName) ? "Player" : playerName;
        this.playerPosition = playerPosition;
        this.playerRotation = playerRotation;
        this.playerEnergy = playerEnergy;
        this.playerCurrency = playerCurrency;
        this.inventoryItems = inventoryItems ?? new List<InventoryItemData>();
        this.carData = carData ?? new List<CarData>();
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
        carData = new List<CarData>();
    }
}