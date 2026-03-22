using TMPro;
using UnityEngine;
using DG.Tweening;

public class PileCountUI : MonoBehaviour
{
    public static PileCountUI Instance { get; private set; }

    public PlayerManager playerManager;

    [Header("UI")]
    public TMP_Text drawCountText;
    public TMP_Text discardCountText;

    [Header("Pile Roots")]
    public RectTransform drawPileRoot;
    public RectTransform discardPileRoot;

    private Tween _drawCountTween;
    private Tween _discardCountTween;
    private Tween _drawPileTween;
    private Tween _discardPileTween;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        RefreshCounts();
    }

    public static void RefreshNow()
    {
        Instance?.RefreshCounts();
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

    public void AnimateReshuffle(int oldDrawCount, int oldDiscardCount, int newDrawCount, int newDiscardCount)
    {
        if (drawCountText == null || discardCountText == null)
            return;

        _drawCountTween?.Kill();
        _discardCountTween?.Kill();
        _drawPileTween?.Kill();
        _discardPileTween?.Kill();

        // Start from the old values so the transfer feels visible
        drawCountText.text = oldDrawCount.ToString();
        discardCountText.text = oldDiscardCount.ToString();

        if (discardPileRoot != null)
        {
            discardPileRoot.localScale = Vector3.one;
            _discardPileTween = discardPileRoot
                .DOScale(0.85f, 0.18f)
                .SetEase(Ease.OutQuad)
                .SetLoops(2, LoopType.Yoyo);
        }

        if (drawPileRoot != null)
        {
            drawPileRoot.localScale = Vector3.one;
            _drawPileTween = drawPileRoot
                .DOScale(1.15f, 0.18f)
                .SetEase(Ease.OutBack)
                .SetLoops(2, LoopType.Yoyo);
        }

        int drawDisplay = oldDrawCount;
        int discardDisplay = oldDiscardCount;

        _drawCountTween = DOTween.To(
            () => drawDisplay,
            x =>
            {
                drawDisplay = x;
                drawCountText.text = x.ToString();
            },
            newDrawCount,
            0.35f
        ).SetEase(Ease.OutQuad);

        _discardCountTween = DOTween.To(
            () => discardDisplay,
            x =>
            {
                discardDisplay = x;
                discardCountText.text = x.ToString();
            },
            newDiscardCount,
            0.35f
        ).SetEase(Ease.OutQuad);
    }
}
