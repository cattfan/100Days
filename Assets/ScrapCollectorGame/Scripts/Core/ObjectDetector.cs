using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectDetector : MonoBehaviour
{
    [Header("Object Interaction Settings")]
    private IInteractable currentObject = null;
    public GameObject interactIcon;

    [Header("Object Detection Settings")]
    public LayerMask objectLayerMask = -1; // Layer của objects
    public string objectTag = "InteractableObject"; // Tag của objects

    void Start()
    {
        if (interactIcon != null)
            interactIcon.SetActive(false);
    }

    void Update()
    {
        UpdateInteractionIcon();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed && currentObject != null && currentObject.CanInteract())
        {
            currentObject.Interact();
            UpdateInteractionIcon();

            // Log cho debug
            Debug.Log("Interacted with object: " + ((MonoBehaviour)currentObject).gameObject.name);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra layer và tag
        if (!IsValidObject(other)) return;

        if (other.TryGetComponent(out IInteractable interactable))
        {
            currentObject = interactable;
            UpdateInteractionIcon();

            Debug.Log("Entered object range: " + other.gameObject.name);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (currentObject != null && other.GetComponent<IInteractable>() == currentObject)
        {
            Debug.Log("Exited object range: " + other.gameObject.name);

            currentObject = null;
            if (interactIcon != null)
                interactIcon.SetActive(false);
        }
    }

    private void UpdateInteractionIcon()
    {
        if (interactIcon == null) return;

        if (currentObject != null)
        {
            bool canInteract = currentObject.CanInteract();
            interactIcon.SetActive(canInteract);
        }
        else
        {
            interactIcon.SetActive(false);
        }
    }

    // Kiểm tra xem object có hợp lệ không
    private bool IsValidObject(Collider2D collider)
    {
        // Kiểm tra layer
        if ((objectLayerMask.value & (1 << collider.gameObject.layer)) == 0)
            return false;

        // Kiểm tra tag nếu được thiết lập
        if (!string.IsNullOrEmpty(objectTag) && !collider.CompareTag(objectTag))
            return false;

        return true;
    }

    // Public methods để các script khác có thể truy cập
    public bool HasObjectInRange()
    {
        return currentObject != null;
    }

    public IInteractable GetCurrentObject()
    {
        return currentObject;
    }
}