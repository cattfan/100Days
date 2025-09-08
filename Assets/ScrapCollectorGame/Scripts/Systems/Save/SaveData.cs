using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int scrapCollected = 0;   
    public Vector3 playerPosition;
    public Quaternion playerRotation;
    public float playerEnergy;
    public int playerCurrency;

    public SaveData(Vector3 playerPosition, Quaternion playerRotation, float playerEnergy, int playerCurrency)
    {
        // scrapCollected giữ nguyên mặc định 0
        this.playerPosition = playerPosition;
        this.playerRotation = playerRotation;
        this.playerEnergy = playerEnergy;
        this.playerCurrency = playerCurrency;
    }
}
