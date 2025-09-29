using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class CurrencyManager : MonoBehaviour
{
    public TMP_Text coinText;
    private int coins = 0;

    public int GetCoins()
    {
        return coins;
    }

    public void ResetCoins()
    {
        coins = 0;
        UpdateCoinText();
    }

    public void SetCoins(int amount)
    {
        coins = Mathf.Max(0, amount);
        UpdateCoinText();
    }

    void Awake()
    {
        UpdateCoinText();
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.f3Key.wasPressedThisFrame)
        {
            AddCoins(1000);
            Debug.Log("F3 pressed! Added 1000 coins. Current coins: " + coins);
        }
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        UpdateCoinText();
    }

    public bool SpendCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            UpdateCoinText();
            return true;
        }
        else
        {
            return false;
        }
    }

    private void UpdateCoinText()
    {
        if (coinText != null)
        {
            coinText.text = ": " + coins;
        }
    }

    public void BackMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Mainmenu");
        Time.timeScale = 1f;
    }
}