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

    void Start()
    {
        UpdateCoinText();
    }

    void Update()
    {
        // Test tăng tiền khi nhấn phím T
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            AddCoins(1000000000); // tăng 10 coins
            Debug.Log("Đã thêm 10 coins");
        }
        // Test giảm tiền khi nhấn phím G
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            SpendCoins(5); // giảm 5 coins
            Debug.Log("Đã trừ 5 coins");
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