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

<<<<<<< HEAD


=======
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
>>>>>>> origin/main

    void Start()
    {
        UpdateCoinText();
    }

    void Update()
    {
        // Test function: Press 'T' to add 10 coins
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
<<<<<<< HEAD
            AddCoins(10);
            Debug.Log("Đã thêm 10 coins");
        }

        // Test function: Press 'G' to spend 5 coins
=======
            AddCoins(1000000000); // tăng 10 coins
            Debug.Log("Đã thêm 10 coins");
        }
        // Test giảm tiền khi nhấn phím G
>>>>>>> origin/main
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
<<<<<<< HEAD
            coinText.text = ": " + coins.ToString();
=======
            coinText.text = ": " + coins;
>>>>>>> origin/main
        }
    }
}