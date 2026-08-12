using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement25D : MonoBehaviour
{
    [Header("Pengaturan Pergerakan")]
    [Tooltip("Kecepatan jalan karakter")]
    public float kecepatanJalan = 5f;
    [Tooltip("Kecepatan lari karakter")]
    public float kecepatanLari = 8f;
    [Tooltip("Tombol keyboard untuk lari")]
    public KeyCode tombolLari = KeyCode.LeftShift;
    
    [Tooltip("Centang ini untuk area Outdoor (2D Side-scrolling) agar karakter HANYA bisa bergerak ke kiri dan kanan.")]
    public bool hanyaKiriKanan = false;
    [Tooltip("Centang jika saat tekan Kiri malah ke Kanan, atau sebaliknya (Berguna jika posisi kamera membelakangi map)")]
    public bool balikArahKiriKanan = false;

    [Header("Pengaturan Animasi Lari")]
    [Tooltip("Centang jika Anda TIDAK PUNYA animasi lari khusus. Script akan otomatis memutar animasi jalan 1.5x lebih cepat saat berlari.")]
    public bool percepatAnimasiJalanSaja = true;

    [Header("Referensi")]
    [Tooltip("Kosongkan jika komponen SpriteRenderer ada di objek ini langsung")]
    public SpriteRenderer spriteRendererKarakter;
    [Tooltip("Otomatis dicari jika dikosongkan")]
    public Animator animatorKarakter;

    private Rigidbody rb;
    private Vector3 arahGerak;
    private bool menghadapKanan = true; // Asumsi default karakter menghadap kanan
    private Transform camTransform;
    private bool sedangLari = false; // Status lari saat ini

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Coba cari komponen secara otomatis jika belum diisi
        if (spriteRendererKarakter == null) spriteRendererKarakter = GetComponentInChildren<SpriteRenderer>();
        if (animatorKarakter == null) animatorKarakter = GetComponentInChildren<Animator>();

        // Pastikan rigidbody disetel dengan benar untuk game 2.5D
        rb.freezeRotation = true; // Mencegah karakter jatuh terguling

        // Cari referensi kamera agar pergerakan sesuai dengan sudut pandang
        if (Camera.main != null)
        {
            camTransform = Camera.main.transform;
        }
        else
        {
            // Jika tidak ada kamera dengan tag "MainCamera", cari sembarang kamera
            Camera sembarangKamera = FindAnyObjectByType<Camera>();
            if (sembarangKamera != null) camTransform = sembarangKamera.transform;
        }
    }

    void Update()
    {
        // --- SISTEM BLOKIR: Hentikan karakter saat UI Panel sedang terbuka ---
        if (SistemBlokirGerak.SedangBukaUI())
        {
            arahGerak = Vector3.zero;
            sedangLari = false;
            if (animatorKarakter != null) animatorKarakter.SetBool("isWalking", false);
            return; // Berhenti memproses input
        }

        // Cek apakah tombol lari ditekan
        sedangLari = Input.GetKey(tombolLari);

        // Ambil input dari keyboard (W/A/S/D atau Panah)
        float inputX = Input.GetAxisRaw("Horizontal");

        if (hanyaKiriKanan)
        {
            // Jika arah kontrol kebalik di layar, balikkan nilai input-nya
            if (balikArahKiriKanan) inputX = -inputX;
            
            // Untuk 2D murni, lupakan arah kamera. Paksa jalan HANYA di sumbu X dunia.
            // Ini mengatasi masalah "jalan berat/seret" akibat karakter terdorong menabrak tembok (sumbu Z).
            arahGerak = new Vector3(inputX, 0f, 0f);
        }
        else
        {
            float inputZ = Input.GetAxisRaw("Vertical");

            // --- SOLUSI: Gerakan mengikuti arah Kamera Utama ---
            Vector3 forward = Vector3.forward;
            Vector3 right = Vector3.right;

            if (camTransform != null)
            {
                forward = camTransform.forward;
                right = camTransform.right;
            }

            // Kita hanya butuh gerakan di bidang datar (X dan Z), hilangkan sumbu Y
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            // Hitung arah gerakan sesungguhnya relatif terhadap layar
            arahGerak = (right * inputX + forward * inputZ).normalized;
        }

        // --- MENGATUR ANIMASI ---
        if (animatorKarakter != null)
        {
            // Jika ada arah gerak, berarti sedang berjalan/berlari
            bool sedangGerak = arahGerak.sqrMagnitude > 0.01f;
            
            if (percepatAnimasiJalanSaja)
            {
                // Cara 1: Mempercepat animasi jalan biasa (Tidak butuh parameter isRunning)
                animatorKarakter.SetBool("isWalking", sedangGerak);
                
                if (sedangGerak && sedangLari)
                    animatorKarakter.speed = 1.6f; // Putar animasi 1.6x lebih cepat
                else
                    animatorKarakter.speed = 1f;   // Kecepatan normal
            }
            else
            {
                // Cara 2: Menggunakan animasi lari terpisah (Butuh parameter bool "isRunning" di Animator)
                animatorKarakter.speed = 1f; // Pastikan kecepatan tetap normal
                
                if (sedangGerak)
                {
                    if (sedangLari)
                    {
                        animatorKarakter.SetBool("isWalking", false);
                        
                        // Gunakan try-catch atau logika aman untuk mencegah error jika parameter belum dibuat
                        foreach (AnimatorControllerParameter param in animatorKarakter.parameters)
                        {
                            if (param.name == "isRunning") animatorKarakter.SetBool("isRunning", true);
                        }
                    }
                    else
                    {
                        animatorKarakter.SetBool("isWalking", true);
                        
                        foreach (AnimatorControllerParameter param in animatorKarakter.parameters)
                        {
                            if (param.name == "isRunning") animatorKarakter.SetBool("isRunning", false);
                        }
                    }
                }
                else
                {
                    animatorKarakter.SetBool("isWalking", false);
                    foreach (AnimatorControllerParameter param in animatorKarakter.parameters)
                    {
                        if (param.name == "isRunning") animatorKarakter.SetBool("isRunning", false);
                    }
                }
            }
        }

        // Logika membalik arah (menghadap kiri/kanan) berdasarkan tombol yang ditekan
        if (inputX > 0 && !menghadapKanan)
        {
            BalikArah();
        }
        else if (inputX < 0 && menghadapKanan)
        {
            BalikArah();
        }
    }

    void FixedUpdate()
    {
        // Tentukan kecepatan saat ini (Lari atau Jalan)
        float kecepatanSaatIni = sedangLari ? kecepatanLari : kecepatanJalan;

        // Terapkan kecepatan pada Rigidbody, biarkan sumbu Y (gravitasi) apa adanya
        Vector3 targetKecepatan = new Vector3(arahGerak.x * kecepatanSaatIni, rb.velocity.y, arahGerak.z * kecepatanSaatIni);
        rb.velocity = targetKecepatan;
    }

    void BalikArah()
    {
        menghadapKanan = !menghadapKanan;

        if (spriteRendererKarakter != null)
        {
            // Membalik menggunakan fitur bawaan SpriteRenderer
            spriteRendererKarakter.flipX = !menghadapKanan;
        }
        else
        {
            // Fallback: membalik menggunakan localScale jika tidak ada SpriteRenderer
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }
}
