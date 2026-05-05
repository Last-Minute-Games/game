using DG.Tweening;
using UnityEngine;

/// <summary>
/// Large hover preview (Overhaul-style): starts offset from the source card, tweens in with DOMove + DORotateQuaternion.
/// Assign a duplicate <see cref="CardRender"/> (hidden by default) in the scene.
/// </summary>
public class NetherCardViewHover : Singleton<NetherCardViewHover>
{
  [SerializeField] private CardRender hoverCard;

  [SerializeField] private float moveDuration = 0.25f;

  [Tooltip("When true, CardFXHelper skips the normal in-hand hover lift/scale so only this preview shows.")]
  public bool SuppressesInPlaceHover = true;

  [SerializeField] private Vector3 sideOffset = new(-2.2f, 0.15f, 0f);

  [SerializeField] private Vector3 liftOffset = new(0f, 0.5f, 0f);

  private Tween _moveTween;

  private Tween _rotateTween;

  public void ShowForCard(CardRender sourceCard)
  {
    if (hoverCard == null || sourceCard == null)
      return;

    hoverCard.gameObject.SetActive(true);

    if (sourceCard.Instance != null)
      hoverCard.Bind(sourceCard.Instance);
    else if (sourceCard.Data != null)
      hoverCard.Bind(sourceCard.Data);

    Transform src = sourceCard.transform;
    Quaternion srcRot = src.rotation;
    Vector3 srcPos = src.position;

    hoverCard.transform.SetPositionAndRotation(
      srcPos + src.TransformDirection(sideOffset),
      srcRot);

    _moveTween?.Kill();
    _rotateTween?.Kill();

    Vector3 endPos = srcPos + src.TransformDirection(liftOffset);
    _moveTween = hoverCard.transform.DOMove(endPos, moveDuration);
    _rotateTween = hoverCard.transform.DORotateQuaternion(Quaternion.identity, moveDuration);
  }

  public void Hide()
  {
    _moveTween?.Kill();
    _rotateTween?.Kill();
    _moveTween = null;
    _rotateTween = null;

    if (hoverCard != null)
      hoverCard.gameObject.SetActive(false);
  }
}
