using UnityEngine;
using UnityEngine.Events;

public class PopupInteraksi : MonoBehaviour
{
    [Header("Pengaturan Jarak")]
    [Tooltip("Jarak maksimal pemain dari objek ini agar popup muncul")]
    public float jarakInteraksi = 3f;

    [Tooltip("Pusat area interaksi. Jika titik kuning tidak pas di tengah objek, buat GameObject kosong di tengah objek, lalu masukkan ke sini. (Boleh dikosongkan)")]
    public Transform pusatInteraksi;

    [Header("Interaksi Tombol (Opsional)")]
    [Tooltip("Tombol yang harus ditekan pemain untuk berinteraksi saat berada di dekat objek (misal: F)")]
    public KeyCode tombolInteraksi = KeyCode.F;
    
    [Tooltip("Event/Fungsi yang akan dijalankan ketika pemain menekan tombol interaksi di atas")]
    public UnityEvent saatDiinteraksi;

    [Header("Referensi")]
    [Tooltip("Objek visual Popup (misalnya sprite '?' atau Canvas UI) yang akan dimunculkan/disembunyikan")]
    public GameObject popupVisual;

    [Tooltip("Otomatis mencari karakter pemain dengan script PlayerMovement25D jika dikosongkan")]
    public Transform playerTransform;

    [Header("Pengaturan Visual")]
    [Tooltip("Centang agar popup selalu menghadap ke arah kamera (sangat berguna untuk game 2.5D/3D)")]
    public bool hadapKamera = true;
    [Tooltip("Gunakan animasi membesar/mengecil (skala) saat popup muncul dan hilang")]
    public bool gunakanAnimasiSkala = true;
    public float kecepatanAnimasi = 10f;

    private bool sedangAktif = false;
    private Vector3 skalaAsli;
    private Camera cam;

    void Start()
    {
        // Cari kamera aktif di scene
        if (Camera.main != null)
        {
            cam = Camera.main;
        }
        else
        {
            cam = FindAnyObjectByType<Camera>();
        }

        // Mencari karakter pemain secara otomatis jika belum diisi
        if (playerTransform == null)
        {
            PlayerMovement25D player = FindAnyObjectByType<PlayerMovement25D>();
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                GameObject playerTag = GameObject.FindGameObjectWithTag("Player");
                if (playerTag != null) playerTransform = playerTag.transform;
            }
        }

        // Menyimpan ukuran asli popup dan menyembunyikannya di awal
        if (popupVisual != null)
        {
            skalaAsli = popupVisual.transform.localScale;
            
            if (gunakanAnimasiSkala)
            {
                popupVisual.transform.localScale = Vector3.zero;
                popupVisual.SetActive(false); 
            }
            else
            {
                popupVisual.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (playerTransform == null || popupVisual == null) return;

        // Mengecek apakah ADA collider pemain di dalam area bola kuning
        Vector3 titikPusat = pusatInteraksi != null ? pusatInteraksi.position : transform.position;
        
        // Dapatkan semua collider yang menyentuh area bola kuning
        Collider[] hitColliders = Physics.OverlapSphere(titikPusat, jarakInteraksi);
        
        sedangAktif = false;
        foreach (var hitCol in hitColliders)
        {
            // Jika yang menyentuh adalah pemain (berdasarkan tag atau transform)
            if (hitCol.CompareTag("Player") || hitCol.transform == playerTransform)
            {
                sedangAktif = true;
                break; // Cukup satu yang ketemu, langsung keluar loop
            }
        }

        // Jika pemain berada di area dan menekan tombol interaksi, jalankan event-nya!
        if (sedangAktif && Input.GetKeyDown(tombolInteraksi))
        {
            saatDiinteraksi?.Invoke();
        }

        // Terapkan animasi skala atau langsung aktif/nonaktif
        if (gunakanAnimasiSkala)
        {
            if (sedangAktif && !popupVisual.activeSelf)
            {
                popupVisual.SetActive(true); // Nyalakan objek sebelum animasinya mulai membesar
            }

            Vector3 targetSkala = sedangAktif ? skalaAsli : Vector3.zero;
            popupVisual.transform.localScale = Vector3.Lerp(popupVisual.transform.localScale, targetSkala, Time.deltaTime * kecepatanAnimasi);
            
            // Matikan objek jika skala sudah benar-benar mengecil habis untuk menghemat performa
            if (!sedangAktif && popupVisual.transform.localScale.sqrMagnitude < 0.001f)
            {
                if (popupVisual.activeSelf) popupVisual.SetActive(false);
            }
        }
        else
        {
            // Jika tidak pakai animasi, langsung matikan/nyalakan
            if (popupVisual.activeSelf != sedangAktif)
            {
                popupVisual.SetActive(sedangAktif);
            }
        }

        // Membuat popup selalu menghadap kamera (Billboarding) agar tidak miring di 2.5D
        if (hadapKamera && cam != null && popupVisual.activeSelf)
        {
            popupVisual.transform.rotation = cam.transform.rotation;
        }
    }
    
    // Fitur tambahan: Bantuan garis visual (bola kuning) di Editor Unity untuk memudahkan mengatur jarak
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 titikPusat = pusatInteraksi != null ? pusatInteraksi.position : transform.position;
        Gizmos.DrawWireSphere(titikPusat, jarakInteraksi);
    }
}
