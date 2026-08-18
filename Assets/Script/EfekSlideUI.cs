using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[RequireComponent(typeof(RectTransform))]
public class EfekSlideUI : MonoBehaviour
{
    public enum ArahSlide { Bawah, Atas, Kiri, Kanan }

    [Header("Pengaturan Animasi")]
    [Tooltip("Centang jika ingin animasi langsung jalan pas objek ini aktif. Hilangkan centang jika ingin memanggilnya manual lewat script/Event")]
    public bool mulaiOtomatis = true;
    [Tooltip("Dari arah mana objek ini akan muncul?")]
    public ArahSlide arahMuncul = ArahSlide.Bawah;
    [Tooltip("Waktu tunggu sebelum mulai animasi slide")]
    public float jedaSebelumMuncul = 0f;
    [Tooltip("Lama proses slide berlangsung")]
    public float durasiAnimasi = 0.8f;
    [Tooltip("Jarak slide (seberapa jauh dari posisinya sebelum muncul)")]
    public float jarakGeser = 1000f;

    [Header("Event Animasi (Opsional)")]
    [Tooltip("Dipanggil tepat setelah animasi Slide In (masuk) selesai")]
    public UnityEvent saatSlideInSelesai;
    [Tooltip("Dipanggil tepat setelah animasi Slide Out (menyingkir) selesai")]
    public UnityEvent saatSlideOutSelesai;

    private RectTransform rectTransform;
    private Vector2 posisiAsli;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        posisiAsli = rectTransform.anchoredPosition;

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        // Langsung sembunyikan di posisi sembunyinya
        rectTransform.anchoredPosition = DapatkanPosisiSembunyi();
        canvasGroup.alpha = 0f;
        if (mulaiOtomatis)
        {
            MulaiSlideIn();
        }
    }

    Vector2 DapatkanPosisiSembunyi()
    {
        Vector2 pos = posisiAsli;
        switch (arahMuncul)
        {
            case ArahSlide.Bawah: pos.y -= jarakGeser; break;
            case ArahSlide.Atas: pos.y += jarakGeser; break;
            case ArahSlide.Kiri: pos.x -= jarakGeser; break;
            case ArahSlide.Kanan: pos.x += jarakGeser; break;
        }
        return pos;
    }

    public void MulaiSlideIn()
    {
        if (!gameObject.activeInHierarchy) 
        {
            gameObject.SetActive(true);
            if (!gameObject.activeInHierarchy) return; // Jika parent mati, batalkan agar tidak error
        }
        StartCoroutine(ProsesSlideIn());
    }

    public void MulaiSlideOut()
    {
        if (!gameObject.activeInHierarchy) 
        {
            gameObject.SetActive(true);
            if (!gameObject.activeInHierarchy) return;
        }
        StartCoroutine(ProsesSlideOut());
    }

    IEnumerator ProsesSlideIn()
    {
        // Tunggu sesuai jeda yang diatur
        if (jedaSebelumMuncul > 0)
        {
            yield return new WaitForSeconds(jedaSebelumMuncul);
        }

        canvasGroup.alpha = 1f;
        float waktu = 0f;
        Vector2 posisiMulai = rectTransform.anchoredPosition;

        while (waktu < durasiAnimasi)
        {
            waktu += Time.deltaTime;
            // Gunakan kurva SmoothStep agar animasinya melambat di akhir (lebih elegan)
            float t = Mathf.SmoothStep(0f, 1f, waktu / durasiAnimasi);
            rectTransform.anchoredPosition = Vector2.Lerp(posisiMulai, posisiAsli, t);
            yield return null;
        }

        rectTransform.anchoredPosition = posisiAsli;
        
        saatSlideInSelesai?.Invoke();
    }

    IEnumerator ProsesSlideOut()
    {
        float waktu = 0f;
        Vector2 posisiMulai = rectTransform.anchoredPosition;
        Vector2 posisiTujuan = DapatkanPosisiSembunyi();

        while (waktu < durasiAnimasi)
        {
            waktu += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, waktu / durasiAnimasi);
            rectTransform.anchoredPosition = Vector2.Lerp(posisiMulai, posisiTujuan, t);
            yield return null;
        }

        rectTransform.anchoredPosition = posisiTujuan;
        canvasGroup.alpha = 0f; // Sembunyikan sepenuhnya setelah turun
        
        saatSlideOutSelesai?.Invoke();
    }
}
