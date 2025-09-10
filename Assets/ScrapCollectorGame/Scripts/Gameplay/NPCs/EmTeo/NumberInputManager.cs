using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NumberInputSystem
{
    public partial class NumberInputManager : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject numberButtonPrefab;
        public GameObject displayText;

        [Header("UI References")]
        public Transform buttonParent;
        public Transform displayParent;
        public Button deleteButton;
        public Button confirmButton;
        public GameObject targetUI;
        private PlayerInput playerInput;

        private List<Button> numberButtons = new List<Button>();
        private List<TextMeshProUGUI> displayTexts = new List<TextMeshProUGUI>();

        private void Awake()
        {
            if (targetUI != null)
                targetUI.SetActive(false);

            // Reset tất cả dữ liệu khi Awake
            ResetAllData();
        }

        void Start()
        {
            Debug.Log("=== NumberInputManager Start ===");

            // Reset lại một lần nữa để đảm bảo
            ResetAllData();

            CreateNumberButtons();
            CreateDisplayTexts();
            SetupButtons();
            UpdateDisplay();
            playerInput = FindFirstObjectByType<PlayerInput>();

            Debug.Log("=== NumberInputManager Start Complete ===");
        }

        void OnEnable()
        {
            // Reset mỗi khi GameObject được active
            ResetAllData();

            // Nếu đã có UI được tạo rồi thì cập nhật display
            if (displayTexts.Count > 0)
            {
                UpdateDisplay();
            }
        }

        /// <summary>
        /// Reset tất cả dữ liệu về trạng thái ban đầu
        /// </summary>
        void ResetAllData()
        {
            Debug.Log("=== Resetting All Data ===");

            // Reset enteredNumbers (giả sử có trong partial class khác)
            if (enteredNumbers != null)
            {
                enteredNumbers.Clear();
            }

            // Reset GameState nếu cần
            if (playerInput != null)
                playerInput.enabled = false;

            Debug.Log("=== Reset Complete ===");
        }

        /// <summary>
        /// Clear và tạo lại tất cả UI elements
        /// </summary>
        void ClearAndRecreateUI()
        {
            // Clear buttons cũ
            ClearNumberButtons();

            // Clear display texts cũ  
            ClearDisplayTexts();

            // Tạo lại
            CreateNumberButtons();
            CreateDisplayTexts();
            SetupButtons();
            UpdateDisplay();
        }

        void ClearNumberButtons()
        {
            // Remove listeners và destroy buttons cũ
            foreach (var button in numberButtons)
            {
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    if (button.gameObject != null)
                        DestroyImmediate(button.gameObject);
                }
            }
            numberButtons.Clear();

            // Clear tất cả children của buttonParent
            if (buttonParent != null)
            {
                for (int i = buttonParent.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(buttonParent.GetChild(i).gameObject);
                }
            }
        }

        void ClearDisplayTexts()
        {
            // Clear display texts cũ
            displayTexts.Clear();

            // Clear tất cả children của displayParent
            if (displayParent != null)
            {
                for (int i = displayParent.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(displayParent.GetChild(i).gameObject);
                }
            }
        }

        void CreateNumberButtons()
        {
            Debug.Log($"Bắt đầu tạo number buttons. Button Parent: {buttonParent}");
            if (buttonParent == null || numberButtonPrefab == null)
            {
                Debug.LogError("Thiếu Button Parent hoặc Number Button Prefab!");
                return;
            }

            int[] numbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };

            for (int i = 0; i < numbers.Length; i++)
            {
                GameObject buttonObj = Instantiate(numberButtonPrefab, buttonParent);
                Button button = buttonObj.GetComponent<Button>();

                int currentNumber = numbers[i];
                if (button != null)
                {
                    button.onClick.AddListener(() => OnNumberButtonClicked(currentNumber));
                    numberButtons.Add(button);
                }

                Text buttonText = buttonObj.GetComponentInChildren<Text>();
                TextMeshProUGUI buttonTextTMP = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

                if (buttonText != null)
                    buttonText.text = currentNumber.ToString();
                else if (buttonTextTMP != null)
                    buttonTextTMP.text = currentNumber.ToString();
            }

            Debug.Log($"Đã tạo {numberButtons.Count} number buttons");
        }

        void CreateDisplayTexts()
        {
            if (displayParent == null || displayText == null) return;

            Debug.Log($"Tạo {maxNumbers} display texts");

            for (int i = 0; i < maxNumbers; i++)
            {
                GameObject textObj = Instantiate(displayText, displayParent);
                TextMeshProUGUI textComponent = textObj.GetComponent<TextMeshProUGUI>() ??
                                                textObj.GetComponentInChildren<TextMeshProUGUI>();

                if (textComponent != null)
                {
                    displayTexts.Add(textComponent);
                    textComponent.text = "_";
                }
                else
                {
                    displayTexts.Add(null);
                    Debug.LogWarning($"Không tìm thấy TextMeshProUGUI component trong display text {i}");
                }
            }

            Debug.Log($"Đã tạo {displayTexts.Count} display texts");
        }

        void SetupButtons()
        {
            // Reset listeners trước khi add mới
            if (deleteButton != null)
            {
                deleteButton.onClick.RemoveAllListeners();
                deleteButton.onClick.AddListener(OnDeleteButtonClicked);
                deleteButton.interactable = false; // Bắt đầu là disabled
            }

            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveAllListeners();
                confirmButton.onClick.AddListener(OnConfirmButtonClicked);
                confirmButton.interactable = false; // Bắt đầu là disabled
            }
        }

        void UpdateDisplay()
        {
            for (int i = 0; i < displayTexts.Count; i++)
            {
                if (displayTexts[i] != null)
                {
                    if (i < enteredNumbers.Count)
                        displayTexts[i].text = enteredNumbers[i].ToString();
                    else
                        displayTexts[i].text = "_";
                }
            }

            // Update button states
            if (confirmButton != null)
                confirmButton.interactable = (enteredNumbers.Count == maxNumbers);

            if (deleteButton != null)
                deleteButton.interactable = (enteredNumbers.Count > 0);
        }

        void ShowTargetUI()
        {
            if (targetUI != null)
            {
                targetUI.SetActive(true);
                if (playerInput != null)
                    playerInput.enabled = false;
                this.gameObject.SetActive(false);

                INumberReceiver receiver = targetUI.GetComponent<INumberReceiver>();
                if (receiver != null)
                {
                    Debug.Log($"Gửi {enteredNumbers.Count} số tới {targetUI.name}: [{string.Join(", ", enteredNumbers)}]");
                    receiver.ReceiveNumbers(enteredNumbers.ToArray());
                }
            }
        }

        /// <summary>
        /// Public method để reset từ bên ngoài nếu cần
        /// </summary>
        public void ForceReset()
        {
            Debug.Log("=== Force Reset Called ===");
            ResetAllData();
            ClearAndRecreateUI();
        }

        void OnDestroy()
        {
            // Clean up khi destroy
            ClearNumberButtons();
            ClearDisplayTexts();
        }
    }

    // Interface để UI khác có thể nhận dữ liệu số
    public interface INumberReceiver
    {
        void ReceiveNumbers(int[] numbers);
    }
}