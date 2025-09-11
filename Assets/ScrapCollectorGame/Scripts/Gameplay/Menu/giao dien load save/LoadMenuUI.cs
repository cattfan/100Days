using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class LoadGameMenu : MonoBehaviour
{
    public SaveController saveController;
    public MenuManager menuManager; // Tham chiếu tới MenuManager
    public GameObject buttonPrefab;
    public Transform buttonContainer;
    public GameObject deleteConfirmationPanel;
    public GameObject DeleteButton; // Nút để bật/tắt chế độ x

    [Header("Delete Confirmation UI")]
    public TMP_Text deleteConfirmationText; // 👉 Thêm TMP_Text để hiển thị nội dung xác nhận
    public TMP_Text TMPTitle ; // Tiêu đề gốc của nút Delete

    private bool isDeleteMode = false;
    private string playerNameToDelete;
    private int coinsToDelete; // 👉 Lưu số coin để hiển thị cùng
    private string Title = "Load Save Game"; // Lưu tiêu đề gốc của nút Delete

    void OnEnable()
    {
        PopulateSaveButtons();
        isDeleteMode = false;
        deleteConfirmationPanel.SetActive(false);
        TMPTitle.text = Title; // Khôi phục tiêu đề gốc khi mở menu

    }

    void PopulateSaveButtons()
    {
        Debug.Log("PopulateSaveButtons called");

        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        List<(string playerName, int coins)> saves = saveController.GetAllSaveSummaries();
        Debug.Log($"Found {saves.Count} save files");

        foreach (var save in saves)
        {
            Debug.Log($"Creating button for: {save.playerName} - {save.coins} coins");

            GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);
            Button btn = btnObj.GetComponent<Button>();
            TMP_Text txt = btnObj.GetComponentInChildren<TMP_Text>();
            txt.text = $"{save.playerName} - {save.coins} coins";

            string playerNameCopy = save.playerName;
            int coinsCopy = save.coins;

            btn.onClick.AddListener(() =>
            {
                if (isDeleteMode)
                {
                    PromptDelete(playerNameCopy, coinsCopy);
                }
                else
                {
                    Debug.Log($"Loading game for player: {playerNameCopy}");
                    PlayerPrefs.SetString("SaveToLoad", playerNameCopy);
                    PlayerPrefs.Save();
                    Debug.Log($"PlayerPrefs set: SaveToLoad = '{playerNameCopy}'");
                    Debug.Log("Loading MainGame scene...");
                    SceneManager.LoadScene("Maingame");
                }
            });
        }

        if (saves.Count == 0)
        {
            Debug.Log("No save files found");
        }
    }

    public void ToggleDeleteMode()
    {
        isDeleteMode = !isDeleteMode;
        TMPTitle.text = isDeleteMode ? "Delete Save Game" : Title; // Thay đổi tiêu đề nút
    }

    // 👉 Cập nhật hàm để truyền thêm coins
    public void PromptDelete(string playerName, int coins)
    {
        playerNameToDelete = playerName;
        coinsToDelete = coins;

        // Hiển thị thông báo xác nhận
        if (deleteConfirmationText != null)
        {
            deleteConfirmationText.text = $"Bạn có chắc muốn xóa save \"{playerNameToDelete} - {coinsToDelete} coins\" không?";
        }
        deleteConfirmationPanel.SetActive(true);
    }

    public void ConfirmDelete()
    {
        saveController.DeleteSaveFile(playerNameToDelete);
        isDeleteMode = false;
        deleteConfirmationPanel.SetActive(false);
        PopulateSaveButtons();
    }

    public void CancelDelete()
    {
        isDeleteMode = false;
        deleteConfirmationPanel.SetActive(false);
    }

}
