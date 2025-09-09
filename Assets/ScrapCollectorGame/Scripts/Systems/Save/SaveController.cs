using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public CurrencyManager currencyManager;
    public ThanhTheLucplayer playerEnergyManager;
    public InventoryPersistence inventoryPersistence;

    [Header("Player Info")]
    public string playerName = "Player"; // Tên player, mặc định là "Player"

    private string customPath;

    void Start()
    {
        // Tạo tên file dựa trên playerName
        string fileName = string.IsNullOrEmpty(playerName) ? "Player" : playerName;
        fileName = SanitizeFileName(fileName);
        customPath = Path.Combine(Application.persistentDataPath, $"{fileName}_save.json");
        Debug.Log("Custom Save Path: " + customPath);

        // 🔑 Kiểm tra xem có yêu cầu load từ MainMenu không
        string saveToLoad = PlayerPrefs.GetString("SaveToLoad", "");
        if (!string.IsNullOrEmpty(saveToLoad))
        {
            // Gán playerName theo file lưu từ MainMenu
            SetPlayerName(saveToLoad);
            LoadGame();

            // Xóa key để tránh load lại khi restart
            PlayerPrefs.DeleteKey("SaveToLoad");
        }
    }


    // Làm sạch tên file, loại bỏ ký tự không hợp lệ
    private string SanitizeFileName(string fileName)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        foreach (char c in invalidChars)
        {
            fileName = fileName.Replace(c, '_');
        }
        return fileName.Trim();
    }

    // Phương thức để thay đổi tên player và cập nhật đường dẫn file
    public void SetPlayerName(string newName)
    {
        playerName = string.IsNullOrEmpty(newName) ? "Player" : newName;
        string fileName = SanitizeFileName(playerName);
        customPath = Path.Combine(Application.persistentDataPath, $"{fileName}_save.json");
        Debug.Log("Updated Save Path: " + customPath);
    }

    public string GetPlayerName()
    {
        return playerName;
    }

    public void SaveGame()
    {
        // Kiểm tra tham chiếu
        if (player == null || currencyManager == null || playerEnergyManager == null || inventoryPersistence == null)
        {
            Debug.LogError("Missing references! Please assign Player, CurrencyManager, ThanhTheLucplayer, and InventoryPersistence in inspector.");
            return;
        }

        // Lấy dữ liệu inventory từ InventoryPersistence
        List<InventoryItemData> inventoryData = inventoryPersistence.GetInventoryData();

        SaveData data = new SaveData(
            playerName: playerName,
            playerPosition: player.position,
            playerRotation: player.rotation,
            playerEnergy: playerEnergyManager.luongtheluchientai,
            playerCurrency: currencyManager.GetCoins(),
            inventoryItems: inventoryData
        );

        string json = JsonUtility.ToJson(data, true);

        try
        {
            File.WriteAllText(customPath, json);
            Debug.Log($"Game saved successfully for {playerName} at: " + customPath);
            Debug.Log($"Saved - Player: {playerName}, Health: {playerEnergyManager.luongtheluchientai}/{playerEnergyManager.luongtheluctoida}, Currency: {currencyManager.GetCoins()}, Items: {inventoryData.Count}");
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

                // Cập nhật tên player nếu có trong save data
                if (!string.IsNullOrEmpty(data.playerName))
                {
                    playerName = data.playerName;
                }

                ApplyLoadedData(data);

                Debug.Log($"Game loaded successfully for {playerName}");
                Debug.Log($"Loaded - Player: {data.playerName}, Health: {data.playerEnergy}, Currency: {data.playerCurrency}, Items: {data.inventoryItems?.Count ?? 0}");
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
        if (player == null || currencyManager == null || playerEnergyManager == null || inventoryPersistence == null)
        {
            Debug.LogError("Cannot apply loaded data - missing references!");
            return;
        }

        // Vị trí + rotation
        player.position = data.playerPosition;
        player.rotation = data.playerRotation;

        // Thể lực
        playerEnergyManager.SetEnergy(data.playerEnergy);

        // Tiền
        ResetCurrency();
        if (data.playerCurrency > 0)
        {
            currencyManager.AddCoins(data.playerCurrency);
        }

        // Inventory (dùng InventoryPersistence)
        inventoryPersistence.LoadInventoryData(data.inventoryItems);

        Debug.Log($"Applied loaded data - Player: {data.playerName}, Health: {data.playerEnergy}/{playerEnergyManager.luongtheluctoida}, Currency: {data.playerCurrency}, Items: {data.inventoryItems?.Count ?? 0}");
    }

    private void ResetCurrency()
    {
        currencyManager.ResetCoins();
    }

    // Energy helpers
    public void UseEnergy(float amount)
    {
        if (playerEnergyManager != null)
        {
            playerEnergyManager.TruTheLuc(amount);
            Debug.Log($"{playerName} used {amount} energy. Current energy: {playerEnergyManager.luongtheluchientai}");
        }
    }

    public void RestoreEnergy(float amount)
    {
        if (playerEnergyManager != null)
        {
            playerEnergyManager.AddEnergy(amount);
            Debug.Log($"{playerName} restored {amount} energy. Current energy: {playerEnergyManager.luongtheluchientai}");
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

    public bool HasSaveFile()
    {
        return File.Exists(customPath);
    }

    public void DeleteSaveFile()
    {
        if (File.Exists(customPath))
        {
            File.Delete(customPath);
            Debug.Log($"Save file deleted for {playerName}");
        }
    }

    // Phương thức để lấy danh sách tất cả file save có thể
    public List<string> GetAllSaveFiles()
    {
        List<string> saveFiles = new List<string>();
        string[] files = Directory.GetFiles(Application.persistentDataPath, "*_save.json");

        foreach (string file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            string playerNameFromFile = fileName.Replace("_save", "");
            saveFiles.Add(playerNameFromFile);
        }

        return saveFiles;
    }
}