using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[RequireComponent(typeof(RectTransform))]
public class EfekSlideUI : MonoBehaviour
{
    [Header("Pengaturan Animasi")]
    [Tooltip("Centang jika ingin animasi langsung jalan pas objek ini aktif. Hilangkan centang jika ingin memanggilnya manual lewat script/Event")]
    public bool mulaiOtomatis = true;
    [Tooltip("Waktu tunggu sebelum mulai animasi slide (misal nunggu BlackPanel selesai fade)")]
    public float jedaSebelumMuncul = 1.5f;
    [Tooltip("Lama proses slide berlangsung")]
    public float durasiAnimasi = 0.8f;
    [Tooltip("Jarak slide (seberapa jauh dari bawah sebelum naik ke atas)")]
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
        // Langsung sembunyikan di bawah layar
        rectTransform.anchoredPosition = new Vector2(posisiAsli.x, posisiAsli.y - jarakGeser);
        canvasGroup.alpha = 0f;
        if (mulaiOtomatis)
        {
            MulaiSlideIn();
        }
    }

    public void MulaiSlideIn()
    {
        StartCoroutine(ProsesSlideIn());
    }

    public void MulaiSlideOut()
    {
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
        Vector2 posisiTujuan = new Vector2(posisiAsli.x, posisiAsli.y - jarakGeser);

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
