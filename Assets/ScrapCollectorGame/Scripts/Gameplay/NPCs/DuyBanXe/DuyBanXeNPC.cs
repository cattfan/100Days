using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class DuyBanXeNPC : MonoBehaviour, IInteractable
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button btnChonXe;
    [SerializeField] private GameObject hoverChonXe;
    [SerializeField] private Button btnMua;
    [SerializeField] private GameObject messCanhBao;
    [SerializeField] private GameObject messXacNhan;
    [SerializeField] private Button btnXacNhan;
    [SerializeField] private Button btnHuy;
    [SerializeField] private Button btnQuayLai;
    [SerializeField] private Button btnThoat;

    [Header("Car Reference")]
    [SerializeField] private GameObject car; // object có CarInteraction

    [Header("Chi phí")]
    [SerializeField] private int giaXe = 100;

    private SaveController saveCtrl; // 👉 thay vì giữ CurrencyManager riêng
    private bool isPlayerInside = false;
    private bool panelOpened = false;
    private bool isSelected = false;
    private bool isCarPurchased = false; // Track if car is already purchased

    public string GetCarId()
    {
        if (car != null)
        {
            var carInteraction = car.GetComponent<CarInteraction>();
            if (carInteraction != null)
            {
                return carInteraction.GetCarId();
            }
        }
        return gameObject.name + "_car";
    }

    public void SetCarAsPurchased()
    {
        isCarPurchased = true;
        
        // Update UI to reflect purchased state
        if (btnMua != null)
        {
            btnMua.interactable = false;
            Text muaText = btnMua.GetComponentInChildren<Text>();
            if (muaText != null) muaText.text = "Đã mua";
        }
        
        if (btnChonXe != null)
        {
            btnChonXe.interactable = false;
        }

        isSelected = false;
        if (hoverChonXe != null)
        {
            hoverChonXe.SetActive(false);
        }

        Debug.Log($"Car {GetCarId()} set as purchased");
    }

    private void Start()
    {
        saveCtrl = FindObjectOfType<SaveController>();

        btnChonXe.onClick.AddListener(ToggleSelect);
        btnMua.onClick.AddListener(TryBuy);
        btnXacNhan.onClick.AddListener(OnConfirmBuy);
        btnHuy.onClick.AddListener(OnCancelBuy);
        btnQuayLai.onClick.AddListener(OnBackFromWarning);
        btnThoat.onClick.AddListener(OnExitPanel);

        panel.SetActive(false);
        hoverChonXe.SetActive(false);
        messCanhBao.SetActive(false);
        messXacNhan.SetActive(false);

        // Check if car is already purchased on start
        CheckCarPurchaseState();
    }

    private void CheckCarPurchaseState()
    {
        if (car != null)
        {
            var carInteraction = car.GetComponent<CarInteraction>();
            if (carInteraction != null && carInteraction.IsUnlocked())
            {
                SetCarAsPurchased();
            }
        }
    }

    private void Update()
    {
        if (isPlayerInside && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TogglePanel();
        }
    }

    private void TogglePanel()
    {
        panelOpened = !panelOpened;
        panel.SetActive(panelOpened);

        if (panelOpened)
        {
            hoverChonXe.SetActive(isSelected);
            messCanhBao.SetActive(false);
            messXacNhan.SetActive(false);
        }
    }

    private void ToggleSelect()
    {
        if (isCarPurchased) return; // Don't allow selection if already purchased
        
        isSelected = !isSelected;
        hoverChonXe.SetActive(isSelected);
    }

    private void TryBuy()
    {
        if (!isSelected || saveCtrl == null || isCarPurchased) return;

        if (saveCtrl.currencyManager.GetCoins() < giaXe)
        {
            messCanhBao.SetActive(true);
        }
        else
        {
            messXacNhan.SetActive(true);
        }
    }

    private void OnConfirmBuy()
    {
        if (saveCtrl != null && saveCtrl.currencyManager.SpendCoins(giaXe))
        {
            Debug.Log("Mua thành công xe!");
            messXacNhan.SetActive(false);

            // Mở khoá xe
            if (car != null)
            {
                var carInt = car.GetComponent<CarInteraction>();
                if (carInt != null) carInt.UnlockCar();
            }

            // Set car as purchased
            SetCarAsPurchased();

            // 🔑 Gọi Save ngay lập tức để lưu trạng thái mua xe
            saveCtrl.SaveGame();
        }
    }

    private void OnCancelBuy()
    {
        messXacNhan.SetActive(false);
    }

    private void OnBackFromWarning()
    {
        messCanhBao.SetActive(false);
    }

    private void OnExitPanel()
    {
        panelOpened = false;
        panel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            panel.SetActive(false);
            panelOpened = false;
        }
    }

    public bool CanInteract() => true;

    public void Interact()
    {
        Debug.Log("Interacted with DuyBanXe: " + gameObject.name);
    }

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }
}
