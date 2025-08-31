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

    void Start()
    {
        UpdateCoinText();
    }

 
   void Update()
    {
        // Test tăng tiền khi nhấn phím T
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            AddCoins(10); // tăng 10 coins
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

    public void SpendCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            UpdateCoinText();
        }
        else
        {
            Debug.Log("Không đủ tiền để mua!");
        }
    }

    private void UpdateCoinText()
    {
        coinText.text = ": " + coins;
    }
}