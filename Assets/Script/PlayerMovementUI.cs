using UnityEngine;

// Script khusus untuk menggerakkan Karakter yang dibuat di dalam Canvas (UI)
[RequireComponent(typeof(RectTransform))]
public class PlayerMovementUI : MonoBehaviour
{
    [Header("Pengaturan Pergerakan (Canvas UI)")]
    [Tooltip("Kecepatan jalan dalam hitungan Pixel per detik. Biasanya butuh angka besar (misal 300 - 500)")]
    public float kecepatanJalan = 300f;
    
    [Tooltip("Centang jika saat tekan Kiri malah ke Kanan")]
    public bool balikArahKiriKanan = false;

    [Header("Pengaturan Visual")]
    [Tooltip("Centang jika gambar asli karakter (saat baru dimasukkan) menghadap ke Kiri. Biarkan kosong jika menghadap Kanan.")]
    public bool gambarBawaanHadapKiri = false;

    [Header("Referensi")]
    [Tooltip("Otomatis dicari jika dikosongkan")]
    public Animator animatorKarakter;

    private RectTransform rectTransform;
    private bool menghadapKanan; // Akan diatur di Start

    // Komponen untuk trik sinkronisasi animasi 2D ke UI
    private SpriteRenderer dummySpriteRenderer;
    private UnityEngine.UI.Image uiImage;

    void Start()
    {
        // Atur arah hadap awal berdasarkan centang di Inspector
        menghadapKanan = !gambarBawaanHadapKiri;

        // Komponen wajib untuk objek UI
        rectTransform = GetComponent<RectTransform>();
        uiImage = GetComponent<UnityEngine.UI.Image>();
        dummySpriteRenderer = GetComponent<SpriteRenderer>();
        
        if (animatorKarakter == null) animatorKarakter = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (rectTransform == null) return;

        // Ambil input A/D atau Panah Kiri/Kanan
        float inputX = Input.GetAxisRaw("Horizontal");
        
        if (balikArahKiriKanan) inputX = -inputX;

        // Gerakkan posisi UI secara langsung berdasarkan Pixel per detik
        if (Mathf.Abs(inputX) > 0.01f)
        {
            rectTransform.anchoredPosition += new Vector2(inputX * kecepatanJalan * Time.deltaTime, 0f);
        }

        // --- MENGATUR ANIMASI ---
        if (animatorKarakter != null)
        {
            bool sedangBerjalan = Mathf.Abs(inputX) > 0.01f;
            animatorKarakter.SetBool("isWalking", sedangBerjalan);
        }

        // Logika membalik gambar karakter saat berbelok
        if (inputX > 0 && !menghadapKanan)
        {
            BalikArah();
        }
        else if (inputX < 0 && menghadapKanan)
        {
            BalikArah();
        }

        // Trik Ajaib: Salin animasi dari SpriteRenderer (milik 3D) ke Image (milik UI)
        if (dummySpriteRenderer != null && uiImage != null && dummySpriteRenderer.sprite != null)
        {
            uiImage.sprite = dummySpriteRenderer.sprite;
        }
    }

    void BalikArah()
    {
        menghadapKanan = !menghadapKanan;
        
        // Membalik arah dengan mengubah skala X menjadi negatif
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
