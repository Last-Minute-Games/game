using TMPro;
using UnityEngine;

public class PileCountUI : MonoBehaviour
{
    public PlayerManager playerManager;

    [Header("UI")]
    public TMP_Text drawCountText;
    public TMP_Text discardCountText;

    void Start()
    {
        RefreshCounts();
    }

    public void RefreshCounts()
    {
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
