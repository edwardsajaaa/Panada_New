using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Events;

[System.Serializable]
public class BarisDialogBergantian
{
    [Tooltip("Objek Gelembung siapa yang mau dinyalakan untuk baris ini? (Misal: tarik objek 'Buble Name' milik NPC atau Player ke sini)")]
    public GameObject gelembungAktif;

    [Tooltip("Tempat Teks Dialog (TMP) di dalam gelembung tersebut")]
    public TextMeshProUGUI tempatTeksDialog;

    [Tooltip("Tempat Teks Nama (Opsional) di dalam gelembung tersebut")]
    public TextMeshProUGUI tempatTeksNama;

    [Tooltip("Nama yang akan ditampilkan")]
    public string namaKarakter;

    [TextArea(2, 4)]
    [Tooltip("Kalimat yang diucapkan")]
    public string kalimat;
}

/// <summary>
/// Script manajer untuk mengatur dialog yang saling berbalas (dua arah) antar beberapa karakter/gelembung.
/// Letakkan script ini pada objek kosong (misalnya "Manajer Dialog NPC 1").
/// Pastikan script KetikTeksDialog lama DIMATIKAN/DIHAPUS dari gelembung-gelembung yang terlibat agar tidak bentrok!
/// </summary>
public class DialogBergantian : MonoBehaviour
{
    [Header("Isi Percakapan Dua Arah")]
    [Tooltip("Urutkan percakapan dari atas ke bawah. Tentukan gelembung siapa yang bicara di tiap baris.")]
    public BarisDialogBergantian[] percakapan;

    [Header("Pengaturan Ketik")]
    public float kecepatanKetik = 0.04f;
    
    [Header("Event Selesai (Opsional)")]
    [Tooltip("Dijalankan setelah semua dialog selesai (misal: memberikan item, membuka pintu, dll)")]
    public UnityEvent saatSemuaSelesai;

    [Header("Pengaturan Input & Visual")]
    [Tooltip("Tombol untuk melanjutkan atau men-skip teks")]
    public KeyCode tombolLanjut = KeyCode.F;

    private int indeksKalimat = 0;
    private bool sedangNgetik = false;
    private bool sudahMulai = false;
    private bool sedangMenungguMenjauh = false;
    private float waktuBolehKlik = 0f;
    [Tooltip("Jika dicentang, dialog terakhir NPC akan tetap tampil dan baru tertutup saat pemain berjalan menjauh.")]
    public bool tutupSaatMenjauh = true;
    public float jarakMaksimal = 3f;
    [Tooltip("Kosongkan saja, akan dicari otomatis")]
    public Transform playerTransform;
    [Tooltip("Kosongkan saja, otomatis memakai posisi objek ini")]
    public Transform pusatInteraksi;

    private int indeksKalimat = 0;
    private bool sedangNgetik = false;
    private bool sudahMulai = false;
    private bool sedangMenungguMenjauh = false;

    void Awake()
    {
        // Cari pemain otomatis jika belum diisi
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }
    }

    void OnEnable()
    {
        // Mencegah input 'F' dari interaksi awal NPC terbaca sebagai perintah skip dialog di frame yang sama
        waktuBolehKlik = Time.time + 0.2f; 
        
        if (percakapan.Length > 0)
        {
            MulaiPercakapan(0);
        }
    }

    void SembunyikanSemuaGelembung()
    {
        // Matikan semua gelembung yang terdaftar agar tidak ada gelembung bocor/dobel
        foreach(var baris in percakapan)
        {
            if (baris.gelembungAktif != null && baris.gelembungAktif.activeSelf)
            {
                baris.gelembungAktif.SetActive(false);
            }
        }
    }

    public void MulaiPercakapan(int indeks)
    {
        indeksKalimat = indeks;
        sudahMulai = true;
        
        SembunyikanSemuaGelembung();
        StartCoroutine(KetikKalimat());
    }

    void Update()
    {
        // Deteksi jarak untuk menutup otomatis jika pemain menjauh
        if (tutupSaatMenjauh && (sudahMulai || sedangMenungguMenjauh))
        {
            Vector3 pusat = pusatInteraksi != null ? pusatInteraksi.position : transform.position;
            if (playerTransform != null && Vector2.Distance(pusat, playerTransform.position) > jarakMaksimal)
            {
                TutupPercakapan();
                return;
            }
        }

        if (!sudahMulai) return;
        
        // Mencegah klik ganda terlalu cepat atau bocor dari event sebelumnya
        if (Time.time < waktuBolehKlik) return;

        // Klik mouse kiri, Spasi, atau tombol lanjut (F) untuk lanjut/skip
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(tombolLanjut))
        {
            waktuBolehKlik = Time.time + 0.1f; // Beri sedikit jeda agar klik tidak dobel
            
            if (sedangNgetik)
            {
                // Skip ngetik, langsung munculkan semua tulisan
                StopAllCoroutines();
                var data = percakapan[indeksKalimat];
                if (data.tempatTeksDialog != null) data.tempatTeksDialog.text = data.kalimat;
                sedangNgetik = false;
            }
            else
            {
                // Lanjut ke kalimat berikutnya
                Lanjut();
            }
        }
    }

    void Lanjut()
    {
        if (indeksKalimat < percakapan.Length - 1)
        {
            indeksKalimat++;
            SembunyikanSemuaGelembung();
            StartCoroutine(KetikKalimat());
        }
        else
        {
            // Percakapan habis
            if (tutupSaatMenjauh)
            {
                // Biarkan dialog terakhir tetap muncul.
                // Matikan input klik, dan tunggu sampai pemain berjalan menjauh.
                sudahMulai = false;
                sedangMenungguMenjauh = true;
                
                // Memicu event selesai (misal: buka pintu) lebih awal
                saatSemuaSelesai?.Invoke();
            }
            else
            {
                TutupPercakapan();
            }
        }
    }

    void TutupPercakapan()
    {
        SembunyikanSemuaGelembung();
        sudahMulai = false;
        sedangMenungguMenjauh = false;
        
        // Panggil event selesai jika belum dipanggil
        if (!tutupSaatMenjauh) saatSemuaSelesai?.Invoke();
        
        // Matikan dirinya sendiri agar siap jika dipanggil lagi
        gameObject.SetActive(false);
    }

    IEnumerator KetikKalimat()
    {
        sedangNgetik = true;
        var data = percakapan[indeksKalimat];

        // Nyalakan gelembung target
        if (data.gelembungAktif != null) 
        {
            data.gelembungAktif.SetActive(true);
            
            // JAGA-JAGA: Jika script KetikTeksDialog yang lama meninggalkan CanvasGroup dengan alpha = 0, paksakan jadi 1
            CanvasGroup cg = data.gelembungAktif.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1f;
            
            CanvasGroup cgParent = data.gelembungAktif.GetComponentInParent<CanvasGroup>();
            if (cgParent != null) cgParent.alpha = 1f;
        }
        
        // Atur teks
        if (data.tempatTeksNama != null) data.tempatTeksNama.text = data.namaKarakter;
        if (data.tempatTeksDialog != null) data.tempatTeksDialog.text = "";

        // Animasi ketik huruf demi huruf
        foreach (char huruf in data.kalimat.ToCharArray())
        {
            if (data.tempatTeksDialog != null) data.tempatTeksDialog.text += huruf;
            yield return new WaitForSeconds(kecepatanKetik);
        }
        
        sedangNgetik = false;
    }
}
