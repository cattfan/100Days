using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace NumberInputSystem
{
    public partial class NumberInputManager : MonoBehaviour
    {
        [Header("Settings")]
        public int maxNumbers = 3; // Số lượng số tối đa có thể nhập (mặc định 3)

        private List<int> enteredNumbers = new List<int>(); // Danh sách số đã nhập

        // Controller logic
        void OnNumberButtonClicked(int number)
        {
            Debug.Log($"OnNumberButtonClicked called with number: {number}");
            Debug.Log($"Current entered numbers count: {enteredNumbers.Count}, Max: {maxNumbers}");

            if (enteredNumbers.Count < maxNumbers)
            {
                enteredNumbers.Add(number);
                UpdateDisplay();

                Debug.Log($"Đã nhập số: {number}. Total numbers: {enteredNumbers.Count}");
            }
            else
            {
                Debug.Log($"Đã đạt giới hạn {maxNumbers} số!");
            }
        }

        void OnDeleteButtonClicked()
        {
            if (enteredNumbers.Count > 0)
            {
                int removedNumber = enteredNumbers[enteredNumbers.Count - 1];
                enteredNumbers.RemoveAt(enteredNumbers.Count - 1);
                UpdateDisplay();

                Debug.Log($"Đã xóa số: {removedNumber}");
            }
            else
            {
                Debug.Log("Không có số nào để xóa!");
            }
        }

        void OnConfirmButtonClicked()
        {
            if (enteredNumbers.Count == maxNumbers)
            {
                ShowTargetUI();

                string numbersString = string.Join(", ", enteredNumbers.ToArray());
                Debug.Log($"Xác nhận với các số: {numbersString}");
            }
            else
            {
                Debug.Log($"Vui lòng nhập đủ {maxNumbers} số trước khi xác nhận!");
            }
        }

        // Các hàm tiện ích public
        public void ClearAllNumbers()
        {
            enteredNumbers.Clear();
            UpdateDisplay();
        }

        public int[] GetEnteredNumbers()
        {
            return enteredNumbers.ToArray();
        }

        public bool IsInputComplete()
        {
            return enteredNumbers.Count == maxNumbers;
        }
    }
}
