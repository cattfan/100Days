using UnityEngine;

public class PlayerEating : MonoBehaviour
{
    private PlayerStamina playerStamina;

    void Awake()
    {
        playerStamina = FindAnyObjectByType<PlayerStamina>();
    }

    public void EatFood(ItemData foodItem)
    {
        if (foodItem == null || !foodItem.isFood)
        {
            Debug.LogWarning("This item is not a food item!");
            return;
        }

        if (playerStamina != null)
        {
            playerStamina.RestoreStamina(foodItem.staminaRestoreAmount);
            Debug.Log($"Player ate {foodItem.itemName} and restored {foodItem.staminaRestoreAmount} stamina.");
        }
    }
}