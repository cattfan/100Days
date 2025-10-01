using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NumberInputSystem
{
    [System.Serializable]
    public class RewardTier
    {
        public string name;                 // Tên giải thưởng
        public int requiredMatches;         // Số lượng số trúng cần thiết
        public bool requireOrder;           // Có cần đúng thứ tự không
        public string rewardMessage;        // Thông báo trúng thưởng
        public Color highlightColor = Color.green; // Màu highlight
        public int coinReward;              // ✅ Số coin thưởng
    }

    public partial class GachaManager : MonoBehaviour
    {
        [Header("Reward System")]
        public TextMeshProUGUI rewardText;
        public RewardTier[] rewardTiers = new RewardTier[]
         {
            new RewardTier { name = "Trúng 3 Số", requiredMatches = 3, requireOrder = true, rewardMessage = "Chúc mừng! Bạn đã trúng 3 số đúng thứ tự!", coinReward = 999},
            new RewardTier { name = "Trúng 2 Số", requiredMatches = 2, requireOrder = true, rewardMessage = "Tốt lắm! Bạn đã trúng 2 số đúng thứ tự!", coinReward = 555 },
            new RewardTier { name = "Trúng 1 Số", requiredMatches = 1, requireOrder = true, rewardMessage = "Hay lắm! Bạn đã trúng 1 số!", coinReward = 111 }
         };


        [Header("Gacha Control Settings")]
        public float rollSpeed = 0.05f;
        public float rollDuration = 3f;
        private int slotsFinished = 0;

        private PlayerInput playerInput;

        // ✅ Hàm random chữ số cho từng vị trí (giới hạn 0-200)
        private int GetRandomDigitForPosition(int position, int totalDigits)
        {
            if (position == 0 && totalDigits == 3)
            {
                // Chữ số đầu tiên chỉ từ 0-1 (cho số 0-199)
                return Random.Range(0, 2);
            }
            else
            {
                // Các chữ số còn lại từ 0-9
                return Random.Range(0, 10);
            }
        }

        public void StartGacha()
        {
            if (exitButton != null)
                exitButton.interactable = false;
            if (startButton != null)
                startButton.interactable = false;
            playerInput = FindFirstObjectByType<PlayerInput>();

            slotsFinished = 0;
            HideRewardMessage();

            // Reset màu của tất cả slots
            ResetSlotColors();

            for (int i = 0; i < gachaTexts.Count; i++)
            {
                if (gachaTexts[i] != null)
                    StartCoroutine(RollSlot(gachaTexts[i], i));
            }
        }

        IEnumerator RollSlot(TextMeshProUGUI textObj, int slotIndex)
        {
            float elapsed = 0f;
            int totalDigits = playerNumbers.Length;

            while (elapsed < rollDuration)
            {
                elapsed += rollSpeed;
                // ✅ Random chữ số phù hợp với vị trí (0-1 cho vị trí đầu, 0-9 cho các vị trí khác)
                int randomDigit = GetRandomDigitForPosition(slotIndex, totalDigits);
                textObj.text = randomDigit.ToString();
                yield return new WaitForSeconds(rollSpeed);
            }

            // ✅ Chốt số cuối cùng theo quy tắc giới hạn
            int finalDigit = GetRandomDigitForPosition(slotIndex, totalDigits);
            textObj.text = finalDigit.ToString();
            slotsFinished++;

            // Khi tất cả slot đã dừng
            if (slotsFinished >= gachaTexts.Count)
            {
                yield return new WaitForSeconds(0.5f); // Chờ một chút trước khi kiểm tra
                CheckReward();

                if (exitButton != null)
                {
                    exitButton.interactable = true;
                    exitButton.onClick.RemoveAllListeners();
                    exitButton.onClick.AddListener(OnExit);
                }
            }
        }

        void CheckReward()
        {
            if (playerNumbers == null || gachaTexts.Count == 0) return;

            // Lấy kết quả gacha
            int[] gachaResults = new int[gachaTexts.Count];
            for (int i = 0; i < gachaTexts.Count; i++)
            {
                if (int.TryParse(gachaTexts[i].text, out int result))
                {
                    gachaResults[i] = result;
                }
            }

            Debug.Log($"Player numbers: [{string.Join(", ", playerNumbers)}]");
            Debug.Log($"Gacha results: [{string.Join(", ", gachaResults)}]");

            // Kiểm tra từ mức cao nhất xuống thấp nhất (3 số -> 2 số -> 1 số)
            foreach (var tier in rewardTiers)
            {
                if (CheckOrderedMatch(playerNumbers, gachaResults, tier.requiredMatches))
                {
                    ShowReward(tier);
                    return;
                }
            }

            // Không trúng gì cả
            ShowNoReward();
        }

        bool CheckOrderedMatch(int[] playerNumbers, int[] gachaResults, int requiredMatches)
        {
            int maxConsecutive = 0;
            int bestStartIndex = -1;

            // Tìm chuỗi liên tiếp dài nhất
            for (int start = 0; start <= playerNumbers.Length - requiredMatches; start++)
            {
                int consecutiveCount = 0;

                for (int i = start; i < Mathf.Min(playerNumbers.Length, gachaResults.Length); i++)
                {
                    if (playerNumbers[i] == gachaResults[i])
                    {
                        consecutiveCount++;
                    }
                    else
                    {
                        break; // Dừng khi không trúng
                    }
                }

                if (consecutiveCount >= requiredMatches && consecutiveCount > maxConsecutive)
                {
                    maxConsecutive = consecutiveCount;
                    bestStartIndex = start;
                }
            }

            if (maxConsecutive >= requiredMatches)
            {
                // Highlight các số trúng liên tiếp
                List<int> matchedIndices = new List<int>();
                for (int i = bestStartIndex; i < bestStartIndex + maxConsecutive; i++)
                {
                    matchedIndices.Add(i);
                }
                HighlightMatchedSlots(matchedIndices, Color.green);
                return true;
            }

            return false;
        }

        void HighlightMatchedSlots(List<int> indices, Color highlightColor)
        {
            foreach (int index in indices)
            {
                if (index >= 0 && index < gachaTexts.Count && gachaTexts[index] != null)
                {
                    var textObj = gachaTexts[index];
                    var sleObj = selectedTexts[index];

                    // 🔹 Lấy Image ở GameObject cha (Slot prefab)
                    var image = textObj.transform.parent.GetComponent<Image>();
                    var selImage = sleObj.transform.parent.GetComponent<Image>();

                    if (image != null) image.color = highlightColor;
                    if (selImage != null) selImage.color = highlightColor;

                    // 🔹 Đổi luôn màu chữ
                    if (textObj != null) textObj.color = highlightColor;
                    if (sleObj != null) sleObj.color = highlightColor;
                }
            }
        }


        void ResetSlotColors()
        {
            for (int i = 0; i < gachaTexts.Count; i++)
            {
                var textObj = gachaTexts[i];
                var selObj = selectedTexts[i];

                if (textObj != null)
                {
                    var image = textObj.transform.parent.GetComponent<Image>();
                    if (image != null) image.color = Color.white; // Nền mặc định
                    textObj.color = Color.black; // ✅ reset màu chữ về đen (hoặc màu bạn muốn)
                }

                if (selObj != null)
                {
                    var selImage = selObj.transform.parent.GetComponent<Image>();
                    if (selImage != null) selImage.color = Color.white;
                    selObj.color = Color.black; // ✅ reset màu chữ
                }
            }
        }



        void ShowReward(RewardTier tier)
        {
            Debug.Log($"Trúng thưởng: {tier.name}");

            if (rewardText != null)
            {
                rewardText.text = tier.rewardMessage;
            }

            ShowRewardMessage();
            SaveController saveController = FindFirstObjectByType<SaveController>();
            if (saveController != null && saveController.currencyManager != null)
            {
                saveController.currencyManager.AddCoins(tier.coinReward);
                Debug.Log($"Added {tier.coinReward} coins via SaveController's CurrencyManager");
            }
            else
            {
                Debug.LogError("Could not find SaveController or its CurrencyManager!");
            }
        }

        void ShowNoReward()
        {
            Debug.Log("Không trúng thưởng");

            if (rewardText != null)
            {
                rewardText.text = "Chúc bạn may mắn lần sau!";
            }

            ShowRewardMessage();
        }

        public void ShowRewardMessage()
        {
            if (RewardMessage != null)
                RewardMessage.SetActive(true);
        }

        public void HideRewardMessage()
        {
            if (RewardMessage != null)
                RewardMessage.SetActive(false);
        }

        public void OnExit()
        {
            gameObject.SetActive(false);
            slotsFinished = 0;
            selectedTexts.Clear();
            playerNumbers = null;
            HideRewardMessage();

            if (exitButton != null)
                exitButton.interactable = false;
            if (startButton != null)
                startButton.interactable = false;
            if (playerInput != null)
                playerInput.enabled = true;
        }
    }
}