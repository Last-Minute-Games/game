using UnityEngine;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;
    public TooltipUI tooltipPrefab;
    private TooltipUI tooltipInstance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        tooltipInstance = Instantiate(tooltipPrefab, transform);
        tooltipInstance.Hide();
    }

    public void ShowTooltip(CardData card, CardRunner runner = null, CharacterBase viewer = null)
    {
        tooltipInstance.Show(card, runner, viewer);
    }

    public void HideTooltip()
    {
        tooltipInstance.Hide();
    }
}
