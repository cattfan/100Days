using UnityEngine;
using UnityEngine.UI;

public class TabsController : MonoBehaviour
{
    [Header("Panel")]
    public Image panelImage;
    public Sprite panelActiveSprite;
    public Sprite panelInactiveSprite;

    [Header("Tab Icons")]
    public Image inventoryIcon;
    public Sprite inventoryActive;
    public Sprite inventoryInactive;

    public Image minimapIcon;
    public Sprite minimapActive;
    public Sprite minimapInactive;

    public Image optionIcon;
    public Sprite optionActive;
    public Sprite optionInactive;

    [Header("Tab Pages")]
    public GameObject inventoryPage;
    public GameObject minimapPage;
    public GameObject optionPage;

    public AudioManagement audioManagement;

    private enum Tab { None, Inventory, Minimap, Option }
    private Tab currentTab = Tab.None;

    private void Awake()
    {
        GameObject audioObject = GameObject.FindGameObjectWithTag("Audio");
        if (audioObject != null)
        {
            audioManagement = audioObject.GetComponent<AudioManagement>();
        }
    }

    public void OpenInventoryTab()
    {
        SetActiveTab(Tab.Inventory);
    }

    public void OpenMinimapTab()
    {
        SetActiveTab(Tab.Minimap);
    }

    public void OpenOptionTab()
    {
        SetActiveTab(Tab.Option);
    }

    private void SetActiveTab(Tab tab)
    {
        currentTab = tab;

        // Đặt sprite panel active
        panelImage.sprite = panelActiveSprite;

        // Reset tất cả icon về Inactive
        inventoryIcon.sprite = inventoryInactive;
        minimapIcon.sprite = minimapInactive;
        optionIcon.sprite = optionInactive;

        // Ẩn tất cả page
        inventoryPage.SetActive(false);
        minimapPage.SetActive(false);
        optionPage.SetActive(false);

        // Set tab và page tương ứng
        switch (currentTab)
        {
            case Tab.Inventory:
                inventoryIcon.sprite = inventoryActive;
                inventoryPage.SetActive(true);
                break;
            case Tab.Minimap:
                minimapIcon.sprite = minimapActive;
                minimapPage.SetActive(true);
                break;
            case Tab.Option:
                optionIcon.sprite = optionActive;
                optionPage.SetActive(true);
                break;
        }

        // Play sound
        if (audioManagement != null)
        {
            audioManagement.PlaySFX(audioManagement.ButtonClick);
        }
    }

    public void CloseTab()
    {
        currentTab = Tab.None;

        // Panel inactive
        panelImage.sprite = panelInactiveSprite;

        // Tất cả icon inactive
        inventoryIcon.sprite = inventoryInactive;
        minimapIcon.sprite = minimapInactive;
        optionIcon.sprite = optionInactive;

        // Ẩn tất cả page
        inventoryPage.SetActive(false);
        minimapPage.SetActive(false);
        optionPage.SetActive(false);

        if (audioManagement != null)
        {
            audioManagement.PlaySFX(audioManagement.CloseMenu);
        }
    }
}
