using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class CurrencyManager : MonoBehaviour
{
    public TMP_Text coinText;
    private int coins = 0; // Biến coins đã là private

    // Thêm hàm public để lấy giá trị của biến coins
    public int GetCoins()
    {
        return coins;
    }

    // Thêm hàm để reset coins về 0 (cho LoadGame)
    public void ResetCoins()
    {
        coins = 0;
        UpdateCoinText();
    }

    // Thêm hàm để set coins trực tiếp (alternative cho ResetCoins + AddCoins)
    public void SetCoins(int amount)
    {
        coins = Mathf.Max(0, amount); // Đảm bảo không âm
        UpdateCoinText();
    }

    void Awake()
    {
        UpdateCoinText();
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
            return true; // Thành công
        }
        else
        {
            return false; // Không đủ tiền
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