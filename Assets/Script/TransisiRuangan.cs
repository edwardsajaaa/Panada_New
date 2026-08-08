using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// Script mandiri untuk transisi pixel antar ruangan (tanpa pindah Scene).
/// Tempel script ini langsung di objek PixelOverlay/Panel Anda.
/// RawImage anak akan dicari secara OTOMATIS jika tidak diisi manual.
/// </summary>
public class TransisiRuangan : MonoBehaviour
{
    public static TransisiRuangan Instance;

    [Header("Referensi")]
    [Tooltip("RawImage untuk loading (OTOMATIS dicari dari anak jika dikosongkan)")]
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
    private bool sedangTransisi = false;

    void Awake()
    {
        Instance = this;
        SiapkanKomponen();
    }

    void Start()
    {
        // Failsafe: sembunyikan lagi di Start untuk jaga-jaga
        SembunyikanSemua();
    }

    void SiapkanKomponen()
    {
        // GARANSI: Objek ini PASTI punya Canvas sendiri
        Canvas c = GetComponent<Canvas>();
        if (c == null)
            c = gameObject.AddComponent<Canvas>();
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
        layarTransisi = GetComponent<Image>();
        if (layarTransisi == null)
            layarTransisi = gameObject.AddComponent<Image>();
        layarTransisi.color = Color.black;
        layarTransisi.raycastTarget = true;

        if (materialTransisi != null)
        {
            layarTransisi.material = materialTransisi;
            materialTransisi.SetFloat("_Progress", 1f);
        }

        // Pastikan layar menutupi seluruh layar
        RectTransform rt = GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // --- OTOMATIS CARI RAWIMAGE ANAK JIKA BELUM DIISI ---
        if (rawImageLoading == null)
        {
            rawImageLoading = GetComponentInChildren<RawImage>(true);
        }
        // Ukuran dan posisi RawImage menggunakan pengaturan asli dari Inspector Anda

        // Sembunyikan semuanya
        SembunyikanSemua();
    }

    void SembunyikanSemua()
    {
        if (!sedangTransisi)
        {
            if (layarTransisi != null) layarTransisi.enabled = false;
            if (rawImageLoading != null) rawImageLoading.gameObject.SetActive(false);
        }
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

        sedangTransisi = true;

        // ====== TAHAP 1: TRANSISI IN (Layar perlahan tertutup pixel hitam) ======
        layarTransisi.enabled = true;
        // RawImage loading masih MATI TOTAL di tahap ini!

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
        if (rawImageLoading != null)
        {
            rawImageLoading.gameObject.SetActive(true);

            // Restart video dari awal agar tidak stuck
            UnityEngine.Video.VideoPlayer vp = rawImageLoading.GetComponent<UnityEngine.Video.VideoPlayer>();
            if (vp != null)
            {
                vp.Stop();
                vp.Play();
            }
        }

        // Jeda agar pemain bisa melihat animasi loading
        yield return new WaitForSeconds(jedaDiTengah);

        // ====== TAHAP 3: JALANKAN EVENT PERPINDAHAN RUANGAN ======
        eventTengah?.Invoke();
        yield return new WaitForEndOfFrame();

        // ====== TAHAP 4: SEMBUNYIKAN RAW IMAGE LOADING DULU ======
        if (rawImageLoading != null)
        {
            UnityEngine.Video.VideoPlayer vp = rawImageLoading.GetComponent<UnityEngine.Video.VideoPlayer>();
            if (vp != null) vp.Stop();
            rawImageLoading.gameObject.SetActive(false);
        }

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
        sedangTransisi = false;
    }
}
