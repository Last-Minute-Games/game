using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Basic visual + interaction component for a card instance in the scene/UI.
// Attach this to a Card prefab (with an Image and optional Texts) and call Bind(data) to populate.
public class CardPrefab : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("Data")]
    public CardData Data;

    [Header("UI Refs (optional)")]
    public Image artworkImage;
    public Text nameText;
    public Text descriptionText;

    [Header("Interaction Settings")]
    [Tooltip("Scale applied while hovering the card.")]
    public float hoverScale = 1.05f;
    [Tooltip("Scale applied while dragging the card.")]
    public float dragScale = 1.05f;

    [Header("Events")] 
    public UnityEvent<CardPrefab> OnClicked;
    public UnityEvent<CardPrefab> OnBeginDragEvent;
    public UnityEvent<CardPrefab> OnEndDragEvent;

    // Runtime
    private Vector3 _originalPosition;
    private Transform _originalParent;
    private bool _isDragging;
    private bool _pointerDown;

    private void Awake()
    {
        _originalParent = transform.parent;
        _originalPosition = transform.localPosition;
    }

    public void Bind(CardData data)
    {
        Data = data;
        if (nameText != null) nameText.text = data != null ? data.Name : string.Empty;
        if (descriptionText != null) descriptionText.text = data != null ? data.Description : string.Empty;
        if (artworkImage != null) artworkImage.sprite = data != null ? data.Artwork : null;
    }

    // Convenience for animation helpers that expect these hooks
    public void SetPosition(Vector3 worldPos)
    {
        transform.position = worldPos;
    }

    public void ResetToOriginalPosition()
    {
        transform.SetParent(_originalParent, worldPositionStays: true);
        transform.localPosition = _originalPosition;
        transform.localScale = Vector3.one;
    }

    // Pointer/Drag handlers
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isDragging) return;
        transform.localScale = Vector3.one * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isDragging) return;
        transform.localScale = Vector3.one;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _pointerDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_pointerDown && !_isDragging)
        {
            OnClicked?.Invoke(this);
        }
        _pointerDown = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _isDragging = true;
        transform.localScale = Vector3.one * dragScale;
        OnBeginDragEvent?.Invoke(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        // Follow cursor in screen space (assuming Screen Space - Overlay canvas).
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;
        transform.localScale = Vector3.one;
        OnEndDragEvent?.Invoke(this);
    }
}
