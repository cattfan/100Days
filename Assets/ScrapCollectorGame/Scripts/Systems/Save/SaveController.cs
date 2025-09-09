using UnityEngine;
using System.IO;

public class SaveController : MonoBehaviour
{
    private string customPath;

    void Start()
    {
        customPath = Path.Combine(Application.persistentDataPath, "savegame.json");
        Debug.Log("Custom Save Path: " + customPath);
    }

    public void SaveGame()
    {
        // Ở đây bạn phải lấy dữ liệu từ Player / GameManager
        SaveData data = new SaveData(
            playerPosition: new Vector3(0, 1, 0),
            playerRotation: Quaternion.identity,
            playerEnergy: 100f,
            playerCurrency: 500
        );

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(customPath, json);
        Debug.Log("Game saved at: " + customPath);
    }

    public void LoadGame()
    {
        if (File.Exists(customPath))
        {
            string json = File.ReadAllText(customPath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            Debug.Log("Game loaded");
        }
        else
        {
            Debug.LogWarning("No save file found!");
        }
    }
}
