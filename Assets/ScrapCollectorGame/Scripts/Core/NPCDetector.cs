using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class NPCDetector : MonoBehaviour
{
    [Header("NPC Interaction Settings")]
    private IInteractable currentNPC = null;
    public GameObject talkIcon;

    [Header("NPC Detection Settings")]
    public LayerMask npcLayerMask = -1;
    public string npcTag = "NPC";

    void Start()
    {
        if (talkIcon != null)
            talkIcon.SetActive(false);
    }

    public void OnTalk(InputAction.CallbackContext context)
    {
        if (context.performed && currentNPC != null && currentNPC.CanInteract())
        {
            currentNPC.Interact();
            Debug.Log("Talked to NPC: " + ((MonoBehaviour)currentNPC).gameObject.name);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsValidNPC(other)) return;

        if (other.TryGetComponent(out IInteractable interactable))
        {
            currentNPC = interactable;

            // Hiện talk icon
            if (talkIcon != null)
                talkIcon.SetActive(true);

            // Hiện tên NPC
            NPCNameComponent nameComponent = other.GetComponent<NPCNameComponent>();
            if (nameComponent != null)
            {
                nameComponent.ShowName();
            }

            Debug.Log("Entered NPC range: " + other.gameObject.name);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (currentNPC != null && other.GetComponent<IInteractable>() == currentNPC)
        {
            Debug.Log("Exited NPC range: " + other.gameObject.name);

            // Ẩn tên NPC (nếu không muốn hiện lúc nào cũng hiện)
            NPCNameComponent nameComponent = other.GetComponent<NPCNameComponent>();
            if (nameComponent != null && !nameComponent.showNameOnStart)
            {
                nameComponent.HideName();
            }

            currentNPC = null;
            if (talkIcon != null)
                talkIcon.SetActive(false);
        }
    }

    private bool IsValidNPC(Collider2D collider)
    {
        if ((npcLayerMask.value & (1 << collider.gameObject.layer)) == 0)
            return false;
        if (!string.IsNullOrEmpty(npcTag) && !collider.CompareTag(npcTag))
            return false;
        return true;
    }

    public bool HasNPCInRange()
    {
        return currentNPC != null;
    }

    public IInteractable GetCurrentNPC()
    {
        return currentNPC;
    }
}