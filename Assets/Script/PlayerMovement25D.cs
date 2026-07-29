using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement25D : MonoBehaviour
{
    [Header("Pengaturan Pergerakan")]
    [Tooltip("Kecepatan jalan karakter")]
    public float kecepatanJalan = 5f;

    [Header("Referensi")]
    [Tooltip("Kosongkan jika komponen SpriteRenderer ada di objek ini langsung")]
    public SpriteRenderer spriteRendererKarakter;

    private Rigidbody rb;
    private Vector3 arahGerak;
    private bool menghadapKanan = true; // Asumsi default karakter menghadap kanan

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Coba cari SpriteRenderer secara otomatis jika belum diisi
        if (spriteRendererKarakter == null)
        {
            spriteRendererKarakter = GetComponentInChildren<SpriteRenderer>();
        }

        // Pastikan rigidbody disetel dengan benar untuk game 2.5D
        rb.freezeRotation = true; // Mencegah karakter jatuh terguling
    }

    void Update()
    {
        // Ambil input dari keyboard (W/A/S/D atau Panah)
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");

        // --- SOLUSI: Gerakan mengikuti arah Kamera Utama ---
        Transform camTransform = Camera.main.transform;
        Vector3 forward = camTransform.forward;
        Vector3 right = camTransform.right;

        // Kita hanya butuh gerakan di bidang datar (X dan Z), hilangkan sumbu Y
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        // Hitung arah gerakan sesungguhnya relatif terhadap layar
        arahGerak = (right * inputX + forward * inputZ).normalized;

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
        // Terapkan kecepatan pada Rigidbody, biarkan sumbu Y (gravitasi) apa adanya
        Vector3 targetKecepatan = new Vector3(arahGerak.x * kecepatanJalan, rb.velocity.y, arahGerak.z * kecepatanJalan);
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
