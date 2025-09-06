using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public Transform originalParent { get; private set; }
    private RectTransform inventoryPanelRect;
    private Canvas parentCanvas;
    private CanvasGroup canvasGroup;
    public bool droppedOnValidSlot = false;

    private AudioManagement audioManager;
    private ItemUI itemUI;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        parentCanvas = GetComponentInParent<Canvas>();

        var inventoryController = FindObjectOfType<InventoryController>();
        if (inventoryController != null)
            inventoryPanelRect = inventoryController.GetInventoryPanel();

        audioManager = FindObjectOfType<AudioManagement>();
        itemUI = GetComponent<ItemUI>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        droppedOnValidSlot = false;
        originalParent = transform.parent;

        var originalSlot = originalParent.GetComponent<Slot>();
        if (originalSlot != null)
            originalSlot.currentItem = null;

        if (parentCanvas != null)
            transform.SetParent(parentCanvas.transform, true);
        else
            transform.SetParent(transform.root, true);

        if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!droppedOnValidSlot)
        {
            bool isInsideInventoryPanel = IsInsideInventoryPanel(eventData.position, eventData.pressEventCamera);

            if (isInsideInventoryPanel)
            {
                transform.SetParent(originalParent);
                transform.localPosition = Vector3.zero;

                var originalSlot = originalParent.GetComponent<Slot>();
                if (originalSlot != null)
                    originalSlot.currentItem = gameObject;
            }
            else
            {
                var originalSlot = originalParent.GetComponent<Slot>();
                if (originalSlot != null)
                    originalSlot.currentItem = null;

                Destroy(gameObject);
                audioManager?.PlaySFX(audioManager.DropItem);
            }
        }
        else
        {
            audioManager?.PlaySFX(audioManager.PlaceItem);
        }

        if (canvasGroup != null) canvasGroup.blocksRaycasts = true;
    }

    private bool IsInsideInventoryPanel(Vector3 screenPosition, Camera camera)
    {
        if (inventoryPanelRect == null) return false;

        return RectTransformUtility.RectangleContainsScreenPoint(
            inventoryPanelRect,
            screenPosition,
            camera
        );
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var slot = GetComponentInParent<Slot>();
        var controller = FindObjectOfType<InventoryController>();
        controller.HandleSlotClick(slot, eventData.button);
    }
}
