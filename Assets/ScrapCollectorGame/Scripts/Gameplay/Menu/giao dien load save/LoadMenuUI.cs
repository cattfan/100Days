using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class LoadGameMenu : MonoBehaviour
{
    public SaveController saveController;
    public MenuManager menuManager;
    public GameObject buttonPrefab;
    public Transform buttonContainer;
    public GameObject deleteConfirmationPanel;
    public GameObject DeleteButton;

    [Header("Delete Confirmation UI")]
    public TMP_Text deleteConfirmationText;
    public TMP_Text TMPTitle;

    private bool isDeleteMode = false;
    private string playerNameToDelete;
    private int coinsToDelete;
    private string Title = "Load Save Game";

    void OnEnable()
    {
        PopulateSaveButtons();
        isDeleteMode = false;
        deleteConfirmationPanel.SetActive(false);
        TMPTitle.text = Title;

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
        TMPTitle.text = isDeleteMode ? "Delete Save Game" : Title;
    }

    public void PromptDelete(string playerName, int coins)
    {
        playerNameToDelete = playerName;
        coinsToDelete = coins;

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
