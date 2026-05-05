using UnityEngine;

/// <summary>
/// Overworld: shows the card catalog entry (e.g. bottom-right button) only after the player
/// has finished a card battle in <c>BattleScene</c> (the Nether) at least once
/// (<see cref="NetherProgressFlags.HasVisitedNether"/>).
/// <para>
/// Setup: add the same <see cref="BattleCardCatalogPopup"/> UI you use in BattleScene (prefab or copy)
/// under the Overworld Canvas. Assign its <b>Open Button</b> to your corner button.
/// Put this component on any active object and assign <see cref="catalogEntryRoot"/> to the
/// GameObject wrapping that button (hidden until unlocked).
/// </para>
/// </summary>
public class OverworldCardCatalogAccess : MonoBehaviour
{
    [Tooltip("Typically an empty parent at bottom-right containing the catalog button; disabled until nether.visited.")]
    [SerializeField] private GameObject catalogEntryRoot;

    private void Start()
    {
        RefreshVisibility();
        if (GameFlags.Instance != null)
            GameFlags.Instance.OnFlagChanged += OnFlagChanged;
    }

    private void OnDestroy()
    {
        if (GameFlags.Instance != null)
            GameFlags.Instance.OnFlagChanged -= OnFlagChanged;
    }

    private void OnFlagChanged(string _)
    {
        RefreshVisibility();
    }

    public void RefreshVisibility()
    {
        if (catalogEntryRoot == null)
            return;
        catalogEntryRoot.SetActive(GameFlags.HasFlag(NetherProgressFlags.HasVisitedNether));
    }
}
