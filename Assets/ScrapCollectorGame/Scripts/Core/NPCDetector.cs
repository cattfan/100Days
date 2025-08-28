using UnityEngine;
using UnityEngine.InputSystem;

public class NPCDetector : MonoBehaviour
{
    [Header("NPC Interaction Settings")]
    private IInteractable currentNPC = null;
    public GameObject talkIcon;

    [Header("NPC Detection Settings")]
    public LayerMask npcLayerMask = -1; // Layer của NPCs
    public string npcTag = "NPC";       // Tag của NPCs

    void Start()
    {
        if (talkIcon != null)
            talkIcon.SetActive(false);
    }

    void Update()
    {
        UpdateTalkIcon();
    }

    public void OnTalk(InputAction.CallbackContext context)
    {
        if (context.performed && currentNPC != null && currentNPC.CanInteract())
        {
            // Gọi hành động nói chuyện
            currentNPC.Interact();
            UpdateTalkIcon();

            // Log để debug
            Debug.Log("Talked to NPC: " + ((MonoBehaviour)currentNPC).gameObject.name);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra layer và tag
        if (!IsValidNPC(other)) return;

        if (other.TryGetComponent(out IInteractable interactable))
        {
            currentNPC = interactable;
            UpdateTalkIcon();

            Debug.Log("Entered NPC range: " + other.gameObject.name);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (currentNPC != null && other.GetComponent<IInteractable>() == currentNPC)
        {
            Debug.Log("Exited NPC range: " + other.gameObject.name);

            currentNPC = null;
            if (talkIcon != null)
                talkIcon.SetActive(false);
        }
    }

    private void UpdateTalkIcon()
    {
        if (talkIcon == null) return;

        if (currentNPC != null)
        {
            bool canTalk = currentNPC.CanInteract();
            talkIcon.SetActive(canTalk);
        }
        else
        {
            talkIcon.SetActive(false);
        }
    }

    // Kiểm tra xem NPC có hợp lệ không
    private bool IsValidNPC(Collider2D collider)
    {
        // Kiểm tra layer
        if ((npcLayerMask.value & (1 << collider.gameObject.layer)) == 0)
            return false;

        // Kiểm tra tag nếu được thiết lập
        if (!string.IsNullOrEmpty(npcTag) && !collider.CompareTag(npcTag))
            return false;

        return true;
    }

    // Public methods để các script khác có thể truy cập
    public bool HasNPCInRange()
    {
        return currentNPC != null;
    }

    public IInteractable GetCurrentNPC()
    {
        return currentNPC;
    }
}
