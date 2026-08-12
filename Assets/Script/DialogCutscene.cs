using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class BarisDialogCutscene
{
    [TextArea(3, 5)]
    public string kalimat;
    
    [Tooltip("Kecepatan ketik khusus untuk baris ini. Biarkan 0 jika ingin menggunakan kecepatan bawaan.")]
    public float kecepatanKetikKhusus = 0f;
    
    [Space(5)]
    [Tooltip("Aksi atau event yang dipicu berbarengan saat teks ini mulai diketik. (Misal: Menyalakan panel awan)")]
    public UnityEvent aksiSaatKalimatMulai;

    [Tooltip("Centang ini jika Anda ingin layar tertutup efek transisi pixel sejenak SEBELUM memicu aksi di atas.")]
    public bool gunakanTransisiPixelSebelumAksi = false;
}

public class DialogCutscene : MonoBehaviour
{
    [Header("Referensi UI")]
    public GameObject panelUtamaDialog;
    public TextMeshProUGUI teksNamaKarakter;
    public TextMeshProUGUI teksIsiDialog;
    
    [Header("Pengaturan Dialog")]
    public string namaKarakter = "Nathan";
    public float kecepatanKetikBawaan = 0.03f;
    [Tooltip("Waktu tunggu sebelum dialog pertama dimulai")]
    public float jedaAwal = 1f;

    [Header("Data Percakapan")]
    public BarisDialogCutscene[] percakapan;

    [Header("Transisi Selesai (Opsional)")]
    [Tooltip("Panel hitam untuk efek fade out di akhir dialog")]
    public GameObject panelLayarHitam;
    public float durasiFadeOut = 1.5f;

    [Header("Teks Penutup (Di Layar Hitam)")]
    [Tooltip("Teks yang muncul di tengah layar hitam setelah dialog buble selesai (Opsional)")]
    [TextArea(2, 4)]
    public string teksPenutupLayarHitam;
    [Tooltip("Komponen TextMeshProUGUI untuk menampilkan teks penutup (tarik dari dalam objek layar hitam Anda)")]
    public TextMeshProUGUI UIteksPenutup;
    [Tooltip("Lama layar dibiarkan gelap sepenuhnya SEBELUM teks penutup mulai mengetik")]
    public float jedaSebelumTeksPenutup = 2f;
    [Tooltip("Lama teks penutup diam di layar sebelum menghilang")]
    public float durasiTampilTeksPenutup = 2f;
    [Tooltip("Durasi efek fade out (menghilang) khusus untuk teks penutup")]
    public float durasiFadeOutTeksPenutup = 1.5f;

    [Header("Event Akhir")]
    [Tooltip("Dijalankan setelah seluruh dialog selesai (dan setelah layar & teks memudar jika ada)")]
    public UnityEvent saatSemuaSelesai;

    private int indeks = 0;
    private bool sedangNgetik = false;
    private bool aktif = false;
    private bool kunciInput = false;
    private float antiSpamTimer = 0f;
    private Coroutine prosesKetik;
    private Material blinkMat;

    void Start()
    {
        // Matikan panel dialog di awal
        if (panelUtamaDialog != null) panelUtamaDialog.SetActive(false);
        if (teksNamaKarakter != null) teksNamaKarakter.text = "";
        if (teksIsiDialog != null) teksIsiDialog.text = "";

        // Matikan teks penutup di awal
        if (UIteksPenutup != null)
        {
            UIteksPenutup.text = "";
            UIteksPenutup.gameObject.SetActive(false);
        }

        // Siapkan layar hitam (transparan di awal)
        if (panelLayarHitam != null)
        {
            Image bgImage = panelLayarHitam.GetComponent<Image>();
            if (bgImage != null)
            {
                Color c = bgImage.color;
                c.a = 0f; // Transparan di awal
                bgImage.color = c;
            }
            panelLayarHitam.SetActive(false);
        }

        // Mulai otomatis
        StartCoroutine(MulaiDialogBerjeda());
    }

    IEnumerator MulaiDialogBerjeda()
    {
        kunciInput = true;
        yield return new WaitForSeconds(jedaAwal);
        
        if (panelUtamaDialog != null) panelUtamaDialog.SetActive(true);
        if (teksNamaKarakter != null) teksNamaKarakter.text = namaKarakter;
        
        aktif = true;
        kunciInput = false;
        MulaiKetik();
    }

    void Update()
    {
        if (!aktif || kunciInput) return;

        if (antiSpamTimer > 0f)
        {
            antiSpamTimer -= Time.deltaTime;
            return; // Tunggu cooldown selesai
        }

        // Klik kiri atau spasi untuk lanjut
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            antiSpamTimer = 0.2f; // Anti-spam 0.2 detik setiap kali nge-klik

            if (sedangNgetik)
            {
                // Jika sedang mengetik, paksa langsung selesai
                if (prosesKetik != null) StopCoroutine(prosesKetik);
                teksIsiDialog.text = percakapan[indeks].kalimat;
                sedangNgetik = false;
            }
            else
            {
                // Jika sudah selesai mengetik, lanjut ke kalimat berikutnya
                LanjutDialog();
            }
        }
    }

    void MulaiKetik()
    {
        if (indeks >= percakapan.Length)
        {
            AkhiriDialog();
            return;
        }

        if (percakapan[indeks].gunakanTransisiPixelSebelumAksi && TransisiRuangan.Instance != null)
        {
            // Kunci input agar tidak bisa diklik saat layar sedang transisi
            kunciInput = true;
            
            // Gunakan efek Pixel Transisi yang sudah Anda buat
            TransisiRuangan.Instance.Jalankan(percakapan[indeks].aksiSaatKalimatMulai);
            
            // Hitung total waktu transisi
            float totalJeda = (TransisiRuangan.Instance.durasiTransisi * 2) + TransisiRuangan.Instance.jedaDiTengah;
            
            // Tunda pengetikan teks sampai transisi selesai membuka
            StartCoroutine(KetikTeksDenganJeda(percakapan[indeks], totalJeda));
        }
        else
        {
            // Panggil event khusus secara langsung (tanpa transisi)
            percakapan[indeks].aksiSaatKalimatMulai?.Invoke();
            prosesKetik = StartCoroutine(KetikTeks(percakapan[indeks]));
        }
    }

    IEnumerator KetikTeksDenganJeda(BarisDialogCutscene baris, float jeda)
    {
        teksIsiDialog.text = "";
        
        // Sembunyikan panel dialog sebentar saat layar transisi
        if (panelUtamaDialog != null) panelUtamaDialog.SetActive(false);
        
        yield return new WaitForSeconds(jeda);
        
        // Munculkan lagi panel dialog setelah transisi layar kebuka
        if (panelUtamaDialog != null) panelUtamaDialog.SetActive(true);
        
        // Buka kunci input
        kunciInput = false;
        
        // Mulai ngetik
        prosesKetik = StartCoroutine(KetikTeks(baris));
    }

    IEnumerator KetikTeks(BarisDialogCutscene baris)
    {
        sedangNgetik = true;
        teksIsiDialog.text = "";
        
        float kecepatan = baris.kecepatanKetikKhusus > 0f ? baris.kecepatanKetikKhusus : kecepatanKetikBawaan;

        foreach (char huruf in baris.kalimat.ToCharArray())
        {
            teksIsiDialog.text += huruf;
            yield return new WaitForSeconds(kecepatan);
        }

        sedangNgetik = false;
    }

    void LanjutDialog()
    {
        indeks++;
        if (indeks < percakapan.Length)
        {
            MulaiKetik();
        }
        else
        {
            AkhiriDialog();
        }
    }

    void AkhiriDialog()
    {
        aktif = false;
        if (panelUtamaDialog != null) panelUtamaDialog.SetActive(false);

        if (panelLayarHitam != null)
        {
            StartCoroutine(ProsesFadeOutLaluSelesai());
        }
        else
        {
            saatSemuaSelesai?.Invoke();
        }
    }

    IEnumerator ProsesFadeOutLaluSelesai()
    {
        panelLayarHitam.SetActive(true);
        
        // 1. Fase Fade Out Layar
        Image bgImage = panelLayarHitam.GetComponent<Image>();
        if (bgImage != null)
        {
            float waktu = 0f;
            Color warnaAsli = bgImage.color;
            warnaAsli.a = 0f;
            
            while (waktu < durasiFadeOut)
            {
                waktu += Time.deltaTime;
                warnaAsli.a = Mathf.Clamp01(waktu / durasiFadeOut);
                bgImage.color = warnaAsli;
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(durasiFadeOut);
        }

        // 2. Fase Epilog (Teks Penutup)
        if (!string.IsNullOrEmpty(teksPenutupLayarHitam) && UIteksPenutup != null)
        {
            // Tambahan: Jeda tunggu layar gelap total sebelum ngetik teks!
            yield return new WaitForSeconds(jedaSebelumTeksPenutup);

            UIteksPenutup.gameObject.SetActive(true);
            UIteksPenutup.text = "";
            
            // Mengembalikan warna teks menjadi solid (karena mungkin pudar dari main sebelumnya)
            Color warnaTeksAwal = UIteksPenutup.color;
            warnaTeksAwal.a = 1f;
            UIteksPenutup.color = warnaTeksAwal;

            // Efek mengetik teks penutup
            foreach (char huruf in teksPenutupLayarHitam.ToCharArray())
            {
                UIteksPenutup.text += huruf;
                yield return new WaitForSeconds(kecepatanKetikBawaan);
            }

            // Tahan di layar
            yield return new WaitForSeconds(durasiTampilTeksPenutup);

            // Efek Fade Out Teks
            float waktuTeks = 0f;
            Color warnaTeksMemudar = UIteksPenutup.color;
            while(waktuTeks < durasiFadeOutTeksPenutup)
            {
                waktuTeks += Time.deltaTime;
                warnaTeksMemudar.a = Mathf.Lerp(1f, 0f, waktuTeks / durasiFadeOutTeksPenutup);
                UIteksPenutup.color = warnaTeksMemudar;
                yield return null;
            }
            
            UIteksPenutup.gameObject.SetActive(false);
        }

        // 3. Eksekusi Aksi Terakhir (seperti pindah scene)
        saatSemuaSelesai?.Invoke();
    }
}
