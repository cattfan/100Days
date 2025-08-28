using UnityEngine;
using UnityEngine.InputSystem; // Dùng Input System mới
using TMPro;

public class CurrencyManager : MonoBehaviour
{
    public TMP_Text coinText;
    private int coins = 0;

    void Start()
    {
        UpdateCoinText();
    }

    void Update()
    {
        // Test: nhấn phím B để cộng 10 xu
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            AddCoins(10);
        }

        // Test: nhấn phím N để trừ 5 xu
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            SpendCoins(5);
        }
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        UpdateCoinText();
    }

    public void SpendCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            UpdateCoinText();
        }
    }

    private void UpdateCoinText()
    {
        coinText.text = ": " + coins;
    }
}
