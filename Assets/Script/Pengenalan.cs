using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Ditambahkan untuk mendukung TextMeshPro
using UnityEngine.UI; // Ditambahkan untuk mendukung komponen Slider

[System.Serializable]
public struct DataDialog
{
    public string namaKarakter;
    [TextArea(3, 5)]
    public string teksDialog;
}

public class Pengenalan : MonoBehaviour
{
    [Header("Referensi Layar Hitam (Fade)")]
    [Tooltip("Buat Panel/Image hitam penuh di Canvas dan masukkan ke sini")]
    public GameObject blackScreenPanel;

    [Header("Referensi Panel/Objek")]
    public GameObject blankMonitor;
    public GameObject bootingTV;
    public GameObject beritaKonteks;
    public GameObject beritaKonteksFullscreen; // Panel berita versi fullscreen
    public GameObject bubbleNamePanel;
    public Slider loadingSlider;

    [Header("Referensi Teks UI")]
    public TMP_Text textNamaKarakter; 
    public TMP_Text textIsiDialog;    

    [Header("Pengaturan Dialog (Bisa diisi banyak orang)")]
    public DataDialog[] dialogAwal;
    public DataDialog[] dialogBerita;

    [Header("Pengaturan Waktu")]
    public float durasiFadeBlack = 1.5f;
    public float jedaSebelumDialog = 3f;
    public float jedaSetelahDialogAwal = 2f;
    public float durasiBootingTV = 5f;
    public float durasiFadeOut = 1f;
    public float jedaBacaBerita = 3f; // Jeda sebelum dialog berita muncul
    public float durasiTransisiTeks = 0.3f; // Waktu fade in/out teks

    private int indeksDialogSaatIni = 0;
    private DataDialog[] arrayDialogAktif;
    private bool sedangDialog = false;
    private bool sedangTransisiTeks = false;
    private Material blinkMaterial; // Material shader untuk efek Eye Blink

    void Start()
    {
        // 1. Kondisi awal: Semua panel di-reset, yang muncul pertama adalah blank monitor dan layar hitam
        if (blackScreenPanel != null) 
        {
            blackScreenPanel.SetActive(true);
            blackScreenPanel.transform.SetAsLastSibling(); // Pastikan layar hitam berada paling depan
            
            // Coba ambil material shader Eye Blink
            Image bgImage = blackScreenPanel.GetComponent<Image>();
            if (bgImage != null && bgImage.material != null && bgImage.material.HasProperty("_Blink"))
            {
                // Buat instance agar material asli di project tidak tertimpa/berubah secara permanen
                blinkMaterial = new Material(bgImage.material);
                bgImage.material = blinkMaterial;
                blinkMaterial.SetFloat("_Blink", 1f); // Set mata tertutup penuh di awal (layar hitam)
            }
            else
            {
                // Fallback jika tidak pakai shader: gunakan CanvasGroup biasa
                CanvasGroup cg = blackScreenPanel.GetComponent<CanvasGroup>();
                if (cg == null) cg = blackScreenPanel.AddComponent<CanvasGroup>();
                cg.alpha = 1f; 
            }
        }

        if (blankMonitor != null) blankMonitor.SetActive(true);
        if (bootingTV != null) bootingTV.SetActive(false);
        if (beritaKonteks != null) beritaKonteks.SetActive(false);
        if (beritaKonteksFullscreen != null) beritaKonteksFullscreen.SetActive(false);
        if (bubbleNamePanel != null) bubbleNamePanel.SetActive(false);
        
        if (loadingSlider != null) loadingSlider.gameObject.SetActive(false); // Sembunyikan slider di awal

        // Memulai coroutine urutan alur
        StartCoroutine(UrutanPengenalan());
    }

    void Update()
    {
        // Lanjut ke dialog berikutnya saat pemain klik kiri mouse atau tekan Spasi
        // Dicegah jika sedang transisi teks agar teks tidak bertumpuk/error
        if (sedangDialog && !sedangTransisiTeks && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
        {
            LanjutDialog();
        }
    }

    IEnumerator UrutanPengenalan()
    {
        // 1.5. Transisi Fade dari Hitam ke Transparan (membuka layar atau membuka mata)
        if (blackScreenPanel != null)
        {
            yield return StartCoroutine(FadeBlackScreen(1f, 0f, durasiFadeBlack));
            blackScreenPanel.SetActive(false); // Nonaktifkan panel hitam setelah pudar
        }

        // 2. Tunggu beberapa detik (default 3 detik) sebelum memunculkan dialog
        yield return new WaitForSeconds(jedaSebelumDialog);

        // 3. Munculkan dialog awal (Anda bisa isi sendiri nama dan dialognya di Inspector)
        yield return StartCoroutine(JalankanDialog(dialogAwal));

        // 3.5 Jeda setelah dialog awal selesai
        yield return new WaitForSeconds(jedaSetelahDialogAwal);

        // 4. Munculkan panel Booting TV
        if (bootingTV != null) bootingTV.SetActive(true);
        
        // 5. Jalankan animasi loading slider (muncul telat 1 detik, lalu ngisi tersendat)
        if (loadingSlider != null)
        {
            loadingSlider.interactable = false; // Matikan interaksi klik oleh player
            loadingSlider.value = 0f;
            loadingSlider.gameObject.SetActive(false); // Pastikan tersembunyi
            
            // Jeda 1 detik sebelum slider muncul di layar
            yield return new WaitForSeconds(1f);
            loadingSlider.gameObject.SetActive(true);

            float sisaWaktuLoading = durasiBootingTV - 1f;
            if (sisaWaktuLoading <= 0) sisaWaktuLoading = 1f; // Failsafe
            
            float waktuTersita = 0f;

            // Loop untuk mengisi slider tersendat-sendat secara acak
            while (waktuTersita < sisaWaktuLoading && loadingSlider.value < 1f)
            {
                // Jeda berhenti (tersendat)
                float jedaAcak = Random.Range(0.1f, 0.4f);
                
                // Pastikan waktu tidak bablas dari total durasi
                if (waktuTersita + jedaAcak > sisaWaktuLoading)
                {
                    jedaAcak = sisaWaktuLoading - waktuTersita;
                }
                
                yield return new WaitForSeconds(jedaAcak);
                waktuTersita += jedaAcak;

                // Lompatan isi loading bar secara acak
                float nilaiAcak = Random.Range(0.05f, 0.35f);
                loadingSlider.value += nilaiAcak;
            }

            loadingSlider.value = 1f; // Pastikan penuh 100% di akhir

            // Jika slider penuh lebih cepat dari sisa waktu, diam sejenak sampai waktunya pas
            if (waktuTersita < sisaWaktuLoading)
            {
                yield return new WaitForSeconds(sisaWaktuLoading - waktuTersita);
            }
        }
        else
        {
            // Jika slider tidak dipasang di Inspector, tetap tunggu secara normal
            yield return new WaitForSeconds(durasiBootingTV);
        }

        // 6. Munculkan gambar berita konteks sebelum Booting TV fade out 
        if (beritaKonteks != null) beritaKonteks.SetActive(true);

        // Memastikan Booting TV berada di atas Berita Konteks secara hierarki supaya efek fade terlihat
        if (bootingTV != null) bootingTV.transform.SetAsLastSibling();

        // 7. Fade out panel Booting TV sehingga menampilkan Berita Konteks di baliknya
        if (bootingTV != null)
        {
            yield return StartCoroutine(FadeOutObjek(bootingTV, durasiFadeOut));
            bootingTV.SetActive(false); // Matikan objek setelah selesai fade out
        }

        // 7.5 Tunggu 3 detik agar pemain bisa mengamati isi gambar Berita Konteks
        yield return new WaitForSeconds(jedaBacaBerita);

        // 8. Muncul kembali dialog dengan pembawa acara (Blink mata akan terjadi di tengah dialog ini)
        yield return StartCoroutine(JalankanDialog(dialogBerita));

        // Selesai pengenalan, di sini Anda bisa menambahkan script untuk lanjut ke scene berikutnya
        Debug.Log("Alur Pengenalan Selesai! Lanjut ke main menu/gameplay.");
    }

    IEnumerator TransisiBlinkDiTengahDialog()
    {
        sedangTransisiTeks = true;
        
        // Fade out teks sebelumnya
        yield return StartCoroutine(FadeTeks(1f, 0f, durasiTransisiTeks));

        if (blackScreenPanel != null)
        {
            blackScreenPanel.SetActive(true);
            blackScreenPanel.transform.SetAsLastSibling();
            
            // Tutup mata (fade ke hitam penuh)
            yield return StartCoroutine(FadeBlackScreen(0f, 1f, 0.25f));
            
            // Gelap total selama 0.5 detik
            yield return new WaitForSeconds(0.5f);
            
            // Ganti latar menjadi berita fullscreen
            if (beritaKonteksFullscreen != null) 
            {
                beritaKonteksFullscreen.SetActive(true);
                // Pastikan dialog ada di depan panel berita
                if (bubbleNamePanel != null) bubbleNamePanel.transform.SetAsLastSibling();
                // Dan blackscreen (kelopak mata) tetap paling depan saat mau dibuka
                blackScreenPanel.transform.SetAsLastSibling();
            }
            
            // Siapkan teks dialog ke-3 di belakang layar
            UpdateTeksUI();
            
            // Buka mata perlahan menampilkan scene baru
            yield return StartCoroutine(FadeBlackScreen(1f, 0f, 0.4f));
            blackScreenPanel.SetActive(false);
        }
        else
        {
            // Fallback jika tidak ada black screen
            if (beritaKonteksFullscreen != null) beritaKonteksFullscreen.SetActive(true);
            UpdateTeksUI();
        }

        // Fade in teks dialog baru
        yield return StartCoroutine(FadeTeks(0f, 1f, durasiTransisiTeks));

        sedangTransisiTeks = false;
    }

    IEnumerator FadeBlackScreen(float nilaiAwal, float nilaiAkhir, float durasi)
    {
        float waktuMulai = Time.time;
        CanvasGroup cg = null;
        
        if (blinkMaterial == null && blackScreenPanel != null)
        {
            cg = blackScreenPanel.GetComponent<CanvasGroup>();
        }

        while (Time.time < waktuMulai + durasi)
        {
            float progress = (Time.time - waktuMulai) / durasi;
            float nilaiSaatIni = Mathf.Lerp(nilaiAwal, nilaiAkhir, progress);
            
            if (blinkMaterial != null)
            {
                blinkMaterial.SetFloat("_Blink", nilaiSaatIni);
            }
            else if (cg != null)
            {
                cg.alpha = nilaiSaatIni;
            }
            yield return null;
        }

        // Pastikan nilai mencapai akhir secara akurat
        if (blinkMaterial != null)
        {
            blinkMaterial.SetFloat("_Blink", nilaiAkhir);
        }
        else if (cg != null)
        {
            cg.alpha = nilaiAkhir;
        }
    }

    IEnumerator JalankanDialog(DataDialog[] arrayDialog)
    {
        arrayDialogAktif = arrayDialog;
        indeksDialogSaatIni = 0;
        
        if (arrayDialogAktif != null && arrayDialogAktif.Length > 0)
        {
            sedangDialog = true;
            if (bubbleNamePanel != null) 
            {
                bubbleNamePanel.SetActive(true);
                bubbleNamePanel.transform.SetAsLastSibling(); // Pastikan UI Dialog selalu paling depan
            }
            
            // Set transisi teks
            sedangTransisiTeks = true;
            
            // PENTING: Update teks dulu dengan data index 0 sebelum di-fade in
            // Supaya saat muncul otomatis, teks pertama sudah ada
            UpdateTeksUI();
            SetTeksAlpha(0f);
            
            // Fade in teks pertama
            yield return StartCoroutine(FadeTeks(0f, 1f, durasiTransisiTeks));
            
            sedangTransisiTeks = false;

            // Coroutine akan menunggu (pause) di sini selama sedangDialog bernilai true
            while (sedangDialog)
            {
                yield return null;
            }
        }
    }

    void LanjutDialog()
    {
        indeksDialogSaatIni++;
        
        // Jika masih ada dialog selanjutnya
        if (indeksDialogSaatIni < arrayDialogAktif.Length)
        {
            // Cek jika ini adalah dialog berita dan masuk ke baris ke-3 (indeks 2)
            if (arrayDialogAktif == dialogBerita && indeksDialogSaatIni == 2)
            {
                StartCoroutine(TransisiBlinkDiTengahDialog());
            }
            else
            {
                StartCoroutine(TransisiTeksBerikutnya());
            }
        }
        else // Jika dialog sudah habis
        {
            StartCoroutine(SelesaikanDialog());
        }
    }

    IEnumerator TransisiTeksBerikutnya()
    {
        sedangTransisiTeks = true;

        // Fade out teks sebelumnya
        yield return StartCoroutine(FadeTeks(1f, 0f, durasiTransisiTeks));

        // Ubah teks
        UpdateTeksUI();

        // Fade in teks baru
        yield return StartCoroutine(FadeTeks(0f, 1f, durasiTransisiTeks));

        sedangTransisiTeks = false;
    }

    IEnumerator SelesaikanDialog()
    {
        sedangTransisiTeks = true;
        
        // Fade out teks terakhir sebelum panel menghilang
        yield return StartCoroutine(FadeTeks(1f, 0f, durasiTransisiTeks));
        
        sedangDialog = false;
        sedangTransisiTeks = false;
        if (bubbleNamePanel != null) bubbleNamePanel.SetActive(false); // Sembunyikan panel dialog
    }

    void UpdateTeksUI()
    {
        // Update teks UI dengan data dari inspector
        if (textNamaKarakter != null) textNamaKarakter.text = arrayDialogAktif[indeksDialogSaatIni].namaKarakter;
        if (textIsiDialog != null) textIsiDialog.text = arrayDialogAktif[indeksDialogSaatIni].teksDialog;
    }

    void SetTeksAlpha(float alphaAkhir)
    {
        // Hanya isi dialog yang ikut transisi, nama karakter tetap utuh (tidak fade)
        if (textIsiDialog != null) 
        {
            Color c = textIsiDialog.color;
            c.a = alphaAkhir;
            textIsiDialog.color = c;
        }
    }

    IEnumerator FadeTeks(float alphaAwal, float alphaAkhir, float durasi)
    {
        float waktuMulai = Time.time;

        while (Time.time < waktuMulai + durasi)
        {
            float progress = (Time.time - waktuMulai) / durasi;
            float currentAlpha = Mathf.Lerp(alphaAwal, alphaAkhir, progress);
            SetTeksAlpha(currentAlpha);
            yield return null;
        }

        SetTeksAlpha(alphaAkhir);
    }

    IEnumerator FadeOutObjek(GameObject objekToFade, float durasi)
    {
        // CanvasGroup digunakan untuk memanipulasi Alpha (transparansi) keseluruhan objek dan child-nya
        CanvasGroup canvasGroup = objekToFade.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = objekToFade.AddComponent<CanvasGroup>();
        }

        // Set alpha awal menjadi penuh (1)
        canvasGroup.alpha = 1f;

        float waktuMulai = Time.time;

        while (Time.time < waktuMulai + durasi)
        {
            float progress = (Time.time - waktuMulai) / durasi;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);
            yield return null;
        }

        // Pastikan alpha benar-benar 0 di akhir
        canvasGroup.alpha = 0f;
    }
}
