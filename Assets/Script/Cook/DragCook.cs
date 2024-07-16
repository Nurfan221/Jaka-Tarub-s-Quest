using UnityEngine;
using UnityEngine.EventSystems;

public class DragCook : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    public string itemName; // Nama item
    public Vector2 originalPosition;
    private Transform originalParent;

    public Transform OriginalParent
    {
        get { return originalParent; }
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        originalPosition = rectTransform.anchoredPosition;
        originalParent = rectTransform.parent;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / GetComponentInParent<Canvas>().scaleFactor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Mengecek apakah item di-drop di slot valid, jika tidak, kembalikan ke posisi awal
        if (eventData.pointerEnter == null || (!eventData.pointerEnter.GetComponent<DropCookSlot>() && !eventData.pointerEnter.GetComponent<CookInventory>()))
        {
            ResetPosition();
        }

        // Memanggil suara klik ketika item di-drop
        SoundManager.Instance.PlaySound("Click");
    }

    public void ResetPosition()
    {
        rectTransform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalPosition;
    }
}