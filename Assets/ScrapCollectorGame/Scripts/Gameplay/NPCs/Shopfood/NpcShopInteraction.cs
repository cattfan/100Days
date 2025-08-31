using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class NpcShopInteraction : MonoBehaviour
{
    private bool playerIsNear = false;
    private InputAction interactAction;
    private bool isShopOpen = false;
    public string shopSceneName = "Shop";
    public ShopData shopItemsData;

    private EventSystem mainEventSystem;
    private PlayerInput playerInput;

    private void Awake()
    {
        interactAction = new InputAction(binding: "<Keyboard>/e");
        interactAction.performed += OnInteract;

        // Thay thế FindObjectOfType bằng FindAnyObjectByType
        mainEventSystem = FindAnyObjectByType<EventSystem>();
        playerInput = FindAnyObjectByType<PlayerInput>();
    }

    private void OnEnable()
    {
        interactAction.Enable();
    }

    private void OnDisable()
    {
        interactAction.Disable();
        interactAction.performed -= OnInteract;
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (playerIsNear)
        {
            if (!isShopOpen)
            {
                StartCoroutine(LoadAndOpenShop());
            }
            else
            {
                UnloadShopScene();
            }
        }
    }

    IEnumerator LoadAndOpenShop()
    {
        if (playerInput != null)
        {
            playerInput.enabled = false;
        }
        if (mainEventSystem != null)
        {
            mainEventSystem.gameObject.SetActive(false);
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(shopSceneName, LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        Scene shopScene = SceneManager.GetSceneByName(shopSceneName);
        if (shopScene.isLoaded)
        {
            GameObject[] rootObjects = shopScene.GetRootGameObjects();
            foreach (GameObject obj in rootObjects)
            {
                ShopUIController shopUIController = obj.GetComponentInChildren<ShopUIController>();
                if (shopUIController != null)
                {
                    shopUIController.SetShopData(shopItemsData);
                    obj.SetActive(true);
                    RectTransform rectTransform = obj.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.anchoredPosition3D = Vector3.zero;
                    }

                    // Tìm EventSystem của scene shop
                    EventSystem shopEventSystem = obj.GetComponentInChildren<EventSystem>();
                    if (shopEventSystem == null)
                    {
                        GameObject eventSystemObj = new GameObject("EventSystem");
                        eventSystemObj.AddComponent<EventSystem>();
                        eventSystemObj.AddComponent<InputSystemUIInputModule>();
                        eventSystemObj.transform.SetParent(obj.transform);
                        shopEventSystem = eventSystemObj.GetComponent<EventSystem>();
                    }
                    if (shopEventSystem != null)
                    {
                        shopEventSystem.gameObject.SetActive(true);
                    }

                    break;
                }
            }
        }
        isShopOpen = true;
    }

    void UnloadShopScene()
    {
        SceneManager.UnloadSceneAsync(shopSceneName);
        isShopOpen = false;

        if (playerInput != null)
        {
            playerInput.enabled = true;
        }
        if (mainEventSystem != null)
        {
            mainEventSystem.gameObject.SetActive(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = false;
        }
    }
}