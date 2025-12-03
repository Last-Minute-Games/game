using UnityEngine;

public class OverworldCoinGameLauncher : MonoBehaviour
{
    [Header("Assign in Inspector")]
    [Tooltip("Direct reference to the coinflip popup GameObject in the scene.")]
    public GameObject coinFlipPopup;
    public MonoBehaviour[] controlsToDisable;

    [Header("HUD (optional)")]
    public GameObject hudGroup;

    [Header("Interaction")]
    [Tooltip("Maximum distance from the player to trigger the coinflip minigame.")]
    public float interactDistance = 2.5f;

    [Header("Protection")]
    public float sceneOpenDelay = 0.35f; // block instant open after load/room swap
    public float reopenCooldown = 0.25f; // block double taps

    public MinigameInstructions coinFlipInstructions;

    bool _canOpen = false;
    float _lastCloseTime = -999f;
    private Transform player;

    void OnEnable()
    {
        _canOpen = false;
        StartCoroutine(EnableOpenAfterDelay(sceneOpenDelay));
        // If you use the old Input Manager, this clears �stuck� inputs across scene loads:
        Input.ResetInputAxes();
    }

    void Start()
    {
        // Find player automatically
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    System.Collections.IEnumerator EnableOpenAfterDelay(float t)
    {
        yield return new WaitForSecondsRealtime(t);
        _canOpen = true;
    }

    void Update()
    {
        // Only trigger if player is near and presses C
        if (player == null) return;
        
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= interactDistance && Input.GetKeyDown(KeyCode.E))
            OpenCoinFlipPopup();
    }

    public void OpenCoinFlipPopup()
    {
        if (!_canOpen) return;
        if (Time.unscaledTime - _lastCloseTime < reopenCooldown) return;
        if (coinFlipPopup == null || coinFlipPopup.activeSelf) return;

        // Show the existing popup GameObject
        coinFlipPopup.SetActive(true);

        if (coinFlipInstructions == null)
        {
            // fallback: try to find it on children
            coinFlipInstructions = coinFlipPopup.GetComponentInChildren<MinigameInstructions>(true);
        }
        if (coinFlipInstructions != null)
        {
            coinFlipInstructions.OnPopupOpened();
        }

        // Pause NPCs and ClockTimer (but NOT player input - using minigame pause)
        GlobalPause.SetMinigamePaused(true);

        foreach (var c in controlsToDisable) if (c) c.enabled = false;

        if (hudGroup != null) //HUD off
            hudGroup.SetActive(false);
    }

    public void CloseCoinFlipPopup()
    {
        if (coinFlipPopup == null || !coinFlipPopup.activeSelf) return;

        // Hide the popup GameObject (don't destroy it)
        coinFlipPopup.SetActive(false);
        
        foreach (var c in controlsToDisable) if (c) c.enabled = true;

        if (hudGroup != null)
            hudGroup.SetActive(true);

        // Resume NPCs and ClockTimer (using minigame pause)
        GlobalPause.SetMinigamePaused(false);

        _lastCloseTime = Time.unscaledTime;
        _canOpen = false;
        StartCoroutine(EnableOpenAfterDelay(sceneOpenDelay)); // brief lockout after close
    }
}
