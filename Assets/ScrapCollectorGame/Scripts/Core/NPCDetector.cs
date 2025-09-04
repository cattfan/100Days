using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class NPCDetector : MonoBehaviour
{
    [Header("NPC Interaction Settings")]
    private IInteractable currentNPC = null;
    public GameObject talkIcon;

    [Header("NPC Detection Settings")]
    public LayerMask npcLayerMask = -1; // Layer for NPCs
    public string npcTag = "NPC";       // Tag for NPCs

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
            currentNPC.Interact();
            UpdateTalkIcon();

            // Log for debugging
            Debug.Log("Talked to NPC: " + ((MonoBehaviour)currentNPC).gameObject.name);
        }
    }

    // Use OnTriggerEnter to find the NPC
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsValidNPC(other)) return;

        if (other.TryGetComponent(out IInteractable interactable))
        {
            currentNPC = interactable;
            UpdateTalkIcon();

            Debug.Log("Entered NPC range: " + other.gameObject.name);
        }
    }

    // Use OnTriggerExit to clear the NPC
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

        if (currentNPC != null && currentNPC.CanInteract())
        {
            talkIcon.SetActive(true);
        }
        else
        {
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