using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AnimasiHandphoneKamar : MonoBehaviour
{
    [Header("=== 1. Pengaturan Waktu ===")]
    [Tooltip("Waktu tunggu (dalam detik) sebelum HP menyala")]
    public float waktuTunggu = 4.5f;

    [Header("=== 2. Pengaturan Visual ===")]
    [Tooltip("Masukkan GameObject/Image HP yang MENYALA (terang) ke sini.")]
    public GameObject visualHpMenyala;
    
    [Header("=== 3. Pengaturan Zoom & Event ===")]
    [Tooltip("Centang jika Anda ingin script otomatis melakukan zoom.")]
    public bool gunakanZoomOtomatisScriptIni = true;
    
    [Tooltip("PILIHAN A: Jika ingin Kamera yang nge-zoom, masukkan Main Camera ke sini (Pastikan Canvas Anda mode Screen Space - Camera)")]
    public Camera kameraYangAkanDizoom;

    [Tooltip("PILIHAN B: Jika ingin UI Panel yang nge-zoom, masukkan panel meja ke sini (Biarkan kosong jika pakai Kamera)")]
    public RectTransform panelMejaYangAkanDizoom;
    
    [Space(5)]
    [Tooltip("OPSIONAL: Titik fokus zoom. Jika KOSONG, otomatis nge-zoom ke HP ini. Jika ingin nge-zoom ke area lain, buat objek kosong di dalam Meja, taruh di posisi yang diinginkan, lalu tarik ke sini.")]
    public RectTransform titikFokusZoom;

    public float durasiZoom = 1.5f;
    [Tooltip("Jika pakai Kamera, angka 3 berarti layar akan 3x lebih dekat (zoom in)")]
    public float targetSkalaZoom = 3f;

    [Space(10)]
    [Tooltip("Event tambahan jika ingin menjalankan sesuatu saat HP ditekan otomatis.")]
    public UnityEvent eventSaatHpMenyala;

    void Start()
    {
        if (visualHpMenyala != null)
            visualHpMenyala.SetActive(false);

        StartCoroutine(ProsesHandphoneMenyala());
    }

    IEnumerator ProsesHandphoneMenyala()
    {
        yield return new WaitForSeconds(waktuTunggu);

        if (visualHpMenyala != null)
            visualHpMenyala.SetActive(true);

        if (eventSaatHpMenyala != null)
            eventSaatHpMenyala.Invoke();

        Button tombolHp = GetComponent<Button>();
        if (tombolHp != null)
            tombolHp.onClick.Invoke();

        if (gunakanZoomOtomatisScriptIni)
        {
            if (kameraYangAkanDizoom != null)
            {
                StartCoroutine(AnimasiZoomKamera());
            }
            else if (panelMejaYangAkanDizoom != null)
            {
                StartCoroutine(AnimasiZoomPanelMeja());
            }
        }
    }

    IEnumerator AnimasiZoomPanelMeja()
    {
        // 1. Dapatkan referensi titik yang akan dituju (Fokus khusus atau HP ini sendiri)
        RectTransform targetRect = titikFokusZoom != null ? titikFokusZoom : GetComponent<RectTransform>();
        Canvas canvas = GetComponentInParent<Canvas>();
        
        Vector3 posisiTengahLayar = canvas != null ? 
            canvas.transform.TransformPoint(canvas.GetComponent<RectTransform>().rect.center) : 
            new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);

        // 2. Simpan status awal
        Vector3 skalaAwal = panelMejaYangAkanDizoom.localScale;
        Vector3 posisiAwal = panelMejaYangAkanDizoom.position;
        Vector3 skalaAkhir = skalaAwal * targetSkalaZoom;

        // 3. Hitung target posisi akhir Panel Meja secara matematis
        Vector3 offsetTargetAwal = targetRect.position - posisiAwal;
        float rasioSkala = targetSkalaZoom / skalaAwal.x;
        Vector3 posisiAkhir = posisiTengahLayar - (offsetTargetAwal * rasioSkala);

        // --- FITUR BARU: CLAMPING AGAR TIDAK KELUAR BATAS LAYAR ---
        // Ambil ukuran layar/canvas saat ini di world space
        Vector3[] canvasCorners = new Vector3[4];
        canvasRect.GetWorldCorners(canvasCorners);
        float screenW = canvasCorners[2].x - canvasCorners[0].x;
        float screenH = canvasCorners[2].y - canvasCorners[0].y;

        // Ambil ukuran Panel Meja saat ini di world space, lalu proyeksikan ukurannya di masa depan (setelah zoom)
        Vector3[] panelCorners = new Vector3[4];
        panelMejaYangAkanDizoom.GetWorldCorners(panelCorners);
        float panelW = (panelCorners[2].x - panelCorners[0].x) * rasioSkala;
        float panelH = (panelCorners[2].y - panelCorners[0].y) * rasioSkala;

        // Hitung batas maksimal pergeseran (selisih ukuran dibagi 2)
        float marginX = Mathf.Max(0, (panelW - screenW) / 2f);
        float marginY = Mathf.Max(0, (panelH - screenH) / 2f);

        // Cari titik "netral" yaitu posisi pivot Meja jika tengah-tengah Meja persis berada di tengah layar
        // Offset dari pivot ke center pada skala awal = Center - Pivot(Position)
        Vector3 currentPanelCenter = (panelCorners[2] + panelCorners[0]) / 2f;
        Vector3 pivotToCenterAwal = currentPanelCenter - posisiAwal;
        // Offset dari pivot ke center pada skala akhir
        Vector3 pivotToCenterAkhir = pivotToCenterAwal * rasioSkala;
        
        // Titik netral: jika center Meja ada di posisiTengahLayar, maka pivotnya ada di:
        Vector3 neutralPos = posisiTengahLayar - pivotToCenterAkhir;

        // Batasi (Clamp) posisi akhir agar tidak melebihi margin dari titik netral!
        posisiAkhir.x = Mathf.Clamp(posisiAkhir.x, neutralPos.x - marginX, neutralPos.x + marginX);
        posisiAkhir.y = Mathf.Clamp(posisiAkhir.y, neutralPos.y - marginY, neutralPos.y + marginY);
        // -----------------------------------------------------------

        // 4. Jalankan animasi perlahan (Lerp)
        float elapsed = 0f;
        while (elapsed < durasiZoom)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / durasiZoom);

            panelMejaYangAkanDizoom.localScale = Vector3.Lerp(skalaAwal, skalaAkhir, t);
            panelMejaYangAkanDizoom.position = Vector3.Lerp(posisiAwal, posisiAkhir, t);

            yield return null;
        }
        
        panelMejaYangAkanDizoom.localScale = skalaAkhir;
        panelMejaYangAkanDizoom.position = posisiAkhir;
    }

    IEnumerator AnimasiZoomKamera()
    {
        float sizeAwal = kameraYangAkanDizoom.orthographicSize;
        float sizeAkhir = sizeAwal / targetSkalaZoom;

        Vector3 posisiAwalKamera = kameraYangAkanDizoom.transform.position;
        Vector3 posisiHP = transform.position;
        Vector3 posisiAkhirKamera = new Vector3(posisiHP.x, posisiHP.y, posisiAwalKamera.z);

        float elapsed = 0f;
        while (elapsed < durasiZoom)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / durasiZoom);

            kameraYangAkanDizoom.orthographicSize = Mathf.Lerp(sizeAwal, sizeAkhir, t);
            kameraYangAkanDizoom.transform.position = Vector3.Lerp(posisiAwalKamera, posisiAkhirKamera, t);
            
            yield return null;
        }

        kameraYangAkanDizoom.orthographicSize = sizeAkhir;
        kameraYangAkanDizoom.transform.position = posisiAkhirKamera;
    }
}
