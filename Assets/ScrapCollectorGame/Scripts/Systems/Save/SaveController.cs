using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public CurrencyManager currencyManager;
    public ThanhTheLucplayer playerEnergyManager;
    public InventoryController inventoryController;

    private string saveFolder;

    // ✅ HÀM WRAPPER CHO BUTTON
    public void SaveDefaultGame()
    {
        SaveGame("save1");
    }

    public void LoadDefaultGame()
    {
        LoadGame("save1");
    }

    // Lưu game với tên file
    public void SaveGame(string saveName)
    {
        if (player == null || currencyManager == null || playerEnergyManager == null || inventoryController == null)
        {
            Debug.LogError("Missing references!");
            return;
        }

        List<InventoryItemData> inventoryData = inventoryController.GetInventoryData();

        SaveData data = new SaveData(
            playerPosition: player.position,
            playerRotation: player.rotation,
            playerEnergy: playerEnergyManager.luongtheluchientai,
            playerCurrency: currencyManager.GetCoins(),
            inventoryItems: inventoryData
        );

        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine(saveFolder, saveName + ".json");

        File.WriteAllText(path, json);
        Debug.Log("Game saved: " + path);
    }

    // Load game từ file cụ thể
    public void LoadGame(string saveName)
    {
        string path = Path.Combine(saveFolder, saveName + ".json");

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            ApplyLoadedData(data);
            Debug.Log("Game loaded: " + saveName);
        }
        else
        {
            Debug.LogWarning("Save file not found: " + saveName);
        }
    }

    // Lấy danh sách tất cả file save
    public List<string> GetAllSaveFiles()
    {
        List<string> saveFiles = new List<string>();
        if (Directory.Exists(saveFolder))
        {
            foreach (var file in Directory.GetFiles(saveFolder, "*.json"))
            {
                saveFiles.Add(Path.GetFileNameWithoutExtension(file));
            }
        }
        return saveFiles;
    }

    private void ApplyLoadedData(SaveData data)
    {
        if (player == null || currencyManager == null || playerEnergyManager == null || inventoryController == null)
        {
            Debug.LogError("Cannot apply loaded data - missing references!");
            return;
        }

        player.position = data.playerPosition;
        player.rotation = data.playerRotation;
        playerEnergyManager.SetEnergy(data.playerEnergy);

        currencyManager.ResetCoins();
        if (data.playerCurrency > 0) currencyManager.AddCoins(data.playerCurrency);

        inventoryController.LoadInventoryData(data.inventoryItems);
    }
    void Start()
    {
        // Tạo thư mục lưu game nếu chưa có
        saveFolder = Path.Combine(Application.persistentDataPath, "Saves");
        if (!Directory.Exists(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
        }

        // 🔑 Đọc tên file cần load từ PlayerPrefs
        string saveName = PlayerPrefs.GetString("SaveToLoad", "");

        if (!string.IsNullOrEmpty(saveName))
        {
            LoadGame(saveName); // Load dữ liệu
            PlayerPrefs.DeleteKey("SaveToLoad"); // Xóa key để không tự load lại lần sau
        }
    }

}
