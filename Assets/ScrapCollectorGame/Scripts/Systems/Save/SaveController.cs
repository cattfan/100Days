using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SaveController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public CurrencyManager currencyManager;
    public ThanhTheLucplayer playerEnergyManager;
    public Inventory inventoryPersistence;

    [Header("Player Info")]
    public string playerName = "Player"; // Tên player, mặc định là "Player"

    private string customPath;

    // Replace the Start method with a coroutine and call it from Start
    void Start()
    {
        StartCoroutine(InitAndLoadIfMainGame());
    }

    // Thay thế InitAndLoadIfMainGame trong SaveController
    private System.Collections.IEnumerator InitAndLoadIfMainGame()
    {
        // Tạo tên file dựa trên playerName
        string fileName = string.IsNullOrEmpty(playerName) ? "Player" : playerName;
        fileName = SanitizeFileName(fileName);
        customPath = Path.Combine(Application.persistentDataPath, $"{fileName}_save.json");
        Debug.Log("Custom Save Path: " + customPath);

        // Nếu đang ở MainGame thì mới cần auto load
        yield return null; // đợi 1 frame cho mọi script khác init
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Debug.Log($"Current Scene: {currentScene}");

        if (currentScene == "Maingame")
        {
            string saveToLoad = PlayerPrefs.GetString("SaveToLoad", "");
            Debug.Log($"SaveToLoad from PlayerPrefs: '{saveToLoad}'");

            if (!string.IsNullOrEmpty(saveToLoad))
            {
                Debug.Log($"Setting player name to: {saveToLoad}");
                SetPlayerName(saveToLoad);

                Debug.Log($"Loading game for player: {playerName}");
                Debug.Log($"Save file exists: {File.Exists(customPath)}");
                Debug.Log($"Save file path: {customPath}");

                LoadGame();
                PlayerPrefs.DeleteKey("SaveToLoad");
                Debug.Log("SaveToLoad key deleted from PlayerPrefs");
            }
            else
            {
                Debug.Log("No SaveToLoad found in PlayerPrefs - starting new game");
            }
        }
        else
        {
            Debug.Log($"Not in MainGame scene, skipping auto-load");
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

        // Lấy dữ liệu xe từ tất cả CarInteraction trong scene
        List<CarData> carData = GetAllCarData();

        SaveData data = new SaveData(
            playerName: playerName,
            playerPosition: player.position,
            playerRotation: player.rotation,
            playerEnergy: playerEnergyManager.luongtheluchientai,
            playerCurrency: currencyManager.GetCoins(),
            inventoryItems: inventoryData,
            carData: carData
        );

        string json = JsonUtility.ToJson(data, true);

        try
        {
            File.WriteAllText(customPath, json);
            Debug.Log($"Game saved successfully for {playerName} at: " + customPath);
            Debug.Log($"Saved - Player: {playerName}, Health: {playerEnergyManager.luongtheluchientai}/{playerEnergyManager.luongtheluctoida}, Currency: {currencyManager.GetCoins()}, Items: {inventoryData.Count}, Cars: {carData.Count}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save game: " + e.Message);
        }
    }

    public void LoadGame()
    {
        Debug.Log($"LoadGame called - checking file: {customPath}");
        Debug.Log($"Before LoadGame - PlayerName: {playerName}, Save Path: {customPath}");


        if (File.Exists(customPath))
        {
            try
            {
                Debug.Log("Save file found, reading...");
                string json = File.ReadAllText(customPath);
                Debug.Log($"JSON content: {json}");

                SaveData data = JsonUtility.FromJson<SaveData>(json);
                Debug.Log($"JSON parsed successfully");

                // Cập nhật tên player nếu có trong save data
                if (!string.IsNullOrEmpty(data.playerName))
                {
                    playerName = data.playerName;
                    Debug.Log($"Player name updated to: {playerName}");
                }

                Debug.Log("Applying loaded data...");
                ApplyLoadedData(data);

                Debug.Log($"Game loaded successfully for {playerName}");
                Debug.Log($"Loaded - Player: {data.playerName}, Health: {data.playerEnergy}, Currency: {data.playerCurrency}, Items: {data.inventoryItems?.Count ?? 0}, Cars: {data.carData?.Count ?? 0}");
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

    // Thêm debug vào ApplyLoadedData
    private void ApplyLoadedData(SaveData data)
    {
        Debug.Log("ApplyLoadedData called");

        if (player == null || currencyManager == null || playerEnergyManager == null || inventoryPersistence == null)
        {
            Debug.LogError("Cannot apply loaded data - missing references!");
            Debug.LogError($"Player: {player != null}, CurrencyManager: {currencyManager != null}, EnergyManager: {playerEnergyManager != null}, InventoryPersistence: {inventoryPersistence != null}");
            return;
        }

        Debug.Log("All references found, applying data...");

        // Vị trí + rotation
        Debug.Log($"Setting player position to: {data.playerPosition}");
        player.position = data.playerPosition;
        player.rotation = data.playerRotation;

        // Thể lực
        Debug.Log($"Setting player energy to: {data.playerEnergy}");
        playerEnergyManager.SetEnergy(data.playerEnergy);

        // Tiền
        Debug.Log($"Setting currency to: {data.playerCurrency}");
        currencyManager.SetCoins(data.playerCurrency); // Sử dụng SetCoins thay vì ResetCoins + AddCoins

        // Inventory (dùng InventoryPersistence)
        Debug.Log($"Loading inventory with {data.inventoryItems?.Count ?? 0} items");
        inventoryPersistence.LoadInventoryData(data.inventoryItems);
        Debug.Log("Inventory slots after load: " + inventoryPersistence.GetSlots().Count(s => s.currentItem != null));

        // Car data - Load car ownership and positions
        Debug.Log($"Loading car data with {data.carData?.Count ?? 0} cars");
        LoadAllCarData(data.carData);

        Debug.Log($"Applied loaded data - Player: {data.playerName}, Health: {data.playerEnergy}/{playerEnergyManager.luongtheluctoida}, Currency: {data.playerCurrency}, Items: {data.inventoryItems?.Count ?? 0}, Cars: {data.carData?.Count ?? 0}");
    }

    private List<CarData> GetAllCarData()
    {
        List<CarData> carDataList = new List<CarData>();
        CarInteraction[] allCars = FindObjectsOfType<CarInteraction>();

        foreach (CarInteraction car in allCars)
        {
            carDataList.Add(car.GetCarData());
        }

        Debug.Log($"Found {carDataList.Count} cars to save");
        return carDataList;
    }

    private void LoadAllCarData(List<CarData> carDataList)
    {
        if (carDataList == null || carDataList.Count == 0)
        {
            Debug.Log("No car data to load");
            return;
        }

        CarInteraction[] allCars = FindObjectsOfType<CarInteraction>();
        DuyBanXeNPC[] allCarNPCs = FindObjectsOfType<DuyBanXeNPC>();

        foreach (CarData carData in carDataList)
        {
            // Load car state
            CarInteraction targetCar = System.Array.Find(allCars, car => car.GetCarId() == carData.carId);
            if (targetCar != null)
            {
                targetCar.LoadCarData(carData);
            }

            // Update corresponding car dealer NPC state
            DuyBanXeNPC correspondingNPC = System.Array.Find(allCarNPCs, npc => npc.GetCarId() == carData.carId);
            if (correspondingNPC != null && carData.isUnlocked)
            {
                correspondingNPC.SetCarAsPurchased();
            }
        }

        Debug.Log($"Loaded {carDataList.Count} car states");
    }

    private void ResetCurrency()
    {
        currencyManager.ResetCoins();
    }


    public float GetCurrentEnergy()
    {
        return playerEnergyManager != null ? playerEnergyManager.luongtheluchientai : 0f;
    }

    public float GetMaxEnergy()
    {
        return playerEnergyManager != null ? playerEnergyManager.luongtheluctoida : 100f;
    }

    // Hàm mới để kiểm tra sự tồn tại của file save
    public bool HasSaveFile(string playerName)
    {
        string fileName = SanitizeFileName(playerName);
        string path = Path.Combine(Application.persistentDataPath, $"{fileName}_save.json");
        return File.Exists(path);
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

    public List<(string playerName, int coins)> GetAllSaveSummaries()
    {
        List<(string, int)> summaries = new List<(string, int)>();
        string[] files = Directory.GetFiles(Application.persistentDataPath, "*_save.json");

        foreach (string file in files)
        {
            try
            {
                string json = File.ReadAllText(file);
                SaveData data = JsonUtility.FromJson<SaveData>(json);

                string name = string.IsNullOrEmpty(data.playerName) ? "Player" : data.playerName;
                int coins = data.playerCurrency;
                summaries.Add((name, coins));
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to read save summary: " + e.Message);
            }
        }

        return summaries;
    }

    public void DeleteSaveFile(string playerName)
    {
        string fileName = SanitizeFileName(playerName);
        string path = Path.Combine(Application.persistentDataPath, $"{fileName}_save.json");

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"Save file deleted for {playerName}");
        }
        else
        {
            Debug.LogWarning($"No save file found for player {playerName} at path: {path}");
        }
    }

}
