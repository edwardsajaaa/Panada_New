using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class AnimasiLogoPanada : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("Efek Spinning")]
    public bool gunakanSpinning = true;
    public float kecepatanPutar = -28f;
    public float jedaSebelumPutar = 0.55f;

    [Header("Pengaturan Interaksi")]
    [Tooltip("Matikan jika logo ini hanya sebagai dekorasi (misalnya di dalam panel credit)")]
    public bool bisaDiklik = true;

    [Header("Efek Kecelup Saat Klik")]
    public float skalaKecelup = 0.72f;
    public float durasiCelup = 0.10f;
    public float durasiKembali = 0.35f;
    public float kekuatanBounce = 1.5f;
    public float geserBawahKecelup = 6f;

    [Header("Event Saat Diklik")]
    public UnityEngine.Events.UnityEvent saatLogoDiklik;

    // State internal
    private Vector3 skalaAwal;
    private Vector2 posisiAwal;
    private RectTransform rectTransform;
    private float sudutPutaran = 0f;
    private bool sedangDitekan = false;
    private Coroutine coroutineKecelup;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Start()
    {
        skalaAwal = transform.localScale;
        if (skalaAwal == Vector3.zero) skalaAwal = Vector3.one;
        if (rectTransform != null) posisiAwal = rectTransform.anchoredPosition;

        if (gunakanSpinning)
            StartCoroutine(LoopSpinning());
    }

    // ─────────────────────────────────────────────
    //  SPINNING — berjalan terus selamanya di background setelah animasi muncul selesai
    // ─────────────────────────────────────────────
    IEnumerator LoopSpinning()
    {
        // 1. Pastikan posisi rotasi awal lurus horizontal (0 derajat)
        sudutPutaran = 0f;
        transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        // 2. Cek apakah ada durasi animasi dari induk (misal Kopi yang meluncur horizontal) atau script ini sendiri
        float durasiTunggu = jedaSebelumPutar;
        AnimasiTombolMenu animInduk = GetComponentInParent<AnimasiTombolMenu>();
        if (animInduk != null && animInduk.gunakanAnimasiIn)
        {
            float totalDurasiInduk = animInduk.durasiAnimasiIn + animInduk.delayMuncul;
            if (totalDurasiInduk > durasiTunggu) durasiTunggu = totalDurasiInduk;
        }

        // 3. Tunggu sampai animasi muncul horizontal selesai total
        if (durasiTunggu > 0f)
        {
            yield return new WaitForSeconds(durasiTunggu);
        }

        // 4. Setelah selesai muncul horizontal, baru mulai berputar secara halus
        while (true)
        {
            sudutPutaran += Time.deltaTime * kecepatanPutar;

            // Normalisasi agar tidak overflow ke angka sangat besar
            if (sudutPutaran > 360f) sudutPutaran -= 360f;
            else if (sudutPutaran < -360f) sudutPutaran += 360f;

            // Terapkan rotasi — hanya sumbu Z (UI berputar di bidang layar)
            transform.localRotation = Quaternion.Euler(0f, 0f, sudutPutaran);

            yield return null;
        }
    }

    // ─────────────────────────────────────────────
    //  KLIK — kecelup & kembali
    // ─────────────────────────────────────────────
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!bisaDiklik) return;
        sedangDitekan = true;
        if (coroutineKecelup != null) StopCoroutine(coroutineKecelup);
        coroutineKecelup = StartCoroutine(AnimasiCelup());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!bisaDiklik) return;
        sedangDitekan = false;
        if (coroutineKecelup != null) StopCoroutine(coroutineKecelup);
        coroutineKecelup = StartCoroutine(AnimasiKembali(true));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!bisaDiklik) return;
        // Cadangan jaminan jika klik cepat terinterupsi/terlepas
        if (!sedangDitekan && coroutineKecelup == null)
        {
            if (coroutineKecelup != null) StopCoroutine(coroutineKecelup);
            coroutineKecelup = StartCoroutine(AnimasiCelupDanKembali());
        }
    }

    IEnumerator AnimasiCelupDanKembali()
    {
        yield return StartCoroutine(AnimasiCelup());
        yield return new WaitForSeconds(0.05f);
        yield return StartCoroutine(AnimasiKembali(true));
    }

    /// <summary>
    /// Fase 1: Logo mengecil dan turun ("kecelup ke dalam kopi")
    /// Spinning terus berjalan di background — hanya skala dan posisi yang berubah di sini.
    /// </summary>
    IEnumerator AnimasiCelup()
    {
        Vector3 skalaTarget  = skalaAwal * skalaKecelup;
        Vector2 posisiTarget = posisiAwal - new Vector2(0f, geserBawahKecelup);

        Vector3 skalaStart   = transform.localScale;
        Vector2 posisiStart  = rectTransform != null ? rectTransform.anchoredPosition : posisiAwal;

        float waktu = 0f;
        while (waktu < durasiCelup)
        {
            waktu += Time.deltaTime;
            float t = Mathf.Clamp01(waktu / durasiCelup);
            float ease = 1f - (1f - t) * (1f - t); // EaseOutQuad

            transform.localScale = Vector3.LerpUnclamped(skalaStart, skalaTarget, ease);
            if (rectTransform != null)
                rectTransform.anchoredPosition = Vector2.LerpUnclamped(posisiStart, posisiTarget, ease);

            yield return null;
        }

        transform.localScale = skalaTarget;
        if (rectTransform != null) rectTransform.anchoredPosition = posisiTarget;
    }

    /// <summary>
    /// Fase 2: Logo balik ke ukuran semula dengan bounce ("mental dari kopi")
    /// Spinning tetap berjalan — hanya skala dan posisi yang dikembalikan.
    /// </summary>
    IEnumerator AnimasiKembali(bool pemicuEvent = false)
    {
        Vector3 skalaStart  = transform.localScale;
        Vector2 posisiStart = rectTransform != null ? rectTransform.anchoredPosition : posisiAwal;

        float waktu = 0f;
        while (waktu < durasiKembali)
        {
            waktu += Time.deltaTime;
            float t = Mathf.Clamp01(waktu / durasiKembali);

            // EaseOutBack — balik melewati ukuran asli sedikit lalu settle (efek "mental")
            float s  = kekuatanBounce;
            float t2 = t - 1f;
            float ease = t2 * t2 * ((s + 1f) * t2 + s) + 1f;

            transform.localScale = Vector3.LerpUnclamped(skalaStart, skalaAwal, ease);
            if (rectTransform != null)
                rectTransform.anchoredPosition = Vector2.LerpUnclamped(posisiStart, posisiAwal, ease);

            yield return null;
        }

        // Pastikan kembali tepat ke nilai awal
        transform.localScale = skalaAwal;
        if (rectTransform != null) rectTransform.anchoredPosition = posisiAwal;

        if (pemicuEvent)
        {
            TransisiMenuUI transisi = GetComponentInParent<TransisiMenuUI>();
            if (transisi != null && transisi.panelCredit != null)
            {
                transisi.BukaPanelCredit();
            }
            saatLogoDiklik?.Invoke();
        }
    }

    void OnDisable()
    {
        StopAllCoroutines();
        sudutPutaran   = 0f;
        sedangDitekan  = false;
        coroutineKecelup = null;
        transform.localScale = skalaAwal;
        if (rectTransform != null) rectTransform.anchoredPosition = posisiAwal;
    }

    void OnEnable()
    {
        // Restart spinning saat GameObject diaktifkan kembali
        sudutPutaran = 0f;
        if (gunakanSpinning)
            StartCoroutine(LoopSpinning());
    }
}
