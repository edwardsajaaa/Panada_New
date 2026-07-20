using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class AnimasiTombolMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public enum ModeStay
    {
        Breathing,              // Bernafas (Membesar-mengecil halus)
        Floating,               // Mengambang (Naik-turun halus)
        Wobble,                 // Bergoyang miring santai
        Spinning,               // Berputar 360 derajat terus-menerus (sangat bagus untuk Logo / Piringan)
        BreathingDanSpinning,   // Gabungan Bernafas + Berputar 360 derajat
        Kombinasi,              // Gabungan Breathing + Floating + Wobble
        Diam                    // Diam saja di posisi setelah muncul (tanpa animasi idle)
    }

    public enum ModeAnimasiIn
    {
        PopInBawah,         // Pop-in membesar dari bawah (Default tombol)
        SlideDariKiri,      // Meluncur masuk dari sisi kiri layar ke kanan
        SlideDariKanan,     // Meluncur masuk dari sisi kanan layar ke kiri
        SlideDariAtas,      // Meluncur masuk dari atas
        SlideDariBawah,     // Meluncur masuk dari bawah
        FlipKartu3D,        // Membalik seperti kartu dari tertutup (-90 Y) ke terbuka (0 Y)
        JatuhKartuMeja,     // Melayang lalu jatuh ke atas meja seperti menaruh kartu
        Fade                // Murni memudar halus (alpha 0 -> 1) tanpa perubahan skala/posisi/rotasi
    }

    [Header("Animasi Masuk")]
    public bool gunakanAnimasiIn = true;
    public ModeAnimasiIn modeAnimasiIn = ModeAnimasiIn.PopInBawah;
    public float delayMuncul = 0f;
    public float durasiAnimasiIn = 0.48f;
    public float kekuatanBounceIn = 1.8f;
    public float geserBawahAwal = 35f;
    public float jarakSlideIn = 800f;

    [Header("Animasi Keluar")]
    public bool gunakanAnimasiOut = true;
    public ModeAnimasiIn modeAnimasiOut = ModeAnimasiIn.SlideDariKiri;
    public float durasiAnimasiOut = 0.35f;
    public float jarakSlideOut = 800f;

    [Header("Animasi Diam (Idle)")]
    public bool gunakanAnimasiStay = true;
    public ModeStay modeStay = ModeStay.Breathing;
    public float kecepatanStay = 3.2f;
    public float intensitasSkalaStay = 0.04f;
    public float intensitasPosisiStay = 4.5f;
    public float intensitasRotasiStay = 2f;
    public float kecepatanPutarStay = -25f;

    [Header("Efek Kursor Hover & Klik")]
    public bool gunakanHoverDanClick = true;
    public float targetSkalaHover = 1.10f;
    public float targetSkalaClick = 0.93f;
    public float kecepatanHover = 16f;
    public bool bawaKeDepanSaatHover = true;
    public float jedaSebelumHover = 0.5f;

    private Vector3 skalaAwal;
    private Vector2 posisiAwal;
    private Quaternion rotasiAwal;
    private int indeksSiblingAwal;
    private float sudutPutaran = 0f;
    private bool sedangHover = false;
    private bool sedangDitekan = false;
    private bool sudahInisialisasi = false;
    private bool siapMenerimaHover = false;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Matikan transisi warna/alpha bawaan Unity pada seluruh komponen Selectable (Button, Toggle, dll) di objek ini & anak-anaknya
        UnityEngine.UI.Selectable[] selectables = GetComponentsInChildren<UnityEngine.UI.Selectable>(true);
        foreach (var s in selectables)
        {
            if (s != null) s.transition = UnityEngine.UI.Selectable.Transition.None;
        }
    }

    void InisialisasiAwal()
    {
        if (sudahInisialisasi) return;
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        skalaAwal = transform.localScale;
        if (skalaAwal == Vector3.zero) skalaAwal = Vector3.one;
        if (rectTransform != null) posisiAwal = rectTransform.anchoredPosition;
        rotasiAwal = transform.localRotation;
        indeksSiblingAwal = transform.GetSiblingIndex();
        sudutPutaran = 0f;
        sudahInisialisasi = true;
    }

    void OnEnable()
    {
        InisialisasiAwal();
        sedangHover = false;
        sedangDitekan = false;
        sudutPutaran = 0f;
        siapMenerimaHover = false;

        // Matikan AnimasiHoverUI jika ada pada tombol yang sama agar tidak bentrok
        AnimasiHoverUI hov = GetComponent<AnimasiHoverUI>();
        if (hov != null) hov.enabled = false;

        StopAllCoroutines();
        if (gunakanAnimasiIn)
        {
            StartCoroutine(ProsesAnimasiIn());
        }
        else
        {
            StartCoroutine(TungguJedaSiapHover(jedaSebelumHover));
            if (gunakanAnimasiStay)
            {
                StartCoroutine(ProsesAnimasiStay());
            }
        }
    }

    void OnDisable()
    {
        if (!sudahInisialisasi) return;
        sedangHover = false;
        sedangDitekan = false;
        sudutPutaran = 0f;
        StopAllCoroutines();
        transform.localScale = skalaAwal;
        if (rectTransform != null) rectTransform.anchoredPosition = posisiAwal;
        transform.localRotation = rotasiAwal;
        if (canvasGroup != null) canvasGroup.alpha = 1f;
    }

    public void ResetKePosisiAwal()
    {
        if (!sudahInisialisasi) return;
        sedangHover = false;
        sedangDitekan = false;
        sudutPutaran = 0f;
        StopAllCoroutines();
        transform.localScale = skalaAwal;
        if (rectTransform != null) rectTransform.anchoredPosition = posisiAwal;
        transform.localRotation = rotasiAwal;
        if (bawaKeDepanSaatHover)
        {
            transform.SetSiblingIndex(indeksSiblingAwal);
        }
        if (canvasGroup != null) canvasGroup.alpha = 1f;
    }

    IEnumerator ProsesAnimasiIn()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Set kondisi awal berdasarkan modeAnimasiIn
        Vector3 skalaMulai = Vector3.zero;
        Vector2 posisiMulai = posisiAwal - new Vector2(0f, geserBawahAwal);
        Quaternion rotasiMulai = rotasiAwal * Quaternion.Euler(0f, 0f, -6f);

        if (modeAnimasiIn == ModeAnimasiIn.PopInBawah)
        {
            skalaMulai = Vector3.zero;
            posisiMulai = posisiAwal - new Vector2(0f, geserBawahAwal);
            rotasiMulai = rotasiAwal * Quaternion.Euler(0f, 0f, -6f);
        }
        else if (modeAnimasiIn == ModeAnimasiIn.SlideDariKiri)
        {
            skalaMulai = skalaAwal;
            posisiMulai = posisiAwal - new Vector2(jarakSlideIn, 0f);
            rotasiMulai = rotasiAwal;
        }
        else if (modeAnimasiIn == ModeAnimasiIn.SlideDariKanan)
        {
            skalaMulai = skalaAwal;
            posisiMulai = posisiAwal + new Vector2(jarakSlideIn, 0f);
            rotasiMulai = rotasiAwal;
        }
        else if (modeAnimasiIn == ModeAnimasiIn.SlideDariAtas)
        {
            skalaMulai = skalaAwal;
            posisiMulai = posisiAwal + new Vector2(0f, jarakSlideIn);
            rotasiMulai = rotasiAwal;
        }
        else if (modeAnimasiIn == ModeAnimasiIn.SlideDariBawah)
        {
            skalaMulai = skalaAwal;
            posisiMulai = posisiAwal - new Vector2(0f, jarakSlideIn);
            rotasiMulai = rotasiAwal;
        }
        else if (modeAnimasiIn == ModeAnimasiIn.FlipKartu3D)
        {
            skalaMulai = skalaAwal * 0.8f;
            posisiMulai = posisiAwal;
            rotasiMulai = rotasiAwal * Quaternion.Euler(0f, -90f, 12f);
        }
        else if (modeAnimasiIn == ModeAnimasiIn.JatuhKartuMeja)
        {
            skalaMulai = skalaAwal * 1.35f;
            posisiMulai = posisiAwal + new Vector2(0f, 320f);
            rotasiMulai = rotasiAwal * Quaternion.Euler(0f, 0f, 16f);
        }
        else if (modeAnimasiIn == ModeAnimasiIn.Fade)
        {
            skalaMulai = skalaAwal;
            posisiMulai = posisiAwal;
            rotasiMulai = rotasiAwal;
        }

        transform.localScale = skalaMulai;
        if (rectTransform != null) rectTransform.anchoredPosition = posisiMulai;
        transform.localRotation = rotasiMulai;
        if (modeAnimasiIn == ModeAnimasiIn.PopInBawah || modeAnimasiIn == ModeAnimasiIn.FlipKartu3D || modeAnimasiIn == ModeAnimasiIn.JatuhKartuMeja || modeAnimasiIn == ModeAnimasiIn.Fade)
            canvasGroup.alpha = 0f;
        else
            canvasGroup.alpha = 0.15f;

        // Tunggu delay jika ada (untuk animasi staggered / muncul berurutan)
        if (delayMuncul > 0f)
        {
            yield return new WaitForSeconds(delayMuncul);
        }

        float waktuMulai = Time.time;
        while (Time.time < waktuMulai + durasiAnimasiIn)
        {
            float progress = (Time.time - waktuMulai) / durasiAnimasiIn;
            
            // Rumus EaseOutBack untuk efek pantulan pop-in / slide-in yang kenyal dan memukau
            float t = progress - 1f;
            float s = kekuatanBounceIn; // overshoot amount
            float easeOutBack = (t * t * ((s + 1f) * t + s) + 1f);

            if (modeAnimasiIn == ModeAnimasiIn.Fade)
            {
                transform.localScale = skalaAwal;
                if (rectTransform != null) rectTransform.anchoredPosition = posisiAwal;
                transform.localRotation = rotasiAwal;
                canvasGroup.alpha = Mathf.Clamp01(progress * 1.4f);
            }
            else
            {
                transform.localScale = Vector3.LerpUnclamped(skalaMulai, skalaAwal, easeOutBack);
                if (rectTransform != null) rectTransform.anchoredPosition = Vector2.LerpUnclamped(posisiMulai, posisiAwal, easeOutBack);
                
                if (modeAnimasiIn == ModeAnimasiIn.FlipKartu3D || modeAnimasiIn == ModeAnimasiIn.JatuhKartuMeja)
                    transform.localRotation = Quaternion.LerpUnclamped(rotasiMulai, rotasiAwal, easeOutBack);
                else
                    transform.localRotation = Quaternion.Lerp(rotasiMulai, rotasiAwal, progress);
                
                if (modeAnimasiIn == ModeAnimasiIn.PopInBawah || modeAnimasiIn == ModeAnimasiIn.FlipKartu3D || modeAnimasiIn == ModeAnimasiIn.JatuhKartuMeja)
                    canvasGroup.alpha = Mathf.Clamp01(progress * 1.6f);
                else
                    canvasGroup.alpha = Mathf.Clamp01(0.15f + progress * 1.5f);
            }

            yield return null;
        }

        // Pastikan kembali tepat ke nilai awal
        transform.localScale = skalaAwal;
        if (rectTransform != null) rectTransform.anchoredPosition = posisiAwal;
        transform.localRotation = rotasiAwal;
        if (canvasGroup != null) canvasGroup.alpha = 1f;

        // Tunggu jeda setelah animasi muncul selesai agar tombol benar-benar aktif & siap sebelum merespon hover
        StartCoroutine(TungguJedaSiapHover(jedaSebelumHover));

        // Setelah Animasi In selesai, langsung lanjut ke Animasi Stay jika diaktifkan
        if (gunakanAnimasiStay && !sedangHover && !sedangDitekan)
        {
            StartCoroutine(ProsesAnimasiStay());
        }
    }

    IEnumerator TungguJedaSiapHover(float durasi)
    {
        siapMenerimaHover = false;
        if (durasi > 0f)
            yield return new WaitForSeconds(durasi);
        
        siapMenerimaHover = true;

        // Jika setelah tombol siap ternyata kursor saat ini sedang diam tepat di atas tombol ini, langsung picu hover secara halus!
        if (gunakanHoverDanClick && rectTransform != null && !DraggableUI.isInteractingWithUI)
        {
            bool kursorDiAtas = RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, null);
            if (kursorDiAtas && !sedangHover)
            {
                OnPointerEnter(null);
            }
        }
    }

    IEnumerator ProsesAnimasiStay()
    {
        // Offset acak agar jika ada beberapa tombol, gerakan nafas/float tidak serentak (terasa alami & dinamis)
        float t = Random.Range(0f, 6.28f);

        while (gunakanAnimasiStay && !sedangHover && !sedangDitekan)
        {
            t += Time.deltaTime * kecepatanStay;
            float sinVal = Mathf.Sin(t);

            if (modeStay == ModeStay.Diam)
            {
                // Diam saja (tanpa gerakan idle setelah muncul)
                break;
            }
            else if (modeStay == ModeStay.Breathing)
            {
                // Bernafas membesar-mengecil halus
                float scaleOffset = sinVal * intensitasSkalaStay;
                transform.localScale = skalaAwal * (1f + scaleOffset);
            }
            else if (modeStay == ModeStay.Floating)
            {
                // Mengambang naik-turun halus
                float posOffset = sinVal * intensitasPosisiStay;
                if (rectTransform != null) rectTransform.anchoredPosition = posisiAwal + new Vector2(0f, posOffset);
            }
            else if (modeStay == ModeStay.Wobble)
            {
                // Goyang miring santai
                float rotOffset = sinVal * intensitasRotasiStay;
                transform.localRotation = rotasiAwal * Quaternion.Euler(0f, 0f, rotOffset);
            }
            else if (modeStay == ModeStay.Spinning)
            {
                // Berputar 360 derajat terus-menerus
                sudutPutaran += Time.deltaTime * kecepatanPutarStay;
                if (sudutPutaran > 360f) sudutPutaran -= 360f;
                else if (sudutPutaran < -360f) sudutPutaran += 360f;
                transform.localRotation = rotasiAwal * Quaternion.Euler(0f, 0f, sudutPutaran);
            }
            else if (modeStay == ModeStay.BreathingDanSpinning)
            {
                // Gabungan Bernafas + Berputar 360 derajat
                float scaleOffset = sinVal * intensitasSkalaStay;
                transform.localScale = skalaAwal * (1f + scaleOffset);

                sudutPutaran += Time.deltaTime * kecepatanPutarStay;
                if (sudutPutaran > 360f) sudutPutaran -= 360f;
                else if (sudutPutaran < -360f) sudutPutaran += 360f;
                transform.localRotation = rotasiAwal * Quaternion.Euler(0f, 0f, sudutPutaran);
            }
            else if (modeStay == ModeStay.Kombinasi)
            {
                // Gabungan Breathing + Floating + Wobble
                float scaleOffset = sinVal * intensitasSkalaStay;
                float posOffset = Mathf.Sin(t * 0.8f) * intensitasPosisiStay;
                float rotOffset = Mathf.Sin(t * 0.6f) * intensitasRotasiStay;

                transform.localScale = skalaAwal * (1f + scaleOffset);
                if (rectTransform != null) rectTransform.anchoredPosition = posisiAwal + new Vector2(0f, posOffset);
                transform.localRotation = rotasiAwal * Quaternion.Euler(0f, 0f, rotOffset);
            }

            yield return null;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!siapMenerimaHover || !gunakanHoverDanClick || DraggableUI.isInteractingWithUI) return;

        sedangHover = true;
        if (canvasGroup != null && canvasGroup.alpha < 1f) canvasGroup.alpha = 1f;
        if (bawaKeDepanSaatHover)
        {
            indeksSiblingAwal = transform.GetSiblingIndex();
            transform.SetAsLastSibling();
        }

        StopAllCoroutines();
        StartCoroutine(TransisiKeTarget(skalaAwal * targetSkalaHover, posisiAwal + new Vector2(0f, 8f), rotasiAwal * Quaternion.Euler(0f, 0f, 2.5f)));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!siapMenerimaHover || !gunakanHoverDanClick || !sudahInisialisasi) return;

        sedangHover = false;
        sedangDitekan = false;
        if (bawaKeDepanSaatHover)
        {
            transform.SetSiblingIndex(indeksSiblingAwal);
        }

        StopAllCoroutines();
        StartCoroutine(TransisiKembaliDanStay());
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!siapMenerimaHover || !gunakanHoverDanClick || DraggableUI.isInteractingWithUI) return;

        sedangDitekan = true;
        StopAllCoroutines();
        // Efek squash / tekan tombol
        StartCoroutine(TransisiKeTarget(skalaAwal * targetSkalaClick, posisiAwal - new Vector2(0f, 3f), rotasiAwal));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!siapMenerimaHover || !gunakanHoverDanClick || !sudahInisialisasi) return;

        sedangDitekan = false;
        if (sedangHover)
        {
            StopAllCoroutines();
            StartCoroutine(TransisiKeTarget(skalaAwal * targetSkalaHover, posisiAwal + new Vector2(0f, 8f), rotasiAwal * Quaternion.Euler(0f, 0f, 2.5f)));
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(TransisiKembaliDanStay());
        }
    }

    IEnumerator TransisiKeTarget(Vector3 targetSkala, Vector2 targetPosisi, Quaternion targetRotasi)
    {
        while (sedangHover || sedangDitekan)
        {
            float step = Time.deltaTime * kecepatanHover;
            transform.localScale = Vector3.Lerp(transform.localScale, targetSkala, step);
            if (rectTransform != null) rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPosisi, step);

            if ((modeStay == ModeStay.Spinning || modeStay == ModeStay.BreathingDanSpinning) && gunakanAnimasiStay)
            {
                sudutPutaran += Time.deltaTime * kecepatanPutarStay;
                if (sudutPutaran > 360f) sudutPutaran -= 360f;
                else if (sudutPutaran < -360f) sudutPutaran += 360f;
                transform.localRotation = rotasiAwal * Quaternion.Euler(0f, 0f, sudutPutaran);
            }
            else
            {
                transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotasi, step);
                if (Vector3.Distance(transform.localScale, targetSkala) < 0.002f)
                {
                    transform.localScale = targetSkala;
                    if (rectTransform != null) rectTransform.anchoredPosition = targetPosisi;
                    transform.localRotation = targetRotasi;
                    break;
                }
            }

            yield return null;
        }
    }

    IEnumerator TransisiKembaliDanStay()
    {
        while (true)
        {
            float step = Time.deltaTime * kecepatanHover;
            transform.localScale = Vector3.Lerp(transform.localScale, skalaAwal, step);
            if (rectTransform != null) rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, posisiAwal, step);

            if ((modeStay == ModeStay.Spinning || modeStay == ModeStay.BreathingDanSpinning) && gunakanAnimasiStay)
            {
                sudutPutaran += Time.deltaTime * kecepatanPutarStay;
                if (sudutPutaran > 360f) sudutPutaran -= 360f;
                else if (sudutPutaran < -360f) sudutPutaran += 360f;
                transform.localRotation = rotasiAwal * Quaternion.Euler(0f, 0f, sudutPutaran);
            }
            else
            {
                transform.localRotation = Quaternion.Lerp(transform.localRotation, rotasiAwal, step);
            }

            if (Vector3.Distance(transform.localScale, skalaAwal) < 0.005f)
            {
                transform.localScale = skalaAwal;
                if (rectTransform != null) rectTransform.anchoredPosition = posisiAwal;
                if (!((modeStay == ModeStay.Spinning || modeStay == ModeStay.BreathingDanSpinning) && gunakanAnimasiStay))
                {
                    transform.localRotation = rotasiAwal;
                }
                break;
            }
            yield return null;
        }

        if (gunakanAnimasiStay && !sedangHover && !sedangDitekan)
        {
            StartCoroutine(ProsesAnimasiStay());
        }
    }

    /// <summary>
    /// Jalankan ulang animasi In dari posisi awal (misal saat panel dipanggil lagi).
    /// </summary>
    public void JalankanUlangAnimasiIn()
    {
        if (!sudahInisialisasi) return;
        StopAllCoroutines();
        sedangHover = false;
        sedangDitekan = false;
        if (gameObject.activeInHierarchy && gunakanAnimasiIn)
        {
            StartCoroutine(ProsesAnimasiIn());
        }
    }

    /// <summary>
    /// Jalankan animasi keluar (Animasi Out) lalu nonaktifkan GameObject atau jalankan callback setelah selesai.
    /// </summary>
    public void JalankanAnimasiOut(System.Action setelahOutSelesai = null, bool nonaktifkanSetelahOut = true)
    {
        if (!sudahInisialisasi)
        {
            InisialisasiAwal();
        }

        StopAllCoroutines();
        sedangHover = false;
        sedangDitekan = false;
        if (gameObject.activeInHierarchy && gunakanAnimasiOut)
        {
            StartCoroutine(ProsesAnimasiOut(setelahOutSelesai, nonaktifkanSetelahOut));
        }
        else
        {
            if (nonaktifkanSetelahOut) gameObject.SetActive(false);
            setelahOutSelesai?.Invoke();
        }
    }

    IEnumerator ProsesAnimasiOut(System.Action setelahOutSelesai, bool nonaktifkanSetelahOut)
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        Vector3 targetSkala = Vector3.zero;
        Vector2 targetPosisi = posisiAwal - new Vector2(0f, geserBawahAwal);
        Quaternion targetRotasi = rotasiAwal * Quaternion.Euler(0f, 0f, 6f);
        float targetAlpha = 0f;

        if (modeAnimasiOut == ModeAnimasiIn.PopInBawah)
        {
            targetSkala = Vector3.zero;
            targetPosisi = posisiAwal - new Vector2(0f, geserBawahAwal);
            targetAlpha = 0f;
        }
        else if (modeAnimasiOut == ModeAnimasiIn.SlideDariKiri)
        {
            targetSkala = transform.localScale;
            targetPosisi = posisiAwal - new Vector2(jarakSlideOut, 0f);
            targetRotasi = rotasiAwal;
            targetAlpha = 0f;
        }
        else if (modeAnimasiOut == ModeAnimasiIn.SlideDariKanan)
        {
            targetSkala = transform.localScale;
            targetPosisi = posisiAwal + new Vector2(jarakSlideOut, 0f);
            targetRotasi = rotasiAwal;
            targetAlpha = 0f;
        }
        else if (modeAnimasiOut == ModeAnimasiIn.SlideDariAtas)
        {
            targetSkala = transform.localScale;
            targetPosisi = posisiAwal + new Vector2(0f, jarakSlideOut);
            targetRotasi = rotasiAwal;
            targetAlpha = 0f;
        }
        else if (modeAnimasiOut == ModeAnimasiIn.SlideDariBawah)
        {
            targetSkala = transform.localScale;
            targetPosisi = posisiAwal - new Vector2(0f, jarakSlideOut);
            targetRotasi = rotasiAwal;
            targetAlpha = 0f;
        }
        else if (modeAnimasiOut == ModeAnimasiIn.FlipKartu3D)
        {
            targetSkala = skalaAwal * 0.8f;
            targetPosisi = posisiAwal;
            targetRotasi = rotasiAwal * Quaternion.Euler(0f, 90f, -12f);
            targetAlpha = 0f;
        }
        else if (modeAnimasiOut == ModeAnimasiIn.JatuhKartuMeja)
        {
            targetSkala = skalaAwal * 1.35f;
            targetPosisi = posisiAwal - new Vector2(0f, 350f);
            targetRotasi = rotasiAwal * Quaternion.Euler(0f, 0f, -16f);
            targetAlpha = 0f;
        }
        else if (modeAnimasiOut == ModeAnimasiIn.Fade)
        {
            targetSkala = skalaAwal;
            targetPosisi = posisiAwal;
            targetRotasi = rotasiAwal;
            targetAlpha = 0f;
        }

        Vector3 skalaStart = transform.localScale;
        Vector2 posisiStart = rectTransform != null ? rectTransform.anchoredPosition : posisiAwal;
        Quaternion rotasiStart = transform.localRotation;
        float alphaStart = canvasGroup != null ? canvasGroup.alpha : 1f;

        float waktuMulai = Time.time;
        while (Time.time < waktuMulai + durasiAnimasiOut)
        {
            float progress = (Time.time - waktuMulai) / durasiAnimasiOut;
            float easeIn = progress * progress; // EaseInQuad supaya makin cepat pas keluar

            transform.localScale = Vector3.LerpUnclamped(skalaStart, targetSkala, easeIn);
            if (rectTransform != null) rectTransform.anchoredPosition = Vector2.LerpUnclamped(posisiStart, targetPosisi, easeIn);
            transform.localRotation = Quaternion.Lerp(rotasiStart, targetRotasi, progress);
            if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(alphaStart, targetAlpha, progress);

            yield return null;
        }

        transform.localScale = targetSkala;
        if (rectTransform != null) rectTransform.anchoredPosition = targetPosisi;
        if (canvasGroup != null) canvasGroup.alpha = targetAlpha;

        if (nonaktifkanSetelahOut)
        {
            gameObject.SetActive(false);
        }

        setelahOutSelesai?.Invoke();
    }
}
