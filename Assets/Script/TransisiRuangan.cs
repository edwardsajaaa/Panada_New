using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// Script mandiri untuk transisi pixel antar ruangan (tanpa pindah Scene).
/// Tempel script ini langsung di objek PixelOverlay Anda.
/// 
/// Alur transisi:
/// 1. Layar tertutup efek pixel (Transisi IN)
/// 2. Setelah benar-benar gelap, RawImage loading muncul
/// 3. Jeda sejenak
/// 4. RawImage loading menghilang
/// 5. Event dijalankan (pindah ruangan)
/// 6. Layar terbuka efek pixel (Transisi OUT)
/// </summary>
public class TransisiRuangan : MonoBehaviour
{
    public static TransisiRuangan Instance;

    [Header("Referensi")]
    [Tooltip("Drag RawImage anak dari PixelOverlay ke sini (untuk menampilkan gambar/video loading)")]
    public RawImage rawImageLoading;

    [Tooltip("Drag material PixelTransitionMaterial ke sini")]
    public Material materialTransisi;

    [Header("Pengaturan")]
    [Tooltip("Durasi efek pixel menutup/membuka layar (dalam detik)")]
    public float durasiTransisi = 0.5f;

    [Tooltip("Jeda saat layar gelap menampilkan loading (dalam detik)")]
    public float jedaDiTengah = 1.0f;

    // Image hitam yang dibuat otomatis untuk efek transisi pixel
    private Image layarTransisi;

    void Awake()
    {
        Instance = this;

        // GARANSI: Objek ini PASTI punya Canvas sendiri agar selalu terlihat di layar
        Canvas c = GetComponent<Canvas>();
        if (c == null)
        {
            c = gameObject.AddComponent<Canvas>();
        }
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 9999;

        if (GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null)
            scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // --- BUAT LAYAR HITAM OTOMATIS UNTUK EFEK TRANSISI ---
        // Ini adalah lapisan hitam di belakang RawImage loading.
        // Material pixel ditempel di sini, bukan di RawImage loading.
        layarTransisi = GetComponent<Image>();
        if (layarTransisi == null)
        {
            layarTransisi = gameObject.AddComponent<Image>();
        }
        layarTransisi.color = Color.black;
        layarTransisi.raycastTarget = true;

        // Pasang material transisi ke layar hitam ini
        if (materialTransisi != null)
        {
            layarTransisi.material = materialTransisi;
            materialTransisi.SetFloat("_Progress", 1f); // Mulai transparan (tak terlihat)
        }

        // Pastikan layar hitam menutupi seluruh layar
        RectTransform rt = GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Sembunyikan layar transisi dan RawImage loading di awal
        layarTransisi.enabled = false;
        if (rawImageLoading != null) rawImageLoading.enabled = false;
    }

    /// <summary>
    /// Panggil fungsi ini untuk menjalankan transisi pindah ruangan.
    /// </summary>
    public void Jalankan(UnityEvent eventDiTengah)
    {
        StartCoroutine(ProsesTransisi(eventDiTengah));
    }

    IEnumerator ProsesTransisi(UnityEvent eventTengah)
    {
        if (materialTransisi == null || layarTransisi == null)
        {
            Debug.LogError("TransisiRuangan: Material belum diisi!");
            eventTengah?.Invoke();
            yield break;
        }

        // ====== TAHAP 1: TRANSISI IN (Layar perlahan tertutup pixel hitam) ======
        layarTransisi.enabled = true;          // Tampilkan layar transisi
        // RawImage loading masih TERSEMBUNYI di tahap ini!

        float timer = 0f;
        while (timer < durasiTransisi)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Lerp(1f, 0f, timer / durasiTransisi);
            materialTransisi.SetFloat("_Progress", progress);
            yield return null;
        }
        materialTransisi.SetFloat("_Progress", 0f); // Layar 100% gelap

        // ====== TAHAP 2: LAYAR SUDAH GELAP TOTAL, MUNCULKAN RAW IMAGE LOADING ======
        if (rawImageLoading != null) rawImageLoading.enabled = true;

        // Jeda agar pemain bisa melihat animasi loading
        yield return new WaitForSeconds(jedaDiTengah);

        // ====== TAHAP 3: JALANKAN EVENT PERPINDAHAN RUANGAN ======
        eventTengah?.Invoke();
        yield return new WaitForEndOfFrame();

        // ====== TAHAP 4: SEMBUNYIKAN RAW IMAGE LOADING DULU ======
        if (rawImageLoading != null) rawImageLoading.enabled = false;

        // Tunggu sebentar agar perpindahan bersih
        yield return new WaitForSeconds(0.1f);

        // ====== TAHAP 5: TRANSISI OUT (Layar perlahan terbuka menampilkan ruangan baru) ======
        timer = 0f;
        while (timer < durasiTransisi)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Lerp(0f, 1f, timer / durasiTransisi);
            materialTransisi.SetFloat("_Progress", progress);
            yield return null;
        }
        materialTransisi.SetFloat("_Progress", 1f); // Layar 100% jernih

        // ====== TAHAP 6: SELESAI, SEMBUNYIKAN SEMUANYA ======
        layarTransisi.enabled = false;
    }
}
