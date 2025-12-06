using UnityEngine;

public interface IInteractable
{
    void Interact();
    
    /// <summary>
    /// Priority level for this interaction. Lower values = higher priority.
    /// 0 = Highest (critical story/dialog), 10 = Lowest (generic items)
    /// </summary>
    int GetInteractionPriority();
    
    /// <summary>
    /// Check if this interaction is currently valid/available
    /// </summary>
    bool CanInteract();
    
    /// <summary>
    /// Whether to show the interaction prompt (E icon) for this interactable.
    /// Return false for invisible interactions like doors/teleports.
    /// </summary>
    bool ShowInteractionPrompt();
}
