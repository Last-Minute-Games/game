using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Settings : MonoBehaviour
{
    [Header("UI (TMP)")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown screenModeDropdown;
    [SerializeField] private Slider masterSlider;

    void OnEnable() { StartCoroutine(BindWhenReady()); }
    void Start() { StartCoroutine(BindWhenReady()); }

    IEnumerator BindWhenReady()
    {
        // wait up to ~2s for SettingsManager to exist (covers weird load orders)
        float t = 0f;
        while (SettingsManager.I == null && t < 2f) { t += Time.unscaledDeltaTime; yield return null; }

        var S = SettingsManager.I;
        if (S == null) { Debug.LogWarning("[Settings] SettingsManager not found."); yield break; }

        // --- RESOLUTIONS: if manager didn't build yet, fall back to Screen.resolutions
        var resLabels = new List<string>();
        if (S.ResList.Count > 0)
        {
            foreach (var r in S.ResList) resLabels.Add($"{r.w} x {r.h}");
        }
        else
        {
            var res = Screen.resolutions
                .OrderByDescending(r => r.width * r.height)
                .ThenByDescending(r => r.refreshRate)
                .GroupBy(r => (r.width, r.height))
                .Select(g => g.First())
                .ToList();

            foreach (var r in res) resLabels.Add($"{r.width} x {r.height}");
        }

        if (resolutionDropdown)
        {
            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(resLabels);
            resolutionDropdown.SetValueWithoutNotify(Mathf.Clamp(S.ResolutionIndex, 0, Mathf.Max(0, resLabels.Count - 1)));
        }

        // Screen mode dropdown: Windowed / Borderless / Fullscreen
        if (screenModeDropdown)
        {
            var modes = new List<string> { "Windowed", "Borderless", "Fullscreen" };
            screenModeDropdown.ClearOptions();
            screenModeDropdown.AddOptions(modes);
            screenModeDropdown.SetValueWithoutNotify((int)S.ScreenMode);
        }

        if (masterSlider) masterSlider.SetValueWithoutNotify(S.MasterVolume);

        // listeners (clean rebind)
        if (resolutionDropdown)
        {
            resolutionDropdown.onValueChanged.RemoveAllListeners();
            resolutionDropdown.onValueChanged.AddListener(i => S.ApplyResolution(i));
        }

        if (screenModeDropdown)
        {
            screenModeDropdown.onValueChanged.RemoveAllListeners();
            screenModeDropdown.onValueChanged.AddListener(i => S.ApplyScreenMode(i));
        }

        if (masterSlider)
        {
            masterSlider.onValueChanged.RemoveAllListeners();
            masterSlider.onValueChanged.AddListener(v => S.ApplyMaster(v));
        }
    }
}
