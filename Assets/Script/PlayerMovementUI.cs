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

    [Header("Referensi")]
    [Tooltip("Otomatis dicari jika dikosongkan")]
    public Animator animatorKarakter;

    private RectTransform rectTransform;
    private bool menghadapKanan = true; // Asumsi karakter awalnya menghadap kanan

    void Start()
    {
        // Komponen wajib untuk objek UI
        rectTransform = GetComponent<RectTransform>();
        
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
