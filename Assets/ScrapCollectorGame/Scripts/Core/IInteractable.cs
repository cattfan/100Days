public interface IInteractable
{
    // Method to handle interaction with the object
    void Interact();
    // Method to check if the object can be interacted with
    bool CanInteract();
    // Optional: Method to get the interaction message or prompt
}