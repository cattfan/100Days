using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [Header("Menus")]
    public GameObject mainMenu;          // giao diện menu chính
    public GameObject optionMenu;        // giao diện option setting

    private void Start()
    {
        // Khi game bắt đầu: MainMenu bật, OptionMenu tắt
        mainMenu.SetActive(true);
        optionMenu.SetActive(false);
    }

    public void PlayGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Maingame");
        Time.timeScale = 1f;
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
}
