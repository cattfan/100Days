using UnityEngine;
using TMPro;
using System;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    [Header("Menus")]
    public GameObject mainMenu;           // giao diện menu chính
    public GameObject optionMenu;         // giao diện option setting
    public GameObject loadGameMenu;       // giao diện load game
    public GameObject newGameMenu;   // giao diện tạo game mới
    public TMP_Text warningText;
    public TMP_InputField playerNameInput;

    [Header("References")]
    public SaveController saveController; // Tham chiếu đến SaveController

    private void Start()
    {
        // Khi game bắt đầu: các menu khác đều tắt
        mainMenu.SetActive(true);
        optionMenu.SetActive(false);
        loadGameMenu.SetActive(false);
        newGameMenu.SetActive(false);
        warningText.text = "";

        if (saveController == null)
        {
            saveController = FindFirstObjectByType<SaveController>();
        }
    }
    public void StartNewGameFromUI()
    {
        if (playerNameInput != null)
        {
            StartNewGame(playerNameInput.text);
        }
    }

    public void PlayGame()
    {
        // Khi bấm Play Game, mở giao diện tạo game mới
        newGameMenu.SetActive(true);
        mainMenu.SetActive(false);
    }

    public void StartNewGame(string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName))
        {
            Debug.LogError("Player name cannot be empty!");
            // Hiển thị thông báo lỗi trên UI cho người chơi nếu cần
            return;
        }

        // Kiểm tra xem đã có save file với tên này chưa
        if (saveController.HasSaveFile(playerName))
        {
            warningText.text = "Tên này đã có chủ hãy thêm hậu tố 1,2,3";
            StartCoroutine(FadeOutText(warningText, 3f));
            return;
        }

        // Ghi lại tên player cần load (tên mới)
        PlayerPrefs.SetString("SaveToLoad", playerName);

        // Chuyển sang Scene MainGame
        UnityEngine.SceneManagement.SceneManager.LoadScene("Maingame");
        Time.timeScale = 1f;
    }

    private IEnumerator FadeOutText(TMP_Text warningText, float duration)
    {
        if (warningText == null) yield break;

        Color originalColor = warningText.color;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            warningText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        warningText.text = "";
        warningText.color = originalColor;
    }

    public void SettingGame()
    {
        mainMenu.SetActive(false);
        optionMenu.SetActive(true);
    }

    public void LoadGameFromMenu(string playerName)
    {
        // Ghi lại tên player cần load
        PlayerPrefs.SetString("SaveToLoad", playerName);

        // Chuyển sang Scene MainGame
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainGame");
    }

    public void BackToMainMenu()
    {
        optionMenu.SetActive(false);
        loadGameMenu.SetActive(false);
        newGameMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OpenLoadGameMenu()
    {
        loadGameMenu.SetActive(true);
        mainMenu.SetActive(false);
    }

    public void CloseLoadGameMenu()
    {
        loadGameMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void CloseNewGameMenu()
    {
        newGameMenu.SetActive(false);
        mainMenu.SetActive(true);
    }
}
