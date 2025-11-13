using UnityEngine;

public class OverworldCoinGameLauncher : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public GameObject coinFlipPopupPrefab;
    public MonoBehaviour[] controlsToDisable;

    [Header("Protection")]
    public float sceneOpenDelay = 0.35f; // block instant open after load/room swap
    public float reopenCooldown = 0.25f; // block double taps

    GameObject _popupInstance;
    bool _canOpen = false;
    float _lastCloseTime = -999f;

    void OnEnable()
    {
        _canOpen = false;
        StartCoroutine(EnableOpenAfterDelay(sceneOpenDelay));
        // If you use the old Input Manager, this clears “stuck” inputs across scene loads:
        Input.ResetInputAxes();
    }

    System.Collections.IEnumerator EnableOpenAfterDelay(float t)
    {
        yield return new WaitForSecondsRealtime(t);
        _canOpen = true;
    }

    void Update()
    {
        // If you still want a keyboard shortcut:
        if (Input.GetKeyDown(KeyCode.C))
            OpenCoinFlipPopup();
    }

    public void OpenCoinFlipPopup()
    {
        if (!_canOpen) return;
        if (Time.unscaledTime - _lastCloseTime < reopenCooldown) return;
        if (_popupInstance != null) return;

        _popupInstance = Instantiate(coinFlipPopupPrefab);
        foreach (var c in controlsToDisable) if (c) c.enabled = false;
    }

    public void CloseCoinFlipPopup()
    {
        if (_popupInstance == null) return;

        Destroy(_popupInstance);
        foreach (var c in controlsToDisable) if (c) c.enabled = true;

        _lastCloseTime = Time.unscaledTime;
        _canOpen = false;
        StartCoroutine(EnableOpenAfterDelay(sceneOpenDelay)); // brief lockout after close
    }
}
