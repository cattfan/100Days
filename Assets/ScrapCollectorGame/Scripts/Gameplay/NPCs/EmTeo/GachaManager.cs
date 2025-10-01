using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace NumberInputSystem
{
    public partial class GachaManager : MonoBehaviour, INumberReceiver
    {
        [Header("UI References")]
        public Transform gachaParent;
        public Transform selectedParent;
        public GameObject numberPrefab;
        public Button startButton;
        public Button exitButton;
        public GameObject RewardMessage;
        
        [Header("Settings")]
        public int maxDigit = 9;           // số lớn nhất random (0..9)
        public int firstDigitMax = 1;      // số đầu tiên chỉ từ 0-1
        
        // Data chung
        private List<TextMeshProUGUI> gachaTexts = new List<TextMeshProUGUI>();
        private List<TextMeshProUGUI> selectedTexts = new List<TextMeshProUGUI>();
        private int[] playerNumbers;
        
        void Start()
        {
            if (exitButton != null) exitButton.interactable = false;
            if (RewardMessage != null) RewardMessage.SetActive(false);
        }
        
        public void ReceiveNumbers(int[] numbers)
        {
            Debug.Log("GachaManager nhận số từ NumberInputManager: " + string.Join(", ", numbers));
            
            // Validate và điều chỉnh số đầu tiên
            if (numbers != null && numbers.Length > 0)
            {
                // Giới hạn số đầu tiên chỉ là 0 hoặc 1
                if (numbers[0] > firstDigitMax)
                {
                    Debug.LogWarning($"Số đầu tiên {numbers[0]} vượt quá giới hạn {firstDigitMax}. Đã điều chỉnh thành {firstDigitMax}.");
                    numbers[0] = firstDigitMax;
                }
                
                // Giới hạn các số còn lại từ 0-9
                for (int i = 1; i < numbers.Length; i++)
                {
                    if (numbers[i] > maxDigit)
                    {
                        Debug.LogWarning($"Số thứ {i + 1} ({numbers[i]}) vượt quá giới hạn {maxDigit}. Đã điều chỉnh thành {maxDigit}.");
                        numbers[i] = maxDigit;
                    }
                    if (numbers[i] < 0)
                    {
                        numbers[i] = 0;
                    }
                }
            }
            
            playerNumbers = numbers;
            
            // clear UI cũ
            foreach (Transform child in gachaParent) Destroy(child.gameObject);
            foreach (Transform child in selectedParent) Destroy(child.gameObject);
            gachaTexts.Clear();
            selectedTexts.Clear();
            
            // tạo slot cho gacha & selected
            for (int i = 0; i < numbers.Length; i++)
            {
                // --- selected slot ---
                GameObject selObj = Instantiate(numberPrefab, selectedParent);
                var selText = selObj.GetComponent<TextMeshProUGUI>() ??
                              selObj.GetComponentInChildren<TextMeshProUGUI>();
                if (selText != null)
                {
                    selText.text = numbers[i].ToString();
                    selectedTexts.Add(selText);
                }
                
                // --- gacha slot ---
                GameObject gachObj = Instantiate(numberPrefab, gachaParent);
                var gachText = gachObj.GetComponent<TextMeshProUGUI>() ??
                               gachObj.GetComponentInChildren<TextMeshProUGUI>();
                if (gachText != null)
                {
                    gachText.text = "_"; // ban đầu để trống
                    gachaTexts.Add(gachText);
                }
            }
            
            if (startButton != null)
            {
                startButton.interactable = true;
                startButton.onClick.RemoveAllListeners();
                startButton.onClick.AddListener(() => StartGacha());
            }
            if (exitButton != null)
                exitButton.interactable = false;
        }
        
        // Phương thức helper để random số theo vị trí
        public int GetRandomNumberForPosition(int position)
        {
            if (position == 0)
            {
                // Số đầu tiên chỉ từ 0-1
                return Random.Range(0, firstDigitMax + 1);
            }
            else
            {
                // Các số còn lại từ 0-9
                return Random.Range(0, maxDigit + 1);
            }
        }
    }
}
