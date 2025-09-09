using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("Drag Settings")]
    public Transform originalParent;
    public bool droppedOnValidSlot = false;

    private CanvasGroup canvasGroup;
    private Canvas parentCanvas;
    private RectTransform inventoryPanelRect;
    private AudioManagement audioManager;
    private Vector3 startPosition;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        parentCanvas = GetComponentInParent<Canvas>();
        audioManager = Object.FindFirstObjectByType<AudioManagement>();

        var inventoryController = Object.FindFirstObjectByType<InventoryManager>();
        if (inventoryController != null)
            inventoryPanelRect = inventoryController.GetInventoryPanel();
    }

    private void Start()
    {
        if (originalParent == null)
            originalParent = transform.parent;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"[DRAG] Begin drag: {gameObject.name}");

        originalParent = transform.parent;
        startPosition = transform.position;
        droppedOnValidSlot = false;

        // Disable raycasts để có thể drop
        canvasGroup.blocksRaycasts = false;

        // Move to canvas level để render trên tất cả UI
        transform.SetParent(parentCanvas.transform);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Follow mouse
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"[DRAG] End drag: {gameObject.name}, dropped on valid slot: {droppedOnValidSlot}");

        canvasGroup.blocksRaycasts = true;

        if (!droppedOnValidSlot)
        {
            bool isInsideInventory = IsInsideInventoryPanel(eventData.position);

            if (isInsideInventory)
            {
                // Return to original position
                ReturnToOriginal();
                Debug.Log("[DRAG] Returned to original slot");
            }
            else
            {
                // Destroy if dropped outside
                var originalSlot = originalParent.GetComponent<Slot>();
                if (originalSlot != null)
                    originalSlot.currentItem = null;

                Debug.Log("[DRAG] Item destroyed - dropped outside inventory");
                Destroy(gameObject);
                audioManager?.PlaySFX(audioManager.DropItem);
            }
        }
        else
        {
            Debug.Log("[DRAG] Successfully placed in new slot");
            audioManager?.PlaySFX(audioManager.PlaceItem);
        }

        droppedOnValidSlot = false; // Reset flag
    }

    private void ReturnToOriginal()
    {
        transform.SetParent(originalParent);
        transform.localPosition = Vector3.zero;

        var slot = originalParent.GetComponent<Slot>();
        if (slot != null)
            slot.currentItem = gameObject;
    }

    private bool IsInsideInventoryPanel(Vector2 screenPosition)
    {
        if (inventoryPanelRect == null) return true;

        return RectTransformUtility.RectangleContainsScreenPoint(
            inventoryPanelRect,
            screenPosition,
            parentCanvas.worldCamera
        );
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Only handle clicks for shop mode
        var slot = GetComponentInParent<Slot>();
        var controller = Object.FindFirstObjectByType<InventoryInteraction>();

        if (controller != null && slot != null && controller.shopMode)
        {
            controller.HandleSlotClick(slot, eventData.button);
        }
    }

    public void UpdateOriginalParent(Transform newParent)
    {
        originalParent = newParent;
    }
}