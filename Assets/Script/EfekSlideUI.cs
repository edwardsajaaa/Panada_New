using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class EfekSlideUI : MonoBehaviour
{
    [Header("Pengaturan Animasi")]
    [Tooltip("Waktu tunggu sebelum mulai animasi slide (misal nunggu BlackPanel selesai fade)")]
    public float jedaSebelumMuncul = 1.5f;
    [Tooltip("Lama proses slide berlangsung")]
    public float durasiAnimasi = 0.8f;
    [Tooltip("Jarak slide (seberapa jauh dari bawah sebelum naik ke atas)")]
    public float jarakGeser = 1000f;

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
        StartCoroutine(ProsesSlideIn());
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
    }
}
