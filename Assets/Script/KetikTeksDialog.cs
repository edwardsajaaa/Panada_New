using UnityEngine;
using TMPro;
using System.Collections;

[System.Serializable]
public class BarisCeritaDialog
{
    [Tooltip("Nama karakter yang sedang berbicara")]
    public string namaKarakter = "Penyiar TV";
    
    [TextArea(2, 4)]
    [Tooltip("Kalimat yang diucapkan oleh karakter tersebut")]
    public string kalimat;
}

// Memastikan objek ini punya CanvasGroup agar bisa disembunyikan dan dimunculkan (Fade)
[RequireComponent(typeof(CanvasGroup))]
public class KetikTeksDialog : MonoBehaviour
{
    [Header("Referensi UI")]
    [Tooltip("Masukkan Text (TMP) yang akan dijadikan tempat teks dialog berjalan")]
    public TextMeshProUGUI teksDialog;
    
    [Tooltip("Masukkan Text (TMP) untuk nama karakter (Opsional)")]
    public TextMeshProUGUI teksNama;

    [Header("Pengaturan Gelembung & Waktu")]
    [Tooltip("Berapa detik gelembung ini harus menunggu sebelum muncul di layar?")]
    public float waktuTungguGelembung = 3f;
    
    [Tooltip("Durasi animasi memudar (fade-in) gelembungnya")]
    public float durasiFadeGelembung = 0.5f;

    [Tooltip("Berapa lama teks diam sebentar setelah gelembung muncul sebelum mulai mengetik?")]
    public float jedaSebelumMengetik = 0.3f;
    
    [Tooltip("Kecepatan mesin tik (semakin kecil semakin cepat)")]
    public float kecepatanKetik = 0.04f;

    [Header("Isi Cerita")]
    [Tooltip("Daftar percakapan Anda. Tekan tombol + untuk menambah dialog!")]
    public BarisCeritaDialog[] percakapan;

    private int indeksKalimat = 0;
    private bool sedangNgetik = false;
    private bool sudahMulai = false;
    private CanvasGroup grupGelembung;

    void Awake()
    {
        grupGelembung = GetComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        if (percakapan.Length > 0 && teksDialog != null)
        {
            // Kosongkan teks di awal
            teksDialog.text = ""; 
            if (teksNama != null) teksNama.text = ""; 
            
            sudahMulai = false;

            // Sembunyikan gelembung sepenuhnya di detik ke-0
            if (grupGelembung != null) 
            {
                grupGelembung.alpha = 0f;
                grupGelembung.interactable = true;
                grupGelembung.blocksRaycasts = true;
            }

            // Mulai proses alur animasi yang mulus
            StartCoroutine(AlurDialogMengalir());
        }
    }

    IEnumerator AlurDialogMengalir()
    {
        // 1. Tunggu 3 detik (waktu agar pemain fokus ke berita TV dulu)
        yield return new WaitForSeconds(waktuTungguGelembung);

        // 2. Memunculkan Gelembung perlahan (Fade In)
        if (grupGelembung != null && durasiFadeGelembung > 0)
        {
            float timer = 0;
            while (timer < durasiFadeGelembung)
            {
                timer += Time.deltaTime;
                grupGelembung.alpha = Mathf.Lerp(0f, 1f, timer / durasiFadeGelembung);
                yield return null;
            }
            grupGelembung.alpha = 1f; // Pastikan mentok 100%
        }

        // 3. Jeda sedikit agar tidak terkesan terburu-buru
        yield return new WaitForSeconds(jedaSebelumMengetik);

        // 4. Barulah mulai mengetik ceritanya
        sudahMulai = true;
        MulaiDialog(0);
    }

    void Update()
    {
        // Hanya bisa lanjut atau skip jika animasi gelembung sudah selesai dan dialog mulai
        if (!sudahMulai) return;

        // Jika pemain mengeklik kiri (mouse) atau menekan Spasi
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (sedangNgetik)
            {
                // Skip animasi ketik dan langsung munculkan semua teks
                StopAllCoroutines();
                teksDialog.text = percakapan[indeksKalimat].kalimat;
                sedangNgetik = false;
            }
            else
            {
                // Lanjut ke kalimat berikutnya
                LanjutDialog();
            }
        }
    }

    public void MulaiDialog(int index)
    {
        indeksKalimat = index;
        StartCoroutine(KetikKalimat());
    }

    void LanjutDialog()
    {
        if (indeksKalimat < percakapan.Length - 1)
        {
            indeksKalimat++;
            StartCoroutine(KetikKalimat());
        }
        else
        {
            // Dialog habis, saatnya menutup gelembung agar pemain bisa fokus ke TV
            StartCoroutine(TutupGelembung());
        }
    }

    IEnumerator TutupGelembung()
    {
        sudahMulai = false; // Mencegah pemain mengklik lagi

        // 1. Fade Out Gelembung
        if (grupGelembung != null && durasiFadeGelembung > 0)
        {
            float timer = 0;
            while (timer < durasiFadeGelembung)
            {
                timer += Time.deltaTime;
                grupGelembung.alpha = Mathf.Lerp(1f, 0f, timer / durasiFadeGelembung);
                yield return null;
            }
            grupGelembung.alpha = 0f;
            
            // Matikan raycast agar tidak menghalangi tombol 'Kembali' di belakangnya
            grupGelembung.interactable = false;
            grupGelembung.blocksRaycasts = false;
        }
    }

    IEnumerator KetikKalimat()
    {
        sedangNgetik = true;
        teksDialog.text = "";
        
        BarisCeritaDialog dataSaatIni = percakapan[indeksKalimat];
        
        // Perbarui nama karakter
        if (teksNama != null)
        {
            teksNama.text = dataSaatIni.namaKarakter;
        }
        
        // Memunculkan teks huruf demi huruf
        foreach (char huruf in dataSaatIni.kalimat.ToCharArray())
        {
            teksDialog.text += huruf;
            yield return new WaitForSeconds(kecepatanKetik);
        }
        
        sedangNgetik = false;
    }
}
