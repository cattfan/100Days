using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class CurrencyManager : MonoBehaviour
{
    // 🎯 Singleton Instance: Public static property to access the single instance
    public static CurrencyManager Instance { get; private set; }

    public TMP_Text coinText;
    private int coins = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Giữ lại bản đầu tiên
            Debug.Log("CurrencyManager chính: " + gameObject.name);
        }
        else
        {
            Debug.Log("CurrencyManager phụ (không bị xóa): " + gameObject.name);
            // KHÔNG destroy nữa
        }
    }




    void Start()
    {
        UpdateCoinText();
    }

    void Update()
    {
        // Test function: Press 'T' to add 10 coins
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            AddCoins(10);
            Debug.Log("Đã thêm 10 coins");
        }

        // Test function: Press 'G' to spend 5 coins
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            SpendCoins(5);
            Debug.Log("Đã trừ 5 coins");
        }
    }

    public int GetCoins()
    {
        return coins;
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
        if (coinText != null)
        {
            coinText.text = ": " + coins.ToString();
        }
    }
}