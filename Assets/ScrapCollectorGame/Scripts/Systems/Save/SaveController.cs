using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public CurrencyManager currencyManager;           // Tham chiếu đến script quản lý tiền
    public ThanhTheLucplayer playerEnergyManager;     // ✅ Thay đổi: sử dụng ThanhTheLucplayer thay vì ThanhTheLuc
    public InventoryController inventoryController;   // Tham chiếu đến InventoryController

    private string customPath;

    void Start()
    {
        customPath = Path.Combine(Application.persistentDataPath, "savegame.json");
        Debug.Log("Custom Save Path: " + customPath);
    }

    public void SaveGame()
    {
        // Kiểm tra tham chiếu
        if (player == null || currencyManager == null || playerEnergyManager == null || inventoryController == null)
        {
            Debug.LogError("Missing references! Please assign Player, CurrencyManager, ThanhTheLucplayer, and InventoryController in inspector.");
            return;
        }

        // Lấy dữ liệu inventory
        List<InventoryItemData> inventoryData = inventoryController.GetInventoryData();

        // ✅ Lấy thể lực từ ThanhTheLucplayer
        SaveData data = new SaveData(
            playerPosition: player.position,
            playerRotation: player.rotation,
            playerEnergy: playerEnergyManager.luongtheluchientai,    // ✅ Lấy từ ThanhTheLucplayer
            playerCurrency: currencyManager.GetCoins(),
            inventoryItems: inventoryData
        );

        string json = JsonUtility.ToJson(data, true);

        try
        {
            File.WriteAllText(customPath, json);
            Debug.Log("Game saved successfully at: " + customPath);
            Debug.Log($"Saved - Health: {playerEnergyManager.luongtheluchientai}/{playerEnergyManager.luongtheluctoida}, Currency: {currencyManager.GetCoins()}, Items: {inventoryData.Count}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save game: " + e.Message);
        }
    }

    public void LoadGame()
    {
        if (File.Exists(customPath))
        {
            try
            {
                string json = File.ReadAllText(customPath);
                SaveData data = JsonUtility.FromJson<SaveData>(json);

                // Áp dụng dữ liệu vào game
                ApplyLoadedData(data);

                Debug.Log("Game loaded successfully");
                Debug.Log($"Loaded - Health: {data.playerEnergy}, Currency: {data.playerCurrency}, Items: {data.inventoryItems?.Count ?? 0}");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to load game: " + e.Message);
            }
        }
        else
        {
            Debug.LogWarning("No save file found at: " + customPath);
        }
    }

    private void ApplyLoadedData(SaveData data)
    {
        if (player == null || currencyManager == null || playerEnergyManager == null || inventoryController == null)
        {
            Debug.LogError("Cannot apply loaded data - missing references!");
            return;
        }

        // Áp dụng vị trí và rotation của player
        player.position = data.playerPosition;
        player.rotation = data.playerRotation;

        // ✅ Áp dụng thể lực vào ThanhTheLucplayer
        playerEnergyManager.SetEnergy(data.playerEnergy);

        // Áp dụng tiền - Reset về 0 trước, sau đó add số tiền đã lưu
        ResetCurrency();
        if (data.playerCurrency > 0)
        {
            currencyManager.AddCoins(data.playerCurrency);
        }

        // Áp dụng inventory
        inventoryController.LoadInventoryData(data.inventoryItems);

        Debug.Log($"Applied loaded data - Health: {data.playerEnergy}/{playerEnergyManager.luongtheluctoida}, Currency: {data.playerCurrency}, Items: {data.inventoryItems?.Count ?? 0}");
    }

    // Reset tiền về 0
    private void ResetCurrency()
    {
        currencyManager.ResetCoins();
    }

    // ✅ Wrapper methods để tương tác với ThanhTheLucplayer - NĂNG LƯỢNG
    public void UseEnergy(float amount)
    {
        if (playerEnergyManager != null)
        {
            playerEnergyManager.TruTheLuc(amount);
            Debug.Log($"Player used {amount} energy. Current energy: {playerEnergyManager.luongtheluchientai}");
        }
    }

    public void RestoreEnergy(float amount)
    {
        if (playerEnergyManager != null)
        {
            playerEnergyManager.AddEnergy(amount);
            Debug.Log($"Player restored {amount} energy. Current energy: {playerEnergyManager.luongtheluchientai}");
        }
    }

    public float GetCurrentEnergy()
    {
        return playerEnergyManager != null ? playerEnergyManager.luongtheluchientai : 0f;
    }

    public float GetMaxEnergy()
    {
        return playerEnergyManager != null ? playerEnergyManager.luongtheluctoida : 100f;
    }

    // Kiểm tra xem có save file không
    public bool HasSaveFile()
    {
        return File.Exists(customPath);
    }

    // Xóa save file
    public void DeleteSaveFile()
    {
        if (File.Exists(customPath))
        {
            File.Delete(customPath);
            Debug.Log("Save file deleted");
        }
    }
}