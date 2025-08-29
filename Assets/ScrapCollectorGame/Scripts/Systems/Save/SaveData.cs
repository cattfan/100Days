using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int scrapCollected;
    public Vector3 playerPosition;
    public Quaternion playerRotation;
    public float playerEnergy;
    public int playerCurrency;
    // Add other game state variables as needed

    public SaveData( Vector3 playerPosition, Quaternion playerRotation, float playerEnergy, int playerCurrency)
    {
        this.scrapCollected = scrapCollected;
        this.playerPosition = playerPosition;
        this.playerRotation = playerRotation;
        this.playerEnergy = playerEnergy;
        this.playerCurrency = playerCurrency;
    }
}
