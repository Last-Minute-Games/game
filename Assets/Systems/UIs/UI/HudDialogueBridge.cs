using UnityEngine;
using cherrydev;

/// <summary>
/// Bridge to connect dialogue system external functions with HudInitializer
/// Attach this to the same GameObject as your DialogBehaviour or any GameObject in the scene
/// </summary>
public class HudDialogueBridge : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the DialogBehaviour component")]
    [SerializeField] private DialogBehaviour dialogBehaviour;
    
    [Tooltip("Reference to the HudInitializer component")]
    [SerializeField] private HudInitializer hudInitializer;
    
    [Tooltip("Auto-find components in scene if not assigned")]
    [SerializeField] private bool autoFindComponents = true;
    
    private void Awake()
    {
        if (autoFindComponents)
        {
            if (dialogBehaviour == null)
            {
                dialogBehaviour = FindObjectOfType<DialogBehaviour>();
                if (dialogBehaviour == null)
                {
                    Debug.LogError("[HudDialogueBridge] Could not find DialogBehaviour in scene!");
                    return;
                }
            }
            
            if (hudInitializer == null)
            {
                hudInitializer = FindObjectOfType<HudInitializer>();
                if (hudInitializer == null)
                {
                    Debug.LogError("[HudDialogueBridge] Could not find HudInitializer in scene!");
                    return;
                }
            }
        }
        
        // Bind external functions to the dialogue system
        // These function names can now be used in the "Func Name" field of Sentence Nodes
        dialogBehaviour.BindExternalFunction("Hudintializer", TriggerHudAnimation);
        dialogBehaviour.BindExternalFunction("HudInitializer", TriggerHudAnimation); // Alternative spelling
        dialogBehaviour.BindExternalFunction("ShowHud", ShowHudImmediately);
        dialogBehaviour.BindExternalFunction("HideHud", HideHudImmediately);
        
        Debug.Log("[HudDialogueBridge] External functions bound successfully");
    }
    
    /// <summary>
    /// Trigger the HUD initialization animation
    /// Bound to dialogue external function name: "Hudintializer" or "HudInitializer"
    /// </summary>
    private void TriggerHudAnimation()
    {
        if (hudInitializer != null)
        {
            Debug.Log("[HudDialogueBridge] Triggering HUD animation from dialogue");
            hudInitializer.TriggerAnimation();
        }
        else
        {
            Debug.LogError("[HudDialogueBridge] HudInitializer reference is null!");
        }
    }
    
    /// <summary>
    /// Show HUD immediately without animation
    /// Bound to dialogue external function name: "ShowHud"
    /// </summary>
    private void ShowHudImmediately()
    {
        if (hudInitializer != null)
        {
            Debug.Log("[HudDialogueBridge] Showing HUD immediately from dialogue");
            hudInitializer.ShowImmediately();
        }
        else
        {
            Debug.LogError("[HudDialogueBridge] HudInitializer reference is null!");
        }
    }
    
    /// <summary>
    /// Hide HUD immediately
    /// Bound to dialogue external function name: "HideHud"
    /// </summary>
    private void HideHudImmediately()
    {
        if (hudInitializer != null)
        {
            Debug.Log("[HudDialogueBridge] Hiding HUD immediately from dialogue");
            hudInitializer.HideImmediately();
        }
        else
        {
            Debug.LogError("[HudDialogueBridge] HudInitializer reference is null!");
        }
    }
}
