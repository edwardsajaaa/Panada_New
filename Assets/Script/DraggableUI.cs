using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static bool isInteractingWithUI = false; // Flag statis untuk mencegah klik bocor ke dialog utama
    public static bool canDrag = false; // Flag statis untuk membatasi kapan player boleh menggeser barang

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!canDrag) return;
        isInteractingWithUI = true; // Kunci input dialog
        rectTransform.SetAsLastSibling(); // Bawa ke paling depan
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Beri jeda 1 frame agar klik mouse (GetMouseButtonDown) di Update() utama tidak sempat mendeteksinya
        Invoke(nameof(LepasKunciInput), 0.1f);
    }

    private void LepasKunciInput()
    {
        isInteractingWithUI = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!canDrag) return;
        isInteractingWithUI = true;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.85f; // Sedikit transparan saat ditarik
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        Invoke(nameof(LepasKunciInput), 0.1f);
    }
}
