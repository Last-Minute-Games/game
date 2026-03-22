using TMPro;
using UnityEngine;

public class PileCountUI : MonoBehaviour
{
    public PlayerManager playerManager;

    [Header("UI")]
    public TMP_Text drawCountText;
    public TMP_Text discardCountText;

    private static PileCountUI _instance;

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        RefreshCounts();
    }

    public static void RefreshNow()
    {
        _instance?.RefreshCounts();
    }

    public void RefreshCounts()
    {
        if (drawCountText == null || discardCountText == null)
            return;

        if (playerManager == null || playerManager.cardManager == null)
        {
            drawCountText.text = "0";
            discardCountText.text = "0";
            return;
        }

        drawCountText.text = playerManager.cardManager.drawPile.Count.ToString();
        discardCountText.text = playerManager.cardManager.discardPile.Count.ToString();
    }
}
