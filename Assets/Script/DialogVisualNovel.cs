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
    [Tooltip("Jika dicentang, saat dialog ini dipanggil lagi, ia hanya akan menampilkan baris terakhir saja.")]
    public bool tampilkanHanyaBarisTerakhirJikaDiulang = true;

    [Header("Event Selesai")]
    public UnityEvent saatDialogSelesai;

    [Header("Jarak (Opsional)")]
    [Tooltip("Centang jika dialog ingin otomatis tertutup saat player menjauh")]
    public bool tutupSaatMenjauh = true;
    [Tooltip("Jarak maksimal sebelum dialog tertutup otomatis")]
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

    void OnEnable()
    {
        aktif = true;
        sedangNgetik = false;
        
        if (tampilkanHanyaBarisTerakhirJikaDiulang && sudahPernahSelesai && percakapan.Length > 0)
        {
            indeks = percakapan.Length - 1;
        }
        else
        {
            indeks = 0;
        }
        
        if (popupInteraksiNPC != null) popupInteraksiNPC.sembunyikanSementara = true;
        
        if (canvasDialogUtama != null) canvasDialogUtama.SetActive(true);
        if (bubleDialogUtama != null) bubleDialogUtama.SetActive(true);
        
        Tampilkan(0);
    }

    void Update()
    {
        if (!aktif) return;

        // Cek jarak menjauh
        if (tutupSaatMenjauh && playerTransform != null && popupInteraksiNPC != null)
        {
            Transform pusat = pusatInteraksi != null ? pusatInteraksi : popupInteraksiNPC.transform;
            if (Vector2.Distance(pusat.position, playerTransform.position) > jarakMaksimal)
            {
                Tutup();
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
                    Tutup();
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

    void Tutup()
    {
        aktif = false;
        sudahPernahSelesai = true; // Tandai bahwa dialog ini sudah pernah selesai dibaca
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
