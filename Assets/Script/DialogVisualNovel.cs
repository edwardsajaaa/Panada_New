using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Events;

[System.Serializable]
public class BarisDialogVN
{
    [Tooltip("Masukkan objek Buble Name (kiri atau kanan) yang mau dinyalakan")]
    public GameObject bubleNamaAktif;
    
    [Tooltip("Nama karakter yang muncul di buble tersebut (misal: Nathan)")]
    public string namaKarakter;

    [Tooltip("Isi percakapannya")]
    [TextArea(2, 4)]
    public string teksDialog;
}

public class DialogVisualNovel : MonoBehaviour
{
    [Header("Referensi UI Utama")]
    [Tooltip("Objek Buble Dialog Utama (kotak besar di bawah)")]
    public GameObject bubleDialogUtama;
    
    [Tooltip("Opsional: Masukkan Canvas Induk (Buble Dialog Canvas) agar otomatis nyala/mati")]
    public GameObject canvasDialogUtama;
    
    [Tooltip("Teks (TextMeshPro) di dalam Buble Dialog Utama untuk menampilkan isi percakapan")]
    public TextMeshProUGUI tempatTeksDialog;
    
    [Tooltip("Masukkan semua objek Buble Name yang ada (Kiri & Kanan), agar script bisa otomatis menyembunyikan yang tidak dipakai")]
    public GameObject[] semuaBubleNama;

    [Header("NPC (Opsional)")]
    [Tooltip("Masukkan script PopupInteraksi milik NPC agar balon '?' disembunyikan otomatis saat ngobrol")]
    public PopupInteraksi popupInteraksiNPC;

    [Header("Isi Percakapan")]
    public BarisDialogVN[] percakapan;
    
    [Header("Pengaturan")]
    public float kecepatanKetik = 0.04f;
    [Tooltip("Tombol untuk lanjut ke dialog berikutnya")]
    public KeyCode tombolLanjut = KeyCode.F;
    
    [Header("Pengaturan Pengulangan")]
    [Tooltip("Centang jika dialog ingin melompat ke baris tertentu saat pemain mengajak ngobrol untuk kedua kalinya.")]
    public bool lompatKeDialogSpesifikJikaDiulang = true;
    [Tooltip("Nomor Element (Indeks) yang mau ditampilkan saat ngobrol kedua kali (misal: isi 14 untuk Element 14)")]
    public int indeksLompatan = 14;

    [Header("Event Selesai")]
    public UnityEvent saatDialogSelesai;

    [Header("Jarak (Opsional)")]
    [Tooltip("Centang jika dialog ingin otomatis tertutup saat player menjauh")]
    public bool tutupSaatMenjauh = true;
    [Tooltip("Jika NPC diisi, jarak akan otomatis mengikuti NPC. Jika kosong, angka ini yang dipakai.")]
    public float jarakMaksimal = 150f;
    [Tooltip("Transform Player (misal Nathan Outdoor)")]
    public Transform playerTransform;
    [Tooltip("Pusat interaksi NPC (biarkan kosong jika jarak dihitung dari NPC langsung)")]
    public Transform pusatInteraksi;

    private int indeks = 0;
    private bool sedangNgetik = false;
    private Coroutine proses;
    private bool aktif = false;
    private bool sudahPernahSelesai = false;

    void Awake()
    {
        if (playerTransform == null)
        {
            PlayerMovementUI playerUI = FindAnyObjectByType<PlayerMovementUI>();
            if (playerUI != null)
            {
                playerTransform = playerUI.transform;
            }
            else
            {
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) playerTransform = p.transform;
            }
        }
    }

    void OnEnable()
    {
        aktif = true;
        sedangNgetik = false;
        
        if (lompatKeDialogSpesifikJikaDiulang && sudahPernahSelesai && percakapan.Length > 0)
        {
            // Pastikan indeks lompatan tidak error / melebihi jumlah percakapan
            indeks = Mathf.Clamp(indeksLompatan, 0, percakapan.Length - 1);
        }
        else
        {
            indeks = 0;
        }
        
        if (popupInteraksiNPC != null) popupInteraksiNPC.sembunyikanSementara = true;
        
        if (canvasDialogUtama != null) canvasDialogUtama.SetActive(true);
        if (bubleDialogUtama != null) bubleDialogUtama.SetActive(true);
        
        Tampilkan(indeks);
    }

    void Update()
    {
        if (!aktif) return;

        // Cek jarak menjauh
        if (tutupSaatMenjauh && playerTransform != null)
        {
            Transform pusat = pusatInteraksi != null ? pusatInteraksi : (popupInteraksiNPC != null ? popupInteraksiNPC.transform : transform);
            
            // Otomatis pakai jarak interaksi NPC (+ buffer kecil), kalau NPC kosong baru pakai jarakMaksimal
            float batasJarak = popupInteraksiNPC != null ? popupInteraksiNPC.jarakInteraksi + 0.1f : jarakMaksimal;
            
            if (Vector2.Distance(pusat.position, playerTransform.position) > batasJarak)
            {
                Tutup(false); // Batal di tengah jalan, jangan ditandai selesai
                return;
            }
        }

        // Lanjut dialog bisa pakai F, Spasi, atau Klik Kiri
        if (Input.GetKeyDown(tombolLanjut) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (sedangNgetik)
            {
                // Kalau teks masih jalan, langsung munculin semua teksnya (skip ngetik)
                if (proses != null) StopCoroutine(proses);
                tempatTeksDialog.text = percakapan[indeks].teksDialog;
                sedangNgetik = false;
            }
            else
            {
                // Lanjut ke baris berikutnya
                if (indeks < percakapan.Length - 1)
                {
                    indeks++;
                    Tampilkan(indeks);
                }
                else
                {
                    Tutup(true); // Selesai secara normal sampai akhir
                }
            }
        }
    }

    void Tampilkan(int i)
    {
        var data = percakapan[i];

        // 1. Sembunyikan semua buble nama dulu
        foreach (var buble in semuaBubleNama)
        {
            if (buble != null) buble.SetActive(false);
        }

        // 2. Nyalakan buble nama yang sesuai giliran
        if (data.bubleNamaAktif != null)
        {
            data.bubleNamaAktif.SetActive(true);
            
            // Otomatis cari komponen teks di dalam buble nama tersebut dan ubah namanya
            TextMeshProUGUI teksNama = data.bubleNamaAktif.GetComponentInChildren<TextMeshProUGUI>();
            if (teksNama != null)
            {
                teksNama.text = data.namaKarakter;
            }
        }

        // 3. Mulai ngetik isi dialog
        if (tempatTeksDialog != null) tempatTeksDialog.text = "";
        if (proses != null) StopCoroutine(proses);
        proses = StartCoroutine(Ketik(data));
    }

    IEnumerator Ketik(BarisDialogVN data)
    {
        sedangNgetik = true;
        foreach (char c in data.teksDialog.ToCharArray())
        {
            if (tempatTeksDialog != null) tempatTeksDialog.text += c;
            yield return new WaitForSeconds(kecepatanKetik);
        }
        sedangNgetik = false;
    }

    void Tutup(bool selesaiNormal = false)
    {
        aktif = false;
        
        if (selesaiNormal)
        {
            sudahPernahSelesai = true; // Tandai bahwa dialog ini sudah pernah selesai dibaca
        }
        
        if (proses != null) StopCoroutine(proses);
        
        // Sembunyikan semua UI percakapan setelah selesai
        foreach (var buble in semuaBubleNama)
        {
            if (buble != null) buble.SetActive(false);
        }
        if (bubleDialogUtama != null) bubleDialogUtama.SetActive(false);
        if (canvasDialogUtama != null) canvasDialogUtama.SetActive(false);
        
        if (popupInteraksiNPC != null) popupInteraksiNPC.sembunyikanSementara = false;
        
        saatDialogSelesai?.Invoke();
        gameObject.SetActive(false); // Matikan script ini sendiri
    }
}
