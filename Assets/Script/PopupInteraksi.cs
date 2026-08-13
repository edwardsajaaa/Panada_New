using UnityEngine;
using UnityEngine.Events;

public class PopupInteraksi : MonoBehaviour
{
    public enum TipeAnimasiGerak { Diam, NaikTurun, GeserSamping }
    [Header("Pengaturan Jarak")]
    [Tooltip("Jarak maksimal pemain dari objek ini agar ikon/popup muncul")]
    public float jarakInteraksi = 3f;

    [Tooltip("KHUSUS OTOMATIS: Jarak agar event benar-benar dipicu. Harus lebih kecil dari Jarak Interaksi agar ikon muncul lebih dulu.")]
    public float jarakOtomatis = 1f;

    [Tooltip("Pusat area interaksi. Jika titik kuning tidak pas di tengah objek, buat GameObject kosong di tengah objek, lalu masukkan ke sini. (Boleh dikosongkan)")]
    public Transform pusatInteraksi;
    
    [Tooltip("CENTANG INI jika karakter dan objek berada di dalam Canvas (seperti level Outdoor Anda) karena UI Canvas tidak menggunakan sistem Collider 3D")]
    public bool modeUICanvas = false;

    [Header("Interaksi Tombol (Opsional)")]
    [Tooltip("Tombol yang harus ditekan pemain untuk berinteraksi saat berada di dekat objek (misal: F)")]
    public KeyCode tombolInteraksi = KeyCode.F;
    
    [Tooltip("Centang ini jika event ingin langsung dijalankan OTOMATIS saat pemain masuk area, tanpa menekan tombol.")]
    public bool pemicuOtomatis = false;

    [Tooltip("Pilih ini jika interaksi ini berfungsi untuk Pindah Ruangan. Layar akan tertutup efek transisi pixel sejenak.")]
    public bool gunakanTransisiPindahRuang = false;

    [Tooltip("Event/Fungsi yang akan dijalankan ketika pemain menekan tombol interaksi di atas")]
    public UnityEvent saatDiinteraksi;

    [Header("Referensi")]
    [Tooltip("Objek visual Popup (misalnya sprite '?' atau Canvas UI) yang akan dimunculkan/disembunyikan")]
    public GameObject popupVisual;

    [Tooltip("Otomatis mencari karakter pemain dengan script PlayerMovement25D atau UI jika dikosongkan")]
    public Transform playerTransform;

    [Header("Pengaturan Visual")]
    [Tooltip("Centang agar popup selalu menghadap ke arah kamera (sangat berguna untuk game 2.5D/3D)")]
    public bool hadapKamera = true;
    [Tooltip("Gunakan animasi membesar/mengecil (skala) saat popup muncul dan hilang")]
    public bool gunakanAnimasiSkala = true;
    public float kecepatanAnimasi = 10f;

    [Header("Animasi Gerak Mengambang (Bouncing)")]
    [Tooltip("Animasi tambahan (naik-turun atau kiri-kanan) saat ikon sedang tampil")]
    public TipeAnimasiGerak tipeGerakTambahan = TipeAnimasiGerak.Diam;
    public float kecepatanBouncing = 5f;
    [Tooltip("Jarak bolak-balik (Isi sekitar 0.5 untuk 3D, atau 10 - 20 untuk UI Canvas)")]
    public float jarakBouncing = 10f;

    private bool sedangAktif = false;
    private Vector3 skalaAsli;
    private Vector3 posisiAwalPopup;
    private Camera cam;

    void Start()
    {
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
            PlayerMovementUI playerUI = FindAnyObjectByType<PlayerMovementUI>();
            PlayerMovement25D player25D = FindAnyObjectByType<PlayerMovement25D>();
            
            if (modeUICanvas && playerUI != null)
            {
                playerTransform = playerUI.transform;
            }
            else if (playerUI != null && playerUI.gameObject.activeInHierarchy)
            {
                playerTransform = playerUI.transform;
            }
            else if (player25D != null && player25D.gameObject.activeInHierarchy)
            {
                playerTransform = player25D.transform;
            }
            else
            {
                GameObject playerTag = GameObject.FindGameObjectWithTag("Player");
                if (playerTag != null) playerTransform = playerTag.transform;
            }
        }

        // Menyimpan ukuran & posisi asli popup dan menyembunyikannya di awal
        if (popupVisual != null)
        {
            skalaAsli = popupVisual.transform.localScale;
            posisiAwalPopup = popupVisual.transform.localPosition;
            
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

    [HideInInspector]
    public bool sembunyikanSementara = false; // Digunakan oleh script lain untuk menyembunyikan balon '?' sementara
    private bool sudahOtomatis = false;

    void Update()
    {
        if (popupVisual == null) return;

        sedangAktif = false;
        bool dalamAreaOtomatis = false;
        Vector3 titikPusat = pusatInteraksi != null ? pusatInteraksi.position : transform.position;

        if (modeUICanvas)
        {
            if (playerTransform != null)
            {
                float jarak = Vector2.Distance(titikPusat, playerTransform.position);
                if (jarak <= jarakInteraksi)
                {
                    sedangAktif = true;
                    if (jarak <= jarakOtomatis) dalamAreaOtomatis = true;
                }
            }
        }
        else
        {
            Collider[] hitColliders = Physics.OverlapSphere(titikPusat, jarakInteraksi);
            foreach (var hitCol in hitColliders)
            {
                if (hitCol.CompareTag("Player") || hitCol.transform == playerTransform)
                {
                    sedangAktif = true;
                    float jarak = Vector3.Distance(titikPusat, hitCol.transform.position);
                    if (jarak <= jarakOtomatis) dalamAreaOtomatis = true;
                    break;
                }
            }
        }

        bool terpicu = false;

        if (sedangAktif)
        {
            if (pemicuOtomatis && !sudahOtomatis && dalamAreaOtomatis)
            {
                terpicu = true;
                sudahOtomatis = true;
            }
            else if (!pemicuOtomatis && Input.GetKeyDown(tombolInteraksi))
            {
                terpicu = true;
            }
        }
        else
        {
            sudahOtomatis = false;
        }

        if (terpicu)
        {
            if (gunakanTransisiPindahRuang && TransisiRuangan.Instance != null)
            {
                TransisiRuangan.Instance.Jalankan(saatDiinteraksi);
            }
            else
            {
                saatDiinteraksi?.Invoke();
            }
        }

        bool harusTampil = sedangAktif && !sembunyikanSementara;

        if (gunakanAnimasiSkala)
        {
            if (harusTampil && !popupVisual.activeSelf)
            {
                popupVisual.SetActive(true);
            }

            Vector3 targetSkala = harusTampil ? skalaAsli : Vector3.zero;
            popupVisual.transform.localScale = Vector3.Lerp(popupVisual.transform.localScale, targetSkala, Time.deltaTime * kecepatanAnimasi);
            
            // Matikan objek jika skala sudah benar-benar mengecil habis untuk menghemat performa
            if (!harusTampil && popupVisual.transform.localScale.sqrMagnitude < 0.001f)
            {
                if (popupVisual.activeSelf) popupVisual.SetActive(false);
            }
        }
        else
        {
            if (popupVisual.activeSelf != harusTampil)
            {
                popupVisual.SetActive(harusTampil);
            }
        }

        if (harusTampil && popupVisual.activeSelf && tipeGerakTambahan != TipeAnimasiGerak.Diam)
        {
            float offset = Mathf.Sin(Time.time * kecepatanBouncing) * jarakBouncing;
            if (tipeGerakTambahan == TipeAnimasiGerak.NaikTurun)
            {
                popupVisual.transform.localPosition = posisiAwalPopup + new Vector3(0, offset, 0);
            }
            else if (tipeGerakTambahan == TipeAnimasiGerak.GeserSamping)
            {
                popupVisual.transform.localPosition = posisiAwalPopup + new Vector3(offset, 0, 0);
            }
        }
        else if (!harusTampil && popupVisual.activeSelf)
        {
            // Kembalikan ke posisi awal jika sedang tidak harus tampil tapi masih dalam proses menghilang
            popupVisual.transform.localPosition = posisiAwalPopup;
        }

        // Membuat popup selalu menghadap kamera (Billboarding) agar tidak miring di 2.5D
        if (hadapKamera && cam != null && popupVisual.activeSelf)
        {
            popupVisual.transform.rotation = cam.transform.rotation;
        }
    }
    
    // Fitur tambahan: Bantuan garis visual (bola kuning/merah) di Editor Unity untuk memudahkan mengatur jarak
    void OnDrawGizmosSelected()
    {
        Vector3 titikPusat = pusatInteraksi != null ? pusatInteraksi.position : transform.position;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(titikPusat, jarakInteraksi);

        if (pemicuOtomatis)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(titikPusat, jarakOtomatis);
        }
    }
}
