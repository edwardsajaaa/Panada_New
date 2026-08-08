using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// Script mandiri untuk transisi pixel antar ruangan (tanpa pindah Scene).
/// Tempel script ini langsung di objek PixelOverlay Anda.
/// </summary>
public class TransisiRuangan : MonoBehaviour
{
    public static TransisiRuangan Instance;

    [Header("Referensi")]
    [Tooltip("Drag RawImage anak dari PixelOverlay ke sini")]
    public RawImage rawImageTransisi;

    [Tooltip("Drag material PixelTransitionMaterial ke sini")]
    public Material materialTransisi;

    [Header("Pengaturan")]
    [Tooltip("Durasi efek pixel menutup/membuka layar (dalam detik)")]
    public float durasiTransisi = 0.5f;

    [Tooltip("Jeda saat layar gelap sebelum ruangan berganti (dalam detik)")]
    public float jedaDiTengah = 0.3f;

    void Awake()
    {
        Instance = this;

        // GARANSI: Objek ini PASTI punya Canvas sendiri agar selalu terlihat di layar,
        // mau ditaruh di mana saja di Hierarchy!
        Canvas c = GetComponent<Canvas>();
        if (c == null)
        {
            c = gameObject.AddComponent<Canvas>();
        }
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 9999; // Paling depan menutupi segalanya

        // Tambahkan GraphicRaycaster agar bisa memblokir input pemain saat transisi
        if (GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();

        // Tambahkan CanvasScaler agar ukurannya selalu Full HD
        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null)
            scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Sembunyikan di awal agar tidak menutupi game
        if (rawImageTransisi != null) rawImageTransisi.enabled = false;
        if (materialTransisi != null) materialTransisi.SetFloat("_Progress", 1f);
    }

    /// <summary>
    /// Panggil fungsi ini untuk menjalankan transisi.
    /// Event yang diberikan akan dijalankan saat layar gelap (di tengah transisi).
    /// </summary>
    public void Jalankan(UnityEvent eventDiTengah)
    {
        StartCoroutine(ProsesTransisi(eventDiTengah));
    }

    IEnumerator ProsesTransisi(UnityEvent eventTengah)
    {
        if (materialTransisi == null || rawImageTransisi == null)
        {
            Debug.LogError("TransisiRuangan: Material atau RawImage belum diisi!");
            eventTengah?.Invoke(); // Tetap jalankan event walau tanpa transisi
            yield break;
        }

        // 1. Tampilkan RawImage transisi
        rawImageTransisi.enabled = true;

        // 2. TRANSISI IN: Layar perlahan tertutup efek pixel (Progress 1 -> 0)
        float timer = 0f;
        while (timer < durasiTransisi)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Lerp(1f, 0f, timer / durasiTransisi);
            materialTransisi.SetFloat("_Progress", progress);
            yield return null;
        }
        materialTransisi.SetFloat("_Progress", 0f); // Layar 100% gelap/tertutup

        // 3. JEDA: Biarkan layar gelap sejenak
        yield return new WaitForSeconds(jedaDiTengah);

        // 4. JALANKAN EVENT: Pindah ruangan (Kamar mati, Outdoor nyala, dsb.)
        eventTengah?.Invoke();

        // 5. Tunggu 1 frame agar Unity sempat memproses perpindahan objek
        yield return new WaitForEndOfFrame();

        // 6. TRANSISI OUT: Layar perlahan terbuka menampilkan ruangan baru (Progress 0 -> 1)
        timer = 0f;
        while (timer < durasiTransisi)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Lerp(0f, 1f, timer / durasiTransisi);
            materialTransisi.SetFloat("_Progress", progress);
            yield return null;
        }
        materialTransisi.SetFloat("_Progress", 1f); // Layar 100% jernih/terbuka

        // 7. Sembunyikan kembali RawImage agar tidak menghalangi permainan
        rawImageTransisi.enabled = false;
    }
}
