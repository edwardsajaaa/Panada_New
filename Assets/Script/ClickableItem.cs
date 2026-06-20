using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ClickableItem : MonoBehaviour, IPointerClickHandler
{
    [Header("Pengaturan Efek")]
    public float kecepatanKedip = 3f;
    public float alphaMinimum = 0.4f;
    
    [Header("Event Saat Diklik")]
    public UnityEvent onClick;

    private Image imageTarget;
    private bool sudahDiklik = false;

    void Start()
    {
        imageTarget = GetComponent<Image>();
    }

    void Update()
    {
        if (!sudahDiklik && imageTarget != null)
        {
            // Efek kedap-kedip menggunakan fungsi Sin (berkisar antara alphaMinimum hingga 1)
            float alpha = Mathf.Lerp(alphaMinimum, 1f, (Mathf.Sin(Time.time * kecepatanKedip) + 1f) / 2f);
            Color c = imageTarget.color;
            c.a = alpha;
            imageTarget.color = c;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!sudahDiklik)
        {
            sudahDiklik = true;
            
            // Kembalikan opasitas menjadi 100% (berhenti berkedip)
            if (imageTarget != null)
            {
                Color c = imageTarget.color;
                c.a = 1f;
                imageTarget.color = c;
            }

            // Jalankan fungsi apapun yang didaftarkan di Inspector
            onClick.Invoke();
        }
    }
}
