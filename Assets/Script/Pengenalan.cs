using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Ditambahkan untuk mendukung TextMeshPro

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
    public GameObject bubbleNamePanel;

    [Header("Referensi Teks UI")]
    public TMP_Text textNamaKarakter; 
    public TMP_Text textIsiDialog;    

    [Header("Pengaturan Dialog (Bisa diisi banyak orang)")]
    public DataDialog[] dialogAwal;
    public DataDialog[] dialogBerita;

    [Header("Pengaturan Waktu")]
    public float durasiFadeBlack = 1.5f;
    public float jedaSebelumDialog = 3f;
    public float durasiBootingTV = 5f;
    public float durasiFadeOut = 1f;
    public float durasiTransisiTeks = 0.3f; // Waktu fade in/out teks

    private int indeksDialogSaatIni = 0;
    private DataDialog[] arrayDialogAktif;
    private bool sedangDialog = false;
    private bool sedangTransisiTeks = false;

    void Start()
    {
        // 1. Kondisi awal: Semua panel di-reset, yang muncul pertama adalah blank monitor dan layar hitam
        if (blackScreenPanel != null) 
        {
            blackScreenPanel.SetActive(true);
            CanvasGroup cg = blackScreenPanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = blackScreenPanel.AddComponent<CanvasGroup>();
            cg.alpha = 1f; // Pastikan layar benar-benar gelap di awal
            blackScreenPanel.transform.SetAsLastSibling(); // Pastikan layar hitam berada paling depan
        }

        if (blankMonitor != null) blankMonitor.SetActive(true);
        if (bootingTV != null) bootingTV.SetActive(false);
        if (beritaKonteks != null) beritaKonteks.SetActive(false);
        if (bubbleNamePanel != null) bubbleNamePanel.SetActive(false);

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
        // 1.5. Transisi Fade dari Hitam ke Transparan (membuka layar)
        if (blackScreenPanel != null)
        {
            yield return StartCoroutine(FadeOutObjek(blackScreenPanel, durasiFadeBlack));
            blackScreenPanel.SetActive(false); // Nonaktifkan panel hitam setelah pudar
        }

        // 2. Tunggu beberapa detik (default 3 detik) sebelum memunculkan dialog
        yield return new WaitForSeconds(jedaSebelumDialog);

        // 3. Munculkan dialog awal (Anda bisa isi sendiri nama dan dialognya di Inspector)
        yield return StartCoroutine(JalankanDialog(dialogAwal));

        // 4. Setelah dialog awal selesai (di-klik), munculkan panel Booting TV
        if (bootingTV != null) bootingTV.SetActive(true);
        
        // 5. Tunggu selama durasi booting TV (default 5 detik)
        yield return new WaitForSeconds(durasiBootingTV);

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

        // 8. Muncul kembali dialog dengan pembawa acara
        yield return StartCoroutine(JalankanDialog(dialogBerita));

        // Selesai pengenalan, di sini Anda bisa menambahkan script untuk lanjut ke scene berikutnya
        Debug.Log("Alur Pengenalan Selesai! Lanjut ke main menu/gameplay.");
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
            
            // Siapkan teks pertama (transparan)
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
            StartCoroutine(TransisiTeksBerikutnya());
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
