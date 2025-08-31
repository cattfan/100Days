using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

public class NpcShopInteraction : MonoBehaviour
{
    private bool playerIsNear = false;
    private InputAction interactAction;
    private bool isShopOpen = false;
    public string shopSceneName = "Shop";

    private void Awake()
    {
        interactAction = new InputAction(binding: "<Keyboard>/e");
        interactAction.performed += OnInteract;
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
                // Tải scene shop lên khi chưa mở
                StartCoroutine(LoadAndEnableShop());
            }
            else
            {
                // Dỡ scene shop khi đã mở
                UnloadShopScene();
            }
        }
    }

    IEnumerator LoadAndEnableShop()
    {
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
                if (obj.GetComponent<Canvas>() != null)
                {
                    // Bật Canvas
                    obj.SetActive(true);

                    // Đặt vị trí của Rect Transform về 0 để Canvas hiển thị ở trung tâm màn hình
                    RectTransform rectTransform = obj.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.anchoredPosition3D = Vector3.zero;
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