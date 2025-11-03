using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Card Data", fileName = "NewCardData")]
public class CardData : GameItemData
{
    // --------------------------------------------------
    // CARD INFO
    // --------------------------------------------------

    [Header("Card Info")]
    [Tooltip("Displayed text for the card's intention (e.g., 'Deal damage', 'Block', etc.).")]
    [TextArea(1, 3)]
    public string intentionText;

    [Tooltip("Sound cue played when this card is used.")]
    public SFXCueData soundCue;

    [Tooltip("Energy cost to play this card.")]
    public int energyCost;

    // --------------------------------------------------
    // ANIMATION SETTINGS
    // --------------------------------------------------

    [Header("Animation Settings")]
    [Tooltip("Animation played by the source entity when this card is used (e.g., Attack).")]
    public EnemyAnim sourceAnim = EnemyAnim.Attack;

    [Tooltip("Animation played by the target entity when this card is used (e.g., Hurt).")]
    public EnemyAnim targetAnim = EnemyAnim.Hurt;

    // --------------------------------------------------
    // VARIABILITY SETTINGS
    // --------------------------------------------------

    [Header("Variability Settings")]
    [Tooltip("If true, this card can roll variable potency (Poor, Normal, Potent) based on its multiplier range.")]
    public bool isVariableCard = false;

    [Tooltip("Artwork shown when the card rolls a Poor outcome (optional).")]
    public Sprite poorArtwork;

    [Tooltip("Artwork shown when the card rolls a Potent outcome (optional).")]
    public Sprite potentArtwork;

    // --------------------------------------------------
    // VALIDATION HELPERS
    // --------------------------------------------------

    /// <summary>
    /// Determines if this card meets the logical criteria for variability:
    /// - It has exactly one effect.
    /// - That effect uses a multiplier.
    /// </summary>
    public bool HasVariableEffect()
    {
        return effectData != null
            && effectData.Count == 1
            && effectData[0] != null
            && effectData[0].usesMultiplier;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Optional sanity check to automatically toggle variability when criteria met
        if (HasVariableEffect() && !isVariableCard)
        {
            Debug.LogWarning($"[CardData] '{itemName}' qualifies as a variable card but 'isVariableCard' is disabled.", this);
        }
    }
#endif
}
