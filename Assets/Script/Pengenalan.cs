using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public struct DataDialog
{
    public string namaKarakter;
    [TextArea(3, 5)]
    public string teksDialog;
}

[System.Serializable]
public struct DataDetailKoran
{
    [Tooltip("Objek gambar koran di Koran Panel (meja) yang bisa diklik")]
    public GameObject koranDiMeja;
    [Tooltip("Objek penampung detail koran ini (misal: 'Image 1' di dalam Panel Detail Koran)")]
    public GameObject panelDetailObjek;
    [Tooltip("Objek gambar poster di dalam panel detail (yang akan bergerak dari tengah ke kiri)")]
    public RectTransform posterDetail;
    [Tooltip("Objek penampung teks berita di sebelah kanan (Scroll View / Content)")]
    public CanvasGroup grupTeksBerita;
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
    public Material tvEffectMaterial; // Material TVEffectMaterial pada FullScreenPassRendererFeature
    public GameObject bubbleNamePanel; // Panel penampung utama (Parent)
    public GameObject panelKoran; // Panel koran yang akan muncul terakhir
    public GameObject panelKasur; // Panel kasur yang muncul di tengah dialog awal
    public GameObject mainMenuPanel; // Panel MainMenu tujuan akhir
    public GameObject mainMenuScreenDialog; // Panel MainMenuScreenDialog di dalam layar TV
    public GameObject[] daftarGambarKoran; // Daftar gambar koran yang muncul 1/1
    
    [Header("Referensi Tombol Next Koran")]
    public GameObject tombolNext; // Objek 'Next' di bawah Koran Panel
    public Transform gambarPanahNext; // Child 'Image' (panah ke bawah) di bawah objek Next (opsional, otomatis dicari jika kosong)
    [Tooltip("Jarak ayunan naik turun panah dalam pixel. Geser slider ke kiri/kanan untuk mengatur jaraknya.")]
    [Range(0.5f, 15f)]
    public float jarakNaikTurunPanah = 3f; // Jarak naik turun animasi (diubah ke 3 agar tidak mentok border)
    [Tooltip("Kecepatan gerakan animasi naik turun.")]
    [Range(0.5f, 10f)]
    public float kecepatanNgambang = 3.5f;  // Kecepatan animasi mengambang
    
    [Header("Referensi Fitur Detail Koran Interaktif")]
    [Tooltip("Panel overlay latar gelap penampung seluruh detail koran (Panel Detail Koran)")]
    public GameObject panelDetailKoranLatar;
    [Tooltip("Daftar konfigurasi 5 koran beserta panel detail beritanya")]
    public DataDetailKoran[] daftarDetailKoran;
    
    public static GameObject instansiTombolNext;
    private bool sedangMelihatDetail = false;
    private Vector2[] posisiAsliKiriPoster;
    
    [Header("Referensi Popup Terpisah (Opsional)")]
    public GameObject panelBackgroundDialog; // Background dialog yang akan ikut popup tiap ganti teks
    public GameObject panelNamaKarakter;     // Panel nama yang hanya popup di awal
    
    public Slider loadingSlider;

    [Header("Referensi Teks UI")]
    public TMP_Text textNamaKarakter; 
    public TMP_Text textIsiDialog;    

    [Header("Pengaturan Dialog (Bisa diisi banyak orang)")]
    public DataDialog[] dialogAwal;
    public DataDialog[] dialogBerita;
    public DataDialog[] dialogKedua;

    [Header("Pengaturan Waktu")]
    public float durasiFadeBlack = 1.5f;
    public float jedaSebelumDialog = 3f;
    public float jedaSetelahDialogAwal = 2f;
    public float durasiBootingTV = 5f;
    public float durasiFadeOut = 1f;
    public float jedaBacaBerita = 3f; // Jeda sebelum dialog berita muncul
    public float durasiTransisiTeks = 0.3f; // Waktu fade in/out teks

    private int indeksDialogSaatIni = 0;
    private int batasAkhirDialogSaatIni = 0;
    private DataDialog[] arrayDialogAktif;
    public static bool sedangDialogAktif = false;
    private bool sedangDialog = false;
    private bool sedangTransisiTeks = false;
    private Material blinkMaterial; // Material shader untuk efek Eye Blink

    [HideInInspector] 
    public bool sedangMencariBarang = false; // Flag untuk menghentikan dialog sampai barang dicari

    void Start()
    {
        instansiTombolNext = tombolNext;
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
        if (panelKoran != null) panelKoran.SetActive(false);
        if (panelKasur != null) panelKasur.SetActive(false);
        if (mainMenuPanel == null)
        {
            Transform tr = transform.Find("MainMenu");
            if (tr != null) mainMenuPanel = tr.gameObject;
        }
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);

        if (mainMenuScreenDialog == null)
        {
            Transform tr = null;
            if (blankMonitor != null)
            {
                tr = blankMonitor.transform.Find("MainMenuScreenDialog");
                if (tr == null) tr = blankMonitor.transform.Find("MainMenuScreen");
            }
            if (tr == null) tr = transform.Find("MainMenuScreenDialog");
            if (tr == null) tr = transform.Find("MainMenuScreen");
            if (tr != null) mainMenuScreenDialog = tr.gameObject;
        }
        if (mainMenuScreenDialog != null) mainMenuScreenDialog.SetActive(false);
        if (tombolNext != null) tombolNext.SetActive(false);
        if (panelDetailKoranLatar != null) panelDetailKoranLatar.SetActive(false);
        
        // Matikan efek TV di awal (intensity = 0 agar tidak terlihat)
        SetTVEffectIntensity(0f);
        
        if (loadingSlider != null) loadingSlider.gameObject.SetActive(false); // Sembunyikan slider di awal

        // Memulai coroutine urutan alur
        StartCoroutine(UrutanPengenalan());
    }

    void Update()
    {
        // Lanjut ke dialog berikutnya saat pemain klik kiri mouse atau tekan Spasi
        // Dicegah jika sedang transisi teks agar teks tidak bertumpuk/error
        // Dicegah juga jika sedang menyeret objek UI (DraggableUI)
        // Dicegah jika sedang di fase wajib mencari barang
        if (sedangDialog && !sedangTransisiTeks && !sedangMencariBarang && !DraggableUI.isInteractingWithUI && 
            (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
        {
            LanjutDialog();
        }

        // Mekanisme tombol ESC atau Klik Kanan untuk menutup detail koran
        if (sedangMelihatDetail && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)))
        {
            TutupDetailKoran();
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

        // Mematikan panel kasur jika masih menyala agar tidak menutupi booting TV
        if (panelKasur != null) panelKasur.SetActive(false);

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

        // 9. Jeda 3 detik setelah dialog berita selesai
        yield return new WaitForSeconds(3f);

        // 10. Transisi Blink penutup untuk kembali ke layar awal
        if (blackScreenPanel != null)
        {
            blackScreenPanel.SetActive(true);
            blackScreenPanel.transform.SetAsLastSibling();
            
            // Tutup mata (fade ke hitam penuh)
            yield return StartCoroutine(FadeBlackScreen(0f, 1f, 0.25f));
            
            // Gelap total selama 0.5 detik (Anda bisa sesuaikan waktunya)
            yield return new WaitForSeconds(0.5f);
            
            // Saat mata tertutup rapat, kembalikan ke panel lama & matikan efek TV
            if (beritaKonteksFullscreen != null) beritaKonteksFullscreen.SetActive(false);
            if (beritaKonteks != null) beritaKonteks.SetActive(true);
            SetTVEffectIntensity(0f);
            
            // Buka mata perlahan
            yield return StartCoroutine(FadeBlackScreen(1f, 0f, 0.4f));
            blackScreenPanel.SetActive(false);
        }
        else
        {
            // Fallback jika tidak ada objek black screen
            if (beritaKonteksFullscreen != null) beritaKonteksFullscreen.SetActive(false);
            if (beritaKonteks != null) beritaKonteks.SetActive(true);
            SetTVEffectIntensity(0f);
        }

        // 11. Jeda 1 detik sebelum mulai dialog kedua (opsional, agar tidak terlalu mendadak)
        yield return new WaitForSeconds(1f);

        // 12. Mulai Dialog Kedua Bagian 1 (indeks 0 sampai 1)
        yield return StartCoroutine(JalankanDialog(dialogKedua, 0, 1));

        // 13. Tampilkan Panel Koran setelah dialog kedua selesai dengan transisi
        yield return StartCoroutine(MunculkanKoran());

        // 14. Sembunyikan tombol Next koran setelah diklik agar tidak menghalangi dialog
        if (tombolNext != null) tombolNext.SetActive(false);

        // 15. Mulai Dialog Kedua Bagian 2 (array ke 3 sampai 5, yaitu indeks 2 sampai 4)
        yield return StartCoroutine(JalankanDialog(dialogKedua, 2, 4));

        // 16. Transisi Blink untuk berpindah dari Koran Panel ke MainMenuScreenDialog (di dalam TV)
        if (blackScreenPanel != null)
        {
            blackScreenPanel.SetActive(true);
            blackScreenPanel.transform.SetAsLastSibling();
            
            // Tutup mata (fade ke hitam penuh)
            yield return StartCoroutine(FadeBlackScreen(0f, 1f, 0.25f));
            yield return new WaitForSeconds(0.5f);
            
            // Saat mata tertutup rapat, matikan Koran Panel & nyalakan MainMenuScreenDialog
            if (panelKoran != null) panelKoran.SetActive(false);
            if (mainMenuScreenDialog != null) mainMenuScreenDialog.SetActive(true);
            
            // Buka mata perlahan
            yield return StartCoroutine(FadeBlackScreen(1f, 0f, 0.4f));
            blackScreenPanel.SetActive(false);
        }
        else
        {
            if (panelKoran != null) panelKoran.SetActive(false);
            if (mainMenuScreenDialog != null) mainMenuScreenDialog.SetActive(true);
        }

        // 17. Jeda 1 detik setelah panel kedip selesai dan benar-benar tersisa MainMenuScreenDialog
        yield return new WaitForSeconds(1f);

        // 18. Mulai kelanjutan Dialog Kedua (indeks 5 sampai selesai)
        if (dialogKedua != null && dialogKedua.Length > 5)
        {
            yield return StartCoroutine(JalankanDialog(dialogKedua, 5, -1));
        }

        // 19. Transisi Blink akhir untuk berpindah dari MainMenuScreenDialog ke MainMenu utama
        if (blackScreenPanel != null)
        {
            blackScreenPanel.SetActive(true);
            blackScreenPanel.transform.SetAsLastSibling();
            
            // Tutup mata (fade ke hitam penuh)
            yield return StartCoroutine(FadeBlackScreen(0f, 1f, 0.25f));
            yield return new WaitForSeconds(0.5f);
            
            // Saat mata tertutup rapat, matikan MainMenuScreenDialog & TV, lalu nyalakan MainMenu utama
            if (mainMenuScreenDialog != null) mainMenuScreenDialog.SetActive(false);
            if (blankMonitor != null) blankMonitor.SetActive(false);
            if (mainMenuPanel == null)
            {
                Transform tr = transform.Find("MainMenu");
                if (tr != null) mainMenuPanel = tr.gameObject;
            }
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
            
            // Buka mata perlahan
            yield return StartCoroutine(FadeBlackScreen(1f, 0f, 0.4f));
            blackScreenPanel.SetActive(false);
        }
        else
        {
            if (mainMenuScreenDialog != null) mainMenuScreenDialog.SetActive(false);
            if (blankMonitor != null) blankMonitor.SetActive(false);
            if (mainMenuPanel == null)
            {
                Transform tr = transform.Find("MainMenu");
                if (tr != null) mainMenuPanel = tr.gameObject;
            }
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        }

        // Selesai pengenalan
        Debug.Log("Alur Pengenalan Selesai! Berhasil berpindah ke Main Menu.");
    }

    IEnumerator MunculkanKoran()
    {
        if (panelKoran == null) yield break;

        // Pastikan tombol next sembunyi di awal kemunculan panel koran
        if (tombolNext != null) tombolNext.SetActive(false);

        // Pastikan semua koran dalam keadaan mati (hide) sebelum panel dinyalakan
        if (daftarGambarKoran != null)
        {
            foreach (var koran in daftarGambarKoran)
            {
                if (koran != null) koran.SetActive(false);
            }
        }

        // Siapkan panel koran untuk di-fade
        CanvasGroup cg = panelKoran.GetComponent<CanvasGroup>();
        if (cg == null) cg = panelKoran.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        
        panelKoran.SetActive(true);

        // Fade in keseluruhan panel (termasuk background blackFade)
        float durasiFade = 1f;
        float waktuMulai = Time.time;
        while (Time.time < waktuMulai + durasiFade)
        {
            cg.alpha = Mathf.Lerp(0f, 1f, (Time.time - waktuMulai) / durasiFade);
            yield return null;
        }
        cg.alpha = 1f;

        // Beri jeda sedikit setelah panel gelap muncul
        yield return new WaitForSeconds(0.5f);

        if (daftarGambarKoran != null)
        {
            // Munculkan koran satu per satu
            for (int i = 0; i < daftarGambarKoran.Length; i++)
            {
                if (daftarGambarKoran[i] == null) continue;

                // Jika bukan koran pertama, wajib tunggu player klik untuk memunculkannya
                if (i > 0)
                {
                    yield return new WaitUntil(() => Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return));
                    // Jeda 1 frame agar input tidak terbaca dobel
                    yield return null; 
                }

                // Munculkan gambar koran ke-i dengan efek slide dari bawah
                daftarGambarKoran[i].SetActive(true);
                yield return StartCoroutine(SlideDariBawahObjek(daftarGambarKoran[i].transform, 0.4f));
            }
        }

        // Beri jeda sejenak setelah semua koran selesai dimunculkan
        yield return new WaitForSeconds(0.3f);

        // Daftarkan event klik & animasi hover interaktif untuk setiap koran di meja
        if (daftarDetailKoran != null)
        {
            for (int i = 0; i < daftarDetailKoran.Length; i++)
            {
                int idx = i;
                if (daftarDetailKoran[i].koranDiMeja != null)
                {
                    Button btnKoran = daftarDetailKoran[i].koranDiMeja.GetComponent<Button>();
                    if (btnKoran == null) btnKoran = daftarDetailKoran[i].koranDiMeja.AddComponent<Button>();
                    btnKoran.interactable = true;
                    btnKoran.transition = Selectable.Transition.None; // Matikan warna abu-abu kusam bawaan tombol
                    btnKoran.onClick.RemoveAllListeners();
                    btnKoran.onClick.AddListener(() => StartCoroutine(BukaDetailKoran(idx)));

                    // Pasang efek animasi hover dinamis
                    AnimasiHoverUI hov = daftarDetailKoran[i].koranDiMeja.GetComponent<AnimasiHoverUI>();
                    if (hov == null) hov = daftarDetailKoran[i].koranDiMeja.AddComponent<AnimasiHoverUI>();
                }
            }
        }

        // Munculkan tombol Next secara Popup
        if (tombolNext != null)
        {
            tombolNext.transform.SetAsLastSibling();
            tombolNext.SetActive(true);
            yield return StartCoroutine(PopupObjekUI(tombolNext.transform, 0.35f));

            // Cari target gambar panah (child dari tombolNext)
            Transform targetPanah = gambarPanahNext;
            if (targetPanah == null && tombolNext.transform.childCount > 0)
            {
                targetPanah = tombolNext.transform.GetChild(0);
            }
            if (targetPanah == null) targetPanah = tombolNext.transform; // Fallback jika tidak ada child

            // Mulai animasi mengambang naik turun pada panah
            if (targetPanah != null)
            {
                StartCoroutine(AnimasikanNgambang(targetPanah));
            }

            // Siapkan deteksi klik tombol Next
            bool isNextClicked = false;
            Button btn = tombolNext.GetComponent<Button>();
            if (btn == null) btn = tombolNext.AddComponent<Button>();
            
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => isNextClicked = true);

            // Tunggu sampai pemain klik tombol Next atau tekan Spasi/Enter (dicegah jika sedang melihat detail berita)
            yield return new WaitUntil(() => isNextClicked || (!sedangMelihatDetail && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))));
        }
        else
        {
            // Fallback jika tombol Next belum di-assign di Inspector
            yield return new WaitUntil(() => !sedangMelihatDetail && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)));
        }

        // Hentikan animasi bergerak dan matikan interaksi koran saat tombol ditekan
        NonaktifkanDanResetSemuaKoran();
    }

    void NonaktifkanDanResetSemuaKoran()
    {
        if (daftarDetailKoran != null)
        {
            foreach (var detail in daftarDetailKoran)
            {
                if (detail.koranDiMeja != null)
                {
                    Button btn = detail.koranDiMeja.GetComponent<Button>();
                    if (btn != null) btn.interactable = false;

                    AnimasiHoverUI hov = detail.koranDiMeja.GetComponent<AnimasiHoverUI>();
                    if (hov != null) hov.ResetKePosisiAwal();
                }
            }
        }
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
            
            // Nyalakan efek Old TV bersamaan dengan tayangan berita fullscreen (via Volume Intensity)
            SetTVEffectIntensity(1f);

            // Pastikan panel dialog tetap berada paling depan menutupi semuanya
            if (bubbleNamePanel != null) bubbleNamePanel.transform.SetAsLastSibling();
        }
            // Dan blackscreen (kelopak mata) tetap paling depan saat mau dibuka
            blackScreenPanel.transform.SetAsLastSibling();
            
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

    IEnumerator JalankanDialog(DataDialog[] arrayDialog, int mulaiIndex = 0, int akhirIndex = -1)
    {
        arrayDialogAktif = arrayDialog;
        if (arrayDialogAktif != null && arrayDialogAktif.Length > 0)
        {
            if (akhirIndex == -1 || akhirIndex >= arrayDialogAktif.Length) akhirIndex = arrayDialogAktif.Length - 1;
            batasAkhirDialogSaatIni = akhirIndex;
            indeksDialogSaatIni = mulaiIndex;
            sedangDialog = true;
            sedangDialogAktif = true;
            NonaktifkanDanResetSemuaKoran();
            if (bubbleNamePanel != null) 
            {
                bubbleNamePanel.SetActive(true);
                bubbleNamePanel.transform.SetAsLastSibling(); // Pastikan UI Dialog selalu paling depan
            }
            
            // Animasi popup khusus untuk nama karakter (hanya di awal dialog)
            if (panelNamaKarakter != null)
            {
                StartCoroutine(PopupAwalObjek(panelNamaKarakter.transform, durasiTransisiTeks));
            }
            
            // Set transisi teks
            sedangTransisiTeks = true;
            
            // PENTING: Update teks dulu dengan data index 0 sebelum di-fade in
            // Supaya saat muncul otomatis, teks pertama sudah ada
            UpdateTeksUI();
            SetTeksAlpha(0f);
            
            // Kembalikan alpha parent ke 1, agar tidak double-fade dengan komponen di dalamnya
            if (bubbleNamePanel != null)
            {
                CanvasGroup cg = bubbleNamePanel.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 1f;
            }
            
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
        
        // Jika masih ada dialog selanjutnya dan belum melewati batas akhir yang ditentukan
        if (indeksDialogSaatIni <= batasAkhirDialogSaatIni && indeksDialogSaatIni < arrayDialogAktif.Length)
        {
            // Cek jika ini adalah dialog awal dan masuk ke baris ke-6 (indeks 5) (Transisi ke Kasur)
            if (arrayDialogAktif == dialogAwal && indeksDialogSaatIni == 5)
            {
                StartCoroutine(TransisiBlinkKeKasur());
            }
            // Setelah dialog ke-8 selesai dibaca (masuk ke indeks 8 / dialog 9), mulai pencarian!
            else if (arrayDialogAktif == dialogAwal && indeksDialogSaatIni == 8)
            {
                StartCoroutine(MulaiPencarianBarang());
            }
            // Setelah dialog ke-9 selesai (masuk ke indeks 9 / dialog 10), kembali ke monitor
            else if (arrayDialogAktif == dialogAwal && indeksDialogSaatIni == 9)
            {
                StartCoroutine(KembaliKeMonitorDanLanjut());
            }
            // Cek jika ini adalah dialog berita dan masuk ke baris ke-3 (indeks 2)
            else if (arrayDialogAktif == dialogBerita && indeksDialogSaatIni == 2)
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

    IEnumerator TransisiBlinkKeKasur()
    {
        sedangTransisiTeks = true;
        // Dialog masih berlanjut normal, jadi jangan kunci pencarian dulu
        // sedangMencariBarang tidak diaktifkan di sini

        // Fade out keseluruhan panel dialog agar benar-benar hilang sebelum layar berkedip
        if (bubbleNamePanel != null)
        {
            yield return StartCoroutine(FadeKeseluruhanPanel(bubbleNamePanel, 1f, 0f, durasiTransisiTeks));
        }
        else
        {
            yield return StartCoroutine(FadeTeks(1f, 0f, durasiTransisiTeks));
        }

        // Transisi Mata Tertutup (Blink)
        if (blackScreenPanel != null)
        {
            blackScreenPanel.SetActive(true);
            blackScreenPanel.transform.SetAsLastSibling();
            
            // Tutup mata perlahan
            yield return StartCoroutine(FadeBlackScreen(0f, 1f, 0.25f));
            
            // Saat mata tertutup rapat, nyalakan panel kasur
            yield return new WaitForSeconds(0.5f);
            if (panelKasur != null) panelKasur.SetActive(true);
            
            // Buka mata perlahan
            yield return StartCoroutine(FadeBlackScreen(1f, 0f, 0.4f));
            blackScreenPanel.SetActive(false);
        }
        else
        {
            // Fallback
            if (panelKasur != null) panelKasur.SetActive(true);
        }

        // Ubah ke teks dialog indeks ke-5 (baris ke-6)
        UpdateTeksUI();
        SetTeksAlpha(0f); // Reset alpha komponen sebelum di-fade in
        
        // Memunculkan kembali bubble name dengan animasi popup seperti saat dialog awal
        if (panelNamaKarakter != null)
        {
            StartCoroutine(PopupAwalObjek(panelNamaKarakter.transform, durasiTransisiTeks));
        }
        if (bubbleNamePanel != null)
        {
            CanvasGroup cg = bubbleNamePanel.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1f;
        }

        // Fade in teks dialog baru
        yield return StartCoroutine(FadeTeks(0f, 1f, durasiTransisiTeks));

        sedangTransisiTeks = false;
    }

    // Coroutine untuk menyembunyikan teks dan mengizinkan pemain mencari barang
    IEnumerator MulaiPencarianBarang()
    {
        sedangTransisiTeks = true;
        
        // Fade out keseluruhan panel dialog agar nama karakternya juga ikut hilang
        if (bubbleNamePanel != null)
        {
            yield return StartCoroutine(FadeKeseluruhanPanel(bubbleNamePanel, 1f, 0f, durasiTransisiTeks));
        }
        else
        {
            yield return StartCoroutine(FadeTeks(1f, 0f, durasiTransisiTeks));
        }
        
        // Kunci dialog dan izinkan player menggeser baju
        sedangMencariBarang = true;
        DraggableUI.canDrag = true;
        
        sedangTransisiTeks = false;
    }

    // Dipanggil saat item (Remote TV) berhasil diklik
    public void BarangDitemukan()
    {
        if (sedangMencariBarang)
        {
            sedangMencariBarang = false;
            DraggableUI.canDrag = false; // Kunci lagi bajunya agar tidak bisa digeser
            
            // Lanjut tampilkan dialog ke-9 (indeks 8)
            UpdateTeksUI();
            SetTeksAlpha(0f); // Reset komponen
            
            // Munculkan kembali nama dan background dialog
            if (panelNamaKarakter != null)
            {
                StartCoroutine(PopupAwalObjek(panelNamaKarakter.transform, durasiTransisiTeks));
            }
            if (bubbleNamePanel != null)
            {
                CanvasGroup cg = bubbleNamePanel.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 1f;
            }
            
            StartCoroutine(FadeTeks(0f, 1f, durasiTransisiTeks));
        }
    }

    IEnumerator KembaliKeMonitorDanLanjut()
    {
        sedangTransisiTeks = true;

        // Fade out teks (dialog ke-9)
        yield return StartCoroutine(FadeTeks(1f, 0f, durasiTransisiTeks));

        // Transisi Mata Tertutup (Blink)
        if (blackScreenPanel != null)
        {
            blackScreenPanel.SetActive(true);
            blackScreenPanel.transform.SetAsLastSibling();
            
            // Tutup mata perlahan
            yield return StartCoroutine(FadeBlackScreen(0f, 1f, 0.25f));
            
            yield return new WaitForSeconds(0.5f);
            
            // Matikan panel kasur, sehingga layar belakangnya (Blank Monitor) terlihat kembali
            if (panelKasur != null) panelKasur.SetActive(false);
            
            // Buka mata perlahan
            yield return StartCoroutine(FadeBlackScreen(1f, 0f, 0.4f));
            blackScreenPanel.SetActive(false);
        }
        else
        {
            if (panelKasur != null) panelKasur.SetActive(false);
        }

        // Lanjut ke indeks dialog berikutnya
        indeksDialogSaatIni++;
        
        if (indeksDialogSaatIni < arrayDialogAktif.Length)
        {
            UpdateTeksUI();
            yield return StartCoroutine(FadeTeks(0f, 1f, durasiTransisiTeks));
            sedangTransisiTeks = false;
        }
        else
        {
            // Jika ternyata dialog awal sudah habis, langsung selesaikan
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
        
        // Fade out keseluruhan panel dialog agar hilangnya lebih mulus
        if (bubbleNamePanel != null)
        {
            yield return StartCoroutine(FadeKeseluruhanPanel(bubbleNamePanel, 1f, 0f, durasiTransisiTeks));
        }
        else
        {
            // Fallback
            yield return StartCoroutine(FadeTeks(1f, 0f, durasiTransisiTeks));
        }

        if (bubbleNamePanel != null) bubbleNamePanel.SetActive(false);
        sedangDialog = false;
        sedangDialogAktif = false;
        sedangTransisiTeks = false;
    }

    IEnumerator FadeKeseluruhanPanel(GameObject panel, float alphaAwal, float alphaAkhir, float durasi)
    {
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();
        
        float waktuMulai = Time.time;
        while (Time.time < waktuMulai + durasi)
        {
            float progress = (Time.time - waktuMulai) / durasi;
            cg.alpha = Mathf.Lerp(alphaAwal, alphaAkhir, progress);
            yield return null;
        }
        cg.alpha = alphaAkhir;
    }

    void UpdateTeksUI()
    {
        // Update teks UI dengan data dari inspector
        if (textNamaKarakter != null) textNamaKarakter.text = arrayDialogAktif[indeksDialogSaatIni].namaKarakter;
        if (textIsiDialog != null) textIsiDialog.text = arrayDialogAktif[indeksDialogSaatIni].teksDialog;
    }

    void SetTeksAlpha(float alphaAkhir)
    {
        // Hanya background dialog dan teks isi yang ikut memudar/transisi
        if (textIsiDialog != null) 
        {
            Color c = textIsiDialog.color; c.a = alphaAkhir; textIsiDialog.color = c;
        }
        if (panelBackgroundDialog != null)
        {
            CanvasGroup cg = panelBackgroundDialog.GetComponent<CanvasGroup>();
            if (cg == null) cg = panelBackgroundDialog.AddComponent<CanvasGroup>();
            cg.alpha = alphaAkhir;
        }
        
        // Memastikan nama karakter & panel namanya selalu utuh 100% (tidak ikut transisi fade out)
        if (textNamaKarakter != null)
        {
            Color c = textNamaKarakter.color; c.a = 1f; textNamaKarakter.color = c;
        }
        if (panelNamaKarakter != null)
        {
            CanvasGroup cg = panelNamaKarakter.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1f;
        }
    }

    IEnumerator FadeTeks(float alphaAwal, float alphaAkhir, float durasi)
    {
        float waktuMulai = Time.time;
        bool isMuncul = (alphaAwal < alphaAkhir);

        // Sedikit percepat durasi asli dari inspektor agar popup terasa lebih cepat & responsif
        durasi = durasi * 0.85f; 

        // Tentukan target yang akan di-scale (Background jika ada, jika tidak fallback ke Teks saja)
        Transform targetScale = (panelBackgroundDialog != null) ? panelBackgroundDialog.transform : (textIsiDialog != null ? textIsiDialog.transform : null);
        if (targetScale != null) targetScale.localScale = Vector3.one;

        while (Time.time < waktuMulai + durasi)
        {
            float progress = (Time.time - waktuMulai) / durasi;
            
            // Alpha fade (memudar)
            float currentAlpha = Mathf.Lerp(alphaAwal, alphaAkhir, Mathf.Sin(progress * Mathf.PI * 0.5f));
            SetTeksAlpha(currentAlpha);
            
            // Animasi Popup Scale
            if (targetScale != null)
            {
                float scale = 1f;
                if (isMuncul)
                {
                    // Membesar dari 0.2 ke 1.0 dengan efek membal (Overshoot / Ease Out Back)
                    float t = progress - 1f;
                    float s = 2.0f; // Tingkat pantulan/bounce
                    float easeOutBack = (t * t * ((s + 1f) * t + s) + 1f);
                    
                    scale = Mathf.Lerp(0.2f, 1f, easeOutBack);
                }
                else
                {
                    // Mengecil dengan cepat dari 1.0 ke 0.2
                    float easeIn = progress * progress * progress; 
                    scale = Mathf.Lerp(1f, 0.2f, easeIn);
                }
                targetScale.localScale = new Vector3(scale, scale, 1f);
            }
            
            yield return null;
        }

        // Setel akhir untuk memastikan nilai pas dan akurat
        SetTeksAlpha(alphaAkhir);
        if (targetScale != null)
        {
            float finalScale = isMuncul ? 1f : 0.2f;
            targetScale.localScale = new Vector3(finalScale, finalScale, 1f);
        }
    }

    IEnumerator PopupAwalObjek(Transform obj, float durasi)
    {
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.gameObject.AddComponent<CanvasGroup>();
        
        cg.alpha = 0f;
        
        // Memastikan teks namanya juga ikut transparan di awal jika ada
        if (textNamaKarakter != null)
        {
            Color c = textNamaKarakter.color;
            c.a = 0f;
            textNamaKarakter.color = c;
        }

        obj.localScale = new Vector3(0.2f, 0.2f, 1f);
        float waktuMulai = Time.time;
        while (Time.time < waktuMulai + durasi)
        {
            float progress = (Time.time - waktuMulai) / durasi;
            float t = progress - 1f;
            float s = 2.0f;
            float easeOutBack = (t * t * ((s + 1f) * t + s) + 1f);
            float scale = Mathf.Lerp(0.2f, 1f, easeOutBack);
            obj.localScale = new Vector3(scale, scale, 1f);
            
            // Fade-in untuk kotak nama dan teksnya
            float currentAlpha = Mathf.Lerp(0f, 1f, progress);
            cg.alpha = currentAlpha;
            if (textNamaKarakter != null)
            {
                Color c = textNamaKarakter.color;
                c.a = currentAlpha;
                textNamaKarakter.color = c;
            }
            
            yield return null;
        }
        obj.localScale = Vector3.one;
        cg.alpha = 1f;
        
        if (textNamaKarakter != null)
        {
            Color c = textNamaKarakter.color;
            c.a = 1f;
            textNamaKarakter.color = c;
        }
    }

    IEnumerator SlideDariBawahObjek(Transform obj, float durasi)
    {
        RectTransform rectTransform = obj.GetComponent<RectTransform>();
        if (rectTransform == null) 
        {
            Debug.LogWarning("Objek " + obj.name + " tidak memiliki RectTransform! Transisi dibatalkan.");
            yield break;
        }

        Vector3 posisiAkhir = rectTransform.localPosition;
        // Geser Y awal ke jauh di bawah layar (dikurangi 1500 pixel dari posisi aslinya)
        Vector3 posisiAwal = posisiAkhir + new Vector3(0, -1500f, 0);
        
        rectTransform.localPosition = posisiAwal;
        
        float waktuMulai = Time.time;
        while (Time.time < waktuMulai + durasi)
        {
            float progress = (Time.time - waktuMulai) / durasi;
            
            // Menggunakan fungsi Cubic Ease-Out agar gerakannya melambat saat hampir sampai di atas
            float t = progress - 1f;
            float easeOut = t * t * t + 1f;
            
            rectTransform.localPosition = Vector3.Lerp(posisiAwal, posisiAkhir, easeOut);
            yield return null;
        }
        
        rectTransform.localPosition = posisiAkhir;
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
    // Mengontrol intensitas efek Old TV langsung melalui property _Intensity pada material
    void SetTVEffectIntensity(float intensity)
    {
        if (tvEffectMaterial == null) return;
        tvEffectMaterial.SetFloat("_Intensity", intensity);
    }

    IEnumerator PopupObjekUI(Transform obj, float durasi)
    {
        if (obj == null) yield break;
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.gameObject.AddComponent<CanvasGroup>();

        cg.alpha = 0f;
        obj.localScale = new Vector3(0.2f, 0.2f, 1f);

        float waktuMulai = Time.time;
        while (Time.time < waktuMulai + durasi)
        {
            float progress = (Time.time - waktuMulai) / durasi;
            float t = progress - 1f;
            float s = 2.0f; // Efek pantulan (overshoot bounce)
            float easeOutBack = (t * t * ((s + 1f) * t + s) + 1f);
            
            float scale = Mathf.Lerp(0.2f, 1f, easeOutBack);
            obj.localScale = new Vector3(scale, scale, 1f);
            cg.alpha = progress;
            
            yield return null;
        }

        obj.localScale = Vector3.one;
        cg.alpha = 1f;
    }

    IEnumerator AnimasikanNgambang(Transform target)
    {
        if (target == null) yield break;
        RectTransform rt = target.GetComponent<RectTransform>();
        Vector2 posAwalRT = rt != null ? rt.anchoredPosition : Vector2.zero;
        Vector3 posAwal = target.localPosition;
        float t = 0f;

        while (tombolNext != null)
        {
            yield return new WaitForEndOfFrame();
            if (tombolNext.activeInHierarchy && !sedangMelihatDetail)
            {
                t += Time.deltaTime * kecepatanNgambang;
                float offset = Mathf.Sin(t) * jarakNaikTurunPanah;
                if (rt != null)
                {
                    rt.anchoredPosition = posAwalRT + new Vector2(0f, offset);
                }
                else
                {
                    target.localPosition = posAwal + new Vector3(0f, offset, 0f);
                }
            }
        }
    }

    IEnumerator BukaDetailKoran(int index)
    {
        if (sedangDialogAktif || sedangMelihatDetail || daftarDetailKoran == null || index < 0 || index >= daftarDetailKoran.Length) yield break;
        
        DataDetailKoran data = daftarDetailKoran[index];
        if (data.panelDetailObjek == null || data.posterDetail == null) yield break;

        sedangMelihatDetail = true;

        // Sembunyikan tombol Next sementara saat melihat berita
        if (tombolNext != null) tombolNext.SetActive(false);

        // Simpan posisi desain asli kiri di Inspector jika belum tersimpan
        if (posisiAsliKiriPoster == null || posisiAsliKiriPoster.Length != daftarDetailKoran.Length)
        {
            posisiAsliKiriPoster = new Vector2[daftarDetailKoran.Length];
            for (int i = 0; i < daftarDetailKoran.Length; i++)
            {
                if (daftarDetailKoran[i].posterDetail != null)
                {
                    posisiAsliKiriPoster[i] = daftarDetailKoran[i].posterDetail.anchoredPosition;
                }
            }
        }

        // Aktifkan panel latar gelap
        if (panelDetailKoranLatar != null)
        {
            panelDetailKoranLatar.SetActive(true);
            panelDetailKoranLatar.transform.SetAsLastSibling();
            CanvasGroup cgLatar = panelDetailKoranLatar.GetComponent<CanvasGroup>();
            if (cgLatar == null) cgLatar = panelDetailKoranLatar.AddComponent<CanvasGroup>();
            cgLatar.alpha = 1f;

            // Tambahkan event klik latar untuk menutup berita
            Button btnLatar = panelDetailKoranLatar.GetComponent<Button>();
            if (btnLatar == null) btnLatar = panelDetailKoranLatar.AddComponent<Button>();
            btnLatar.onClick.RemoveAllListeners();
            btnLatar.onClick.AddListener(TutupDetailKoran);
        }

        // Matikan panel detail koran lain, nyalakan hanya yang dipilih
        for (int i = 0; i < daftarDetailKoran.Length; i++)
        {
            if (daftarDetailKoran[i].panelDetailObjek != null)
            {
                daftarDetailKoran[i].panelDetailObjek.SetActive(i == index);
            }
        }

        // Sembunyikan teks berita terlebih dahulu
        if (data.grupTeksBerita != null)
        {
            data.grupTeksBerita.alpha = 0f;
            data.grupTeksBerita.gameObject.SetActive(false);
        }

        // Letakkan poster di tengah layar terlebih dahulu
        data.posterDetail.anchoredPosition = Vector2.zero;
        data.posterDetail.localScale = new Vector3(0.2f, 0.2f, 1f);

        CanvasGroup cgPoster = data.posterDetail.GetComponent<CanvasGroup>();
        if (cgPoster == null) cgPoster = data.posterDetail.gameObject.AddComponent<CanvasGroup>();
        cgPoster.alpha = 0f;

        // 1. Animasi Popup Muncul di Tengah
        float durasiPopup = 0.35f;
        float waktuMulai = Time.time;
        while (Time.time < waktuMulai + durasiPopup)
        {
            float prog = (Time.time - waktuMulai) / durasiPopup;
            float t = prog - 1f;
            float s = 2.0f;
            float easeOutBack = (t * t * ((s + 1f) * t + s) + 1f);

            float scale = Mathf.Lerp(0.2f, 1f, easeOutBack);
            data.posterDetail.localScale = new Vector3(scale, scale, 1f);
            cgPoster.alpha = prog;
            yield return null;
        }
        data.posterDetail.localScale = Vector3.one;
        cgPoster.alpha = 1f;

        // 2. Diam sejenak di tengah layar agar pemain bisa mengamati poster
        yield return new WaitForSeconds(0.45f);

        // 3. Animasi Geser ke Kiri (menuju posisi desain Inspector)
        Vector2 targetKiri = posisiAsliKiriPoster[index];
        float durasiGeser = 0.4f;
        waktuMulai = Time.time;
        while (Time.time < waktuMulai + durasiGeser)
        {
            float prog = (Time.time - waktuMulai) / durasiGeser;
            float t = prog - 1f;
            float easeOut = t * t * t + 1f;

            data.posterDetail.anchoredPosition = Vector2.Lerp(Vector2.zero, targetKiri, easeOut);
            yield return null;
        }
        data.posterDetail.anchoredPosition = targetKiri;

        // 4. Animasi Munculkan Teks di Sebelah Kanan
        if (data.grupTeksBerita != null)
        {
            data.grupTeksBerita.gameObject.SetActive(true);
            data.grupTeksBerita.blocksRaycasts = true;
            data.grupTeksBerita.interactable = true;

            SiapkanDanPerbaikiScroll(data);

            ScrollRect scrollTarget = data.panelDetailObjek.GetComponentInChildren<ScrollRect>(true);
            if (scrollTarget != null) scrollTarget.verticalNormalizedPosition = 1f;

            float durasiTeks = 0.3f;
            waktuMulai = Time.time;
            while (Time.time < waktuMulai + durasiTeks)
            {
                data.grupTeksBerita.alpha = (Time.time - waktuMulai) / durasiTeks;
                yield return null;
            }
            data.grupTeksBerita.alpha = 1f;

            // Kunci posisi scroll di baris awal (paling atas) setelah animasi fade selesai
            if (scrollTarget != null) scrollTarget.verticalNormalizedPosition = 1f;
        }
    }

    public void TutupDetailKoran()
    {
        if (!sedangMelihatDetail) return;
        StartCoroutine(ProsesTutupDetail());
    }

    IEnumerator ProsesTutupDetail()
    {
        if (panelDetailKoranLatar != null)
        {
            CanvasGroup cg = panelDetailKoranLatar.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                float dur = 0.2f;
                float wm = Time.time;
                while (Time.time < wm + dur)
                {
                    cg.alpha = Mathf.Lerp(1f, 0f, (Time.time - wm) / dur);
                    yield return null;
                }
            }
            panelDetailKoranLatar.SetActive(false);
        }

        if (daftarDetailKoran != null)
        {
            for (int i = 0; i < daftarDetailKoran.Length; i++)
            {
                if (daftarDetailKoran[i].panelDetailObjek != null)
                    daftarDetailKoran[i].panelDetailObjek.SetActive(false);
            }
        }

        sedangMelihatDetail = false;

        if (tombolNext != null)
        {
            tombolNext.transform.SetAsLastSibling();
            tombolNext.SetActive(true);
        }
    }

    void SiapkanDanPerbaikiScroll(DataDetailKoran data)
    {
        if (data.panelDetailObjek == null) return;

        ScrollRect scroll = data.panelDetailObjek.GetComponentInChildren<ScrollRect>(true);
        if (scroll != null)
        {
            scroll.vertical = true;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 30f;

            Graphic gScroll = scroll.GetComponent<Graphic>();
            if (gScroll == null) gScroll = scroll.gameObject.AddComponent<Image>();
            gScroll.raycastTarget = true;
            if (gScroll is Image img && scroll.GetComponent<Image>() == img && img.color.a > 0.01f)
            {
                Color c = img.color; c.a = 0.001f; img.color = c;
            }

            if (scroll.content != null)
            {
                ContentSizeFitter csf = scroll.content.GetComponent<ContentSizeFitter>();
                if (csf == null) csf = scroll.content.gameObject.AddComponent<ContentSizeFitter>();
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                
                LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
            }

            // Wajib set ke 1.0 (Top) agar scrollbar selalu mulai dari baris pertama teks
            scroll.verticalNormalizedPosition = 1f;
        }
    }
}
