using UnityEngine;

public class OverworldCoinGameLauncher : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public GameObject coinFlipPopupPrefab;
    public MonoBehaviour[] controlsToDisable;

    [Header("HUD (optional)")]
    public GameObject hudGroup;

    [Header("Interaction")]
    [Tooltip("Maximum distance from the player to trigger the coinflip minigame.")]
    public float interactDistance = 2.5f;

    [Header("Protection")]
    public float sceneOpenDelay = 0.35f; // block instant open after load/room swap
    public float reopenCooldown = 0.25f; // block double taps

    GameObject _popupInstance;
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
        if (distance <= interactDistance && Input.GetKeyDown(KeyCode.C))
            OpenCoinFlipPopup();
    }

    public void OpenCoinFlipPopup()
    {
        if (!_canOpen) return;
        if (Time.unscaledTime - _lastCloseTime < reopenCooldown) return;
        if (_popupInstance != null) return;

        _popupInstance = Instantiate(coinFlipPopupPrefab);

        // NEW: pause the overworld timer
        FindObjectOfType<ClockTimer>()?.PauseTimer(true);

        foreach (var c in controlsToDisable) if (c) c.enabled = false;

        if (hudGroup != null) //HUD off
            hudGroup.SetActive(false);
    }

    public void CloseCoinFlipPopup()
    {
        if (_popupInstance == null) return;

        

        Destroy(_popupInstance);
        foreach (var c in controlsToDisable) if (c) c.enabled = true;

        if (hudGroup != null)
            hudGroup.SetActive(true);

        FindObjectOfType<ClockTimer>()?.PauseTimer(false);

        _lastCloseTime = Time.unscaledTime;
        _canOpen = false;
        StartCoroutine(EnableOpenAfterDelay(sceneOpenDelay)); // brief lockout after close
    }
}
