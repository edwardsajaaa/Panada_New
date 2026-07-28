using System.Collections;
using UnityEngine;
using TMPro;

public class SistemDialogKamar : MonoBehaviour
{
    [Header("Referensi UI")]
    public GameObject panelBubleName;
    public TMP_Text teksNamaKarakter;
    public TMP_Text teksIsiDialog;

    [Header("Data Percakapan")]
    [Tooltip("Percakapan utama yang dimainkan pertama kali")]
    public DataDialog[] percakapan;
    [Tooltip("Percakapan yang dimainkan setelah membalas pesan (Skenario Lanjutan)")]
    public DataDialog[] percakapanLanjutan;

    [HideInInspector]
    public bool gunakanLanjutan = false;

    private DataDialog[] DialogAktif 
    { 
        get { return (gunakanLanjutan && percakapanLanjutan != null && percakapanLanjutan.Length > 0) ? percakapanLanjutan : percakapan; } 
    }

    [Header("Pengaturan Transisi Masuk (Ketik)")]
    [Tooltip("Waktu jeda (detik) per karakter saat mengetik")]
    public float kecepatanKetik = 0.03f; 

    public enum TransisiKeluar { Fade, PopOut, HilangLangsung, Blink }
    
    [Header("Pengaturan Transisi Keluar")]
    public TransisiKeluar transisiBuble = TransisiKeluar.PopOut;
    [Tooltip("Durasi tutup untuk Buble Name")]
    public float durasiTutupBuble = 0.3f;

    [Header("Transisi Blink (Opsional)")]
    [Tooltip("Masukkan BlackScreenPanel jika memilih transisiBuble = Blink")]
    public GameObject panelLayarHitam;
    public float durasiTutupMata = 0.25f;
    public float jedaGelap = 0.15f;
    public float durasiBukaMata = 0.4f;

    [Header("Aksi Setelah Dialog Habis (Urutan Normal)")]
    public GameObject[] objekYangIkutMati;
    public GameObject[] objekYangDinyalakan;
    [Tooltip("Animasi keluar untuk objek yang ikut mati (otomatis dipasangi script AnimasiTombolMenu)")]
    public AnimasiTombolMenu.ModeAnimasiIn transisiObjekLain = AnimasiTombolMenu.ModeAnimasiIn.PopInBawah;

    [Header("Aksi Setelah Dialog Lanjutan Habis (Balas Sekarang)")]
    public GameObject[] objekYangIkutMatiLanjutan;
    public GameObject[] objekYangDinyalakanLanjutan;
    [Tooltip("Pilih objek induk (misal: Story) untuk dimatikan di akhir dialog lanjutan")]
    public GameObject panelStoryUtama;
    [Tooltip("Pilih objek Loading Screen (misal: PixelOverlay) yang akan dinyalakan saat layar sudah gelap sempurna")]
    public GameObject panelLoadingScreen;

    [Header("Pengaturan Efek Zoom (Opsional)")]
    [Tooltip("Masukkan Panel Meja atau background yang ingin di-zoom")]
    public RectTransform panelUntukZoom;
    
    [Tooltip("Jika dicentang, posisi & skala Zoom In akan otomatis mengikuti posisi meja saat dialog ini ditutup (Sangat disarankan!).")]
    public bool gunakanPosisiAwalSebagaiZoomIn = true;
    
    public Vector3 skalaZoomOut = Vector3.one;
    public Vector3 skalaZoomIn = new Vector3(1.2f, 1.2f, 1f);
    
    [Tooltip("Jika dicentang, proses Zoom Out tidak memiliki animasi (instan).")]
    public bool zoomOutInstan = true;
    
    [Header("Pengaturan Posisi (Berdasarkan Left, Bottom, Right, Top)")]
    public bool ubahPosisiJuga = true;
    [Tooltip("Normal: (Left, Bottom)")]
    public Vector2 offsetMinZoomOut = Vector2.zero;
    [Tooltip("Normal: (-Right, -Top)")]
    public Vector2 offsetMaxZoomOut = Vector2.zero;
    [Tooltip("Hanya dipakai jika gunakanPosisiAwal = false")]
    public Vector2 offsetMinZoomIn = new Vector2(160f, 94.585f);
    [Tooltip("Hanya dipakai jika gunakanPosisiAwal = false")]
    public Vector2 offsetMaxZoomIn = new Vector2(-160f, -89.585f);

    public float durasiAnimasiZoom = 0.5f;
    [Tooltip("Waktu tunggu setelah zoom out sebelum kembali zoom in")]
    public float jedaSebelumZoomInLagi = 3f;
    
    [Header("Trigger Objek")]
    [Tooltip("Handphone atau objek yang akan dinyalakan tepat SEBELUM animasi zoom in dimulai")]
    public GameObject objekNyalaSaatZoomIn;
    [Tooltip("Objek yang akan diaktifkan SETELAH zoom in selesai (misal: untuk memunculkan notifikasi baru)")]
    public GameObject objekTriggerSetelahZoom;

    private int indeksDialog = 0;
    private bool sedangTransisi = false;
    private Coroutine transisiCoroutine;
    private bool sedangDitutup = false;

    private Vector3 savedZoomInScale;
    private Vector2 savedZoomInMin;
    private Vector2 savedZoomInMax;

    void OnEnable()
    {
        indeksDialog = 0;
        sedangDitutup = false;
        
        if (panelBubleName != null)
        {
            // Set durasi standar untuk popup awal
            StartCoroutine(PopupAwalObjek(panelBubleName.transform, 0.3f));
        }

        if (DialogAktif != null && DialogAktif.Length > 0)
        {
            TampilkanDialogSekarang();
        }
    }

    void Update()
    {
        if (sedangDitutup) return; // Kunci input jika sedang proses tutup

        if (Input.GetMouseButtonDown(0))
        {
            if (sedangTransisi)
            {
                if (transisiCoroutine != null) StopCoroutine(transisiCoroutine);
                
                // Langsung tampilkan semua karakter
                if (teksIsiDialog != null)
                {
                    teksIsiDialog.maxVisibleCharacters = teksIsiDialog.text.Length;
                }
                sedangTransisi = false;
            }
            else
            {
                LanjutKeDialogBerikutnya();
            }
        }
    }

    void TampilkanDialogSekarang()
    {
        if (indeksDialog < DialogAktif.Length)
        {
            if (teksNamaKarakter != null)
                teksNamaKarakter.text = DialogAktif[indeksDialog].namaKarakter;

            if (transisiCoroutine != null) StopCoroutine(transisiCoroutine);
            transisiCoroutine = StartCoroutine(KetikTeks(DialogAktif[indeksDialog].teksDialog));
        }
    }

    IEnumerator KetikTeks(string teks)
    {
        sedangTransisi = true;
        SetTeksAlpha(1f); // Pastikan alpha tidak tembus pandang
        
        teksIsiDialog.text = teks;
        // Hitung total karakter bersih yang dimiliki oleh TextMeshPro
        teksIsiDialog.ForceMeshUpdate();
        int totalKarakter = teksIsiDialog.textInfo.characterCount;
        
        teksIsiDialog.maxVisibleCharacters = 0;

        for (int i = 0; i <= totalKarakter; i++)
        {
            teksIsiDialog.maxVisibleCharacters = i;
            yield return new WaitForSecondsRealtime(kecepatanKetik);
        }

        sedangTransisi = false;
    }

    void SetTeksAlpha(float alpha)
    {
        if (teksIsiDialog != null)
        {
            Color c = teksIsiDialog.color;
            c.a = alpha;
            teksIsiDialog.color = c;
        }
    }

    IEnumerator PopupAwalObjek(Transform obj, float durasi)
    {
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.gameObject.AddComponent<CanvasGroup>();
        
        cg.alpha = 0f;
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
            cg.alpha = Mathf.Lerp(0f, 1f, progress);
            
            yield return null;
        }
        
        obj.localScale = Vector3.one;
        cg.alpha = 1f;
    }

    void LanjutKeDialogBerikutnya()
    {
        indeksDialog++;
        if (indeksDialog < DialogAktif.Length)
        {
            TampilkanDialogSekarang();
        }
        else
        {
            if (!sedangDitutup)
            {
                sedangDitutup = true;

                if (transisiBuble == TransisiKeluar.Blink && panelLayarHitam != null)
                {
                    // Aktifkan layar hitam lebih dulu agar bisa menjalankan coroutine
                    panelLayarHitam.SetActive(true);
                    
                    MonoBehaviour runner = panelLayarHitam.GetComponent<UnityEngine.UI.Image>();
                    if (runner == null) runner = this;

                    runner.StartCoroutine(BlinkOutRoutine(runner));
                }
                else
                {
                    StartCoroutine(FadeOutLaluTutup());
                }
            }
        }
    }

    IEnumerator BlinkOutRoutine(MonoBehaviour runner)
    {
        panelLayarHitam.transform.SetAsLastSibling();

        UnityEngine.UI.Image bgImage = panelLayarHitam.GetComponent<UnityEngine.UI.Image>();
        Material originalMat = null;
        Material blinkMat = null;

        if (bgImage != null && bgImage.material != null && bgImage.material.HasProperty("_Blink"))
        {
            originalMat = bgImage.material;
            blinkMat = new Material(originalMat);
            bgImage.material = blinkMat;
            blinkMat.SetFloat("_Blink", 0f);
        }

        // 1. TUTUP MATA
        float waktu = 0f;
        while (waktu < durasiTutupMata)
        {
            waktu += Time.unscaledDeltaTime;
            if (blinkMat != null) blinkMat.SetFloat("_Blink", Mathf.Clamp01(waktu / durasiTutupMata));
            yield return null;
        }
        if (blinkMat != null) blinkMat.SetFloat("_Blink", 1f);

        // 2. JEDA GELAP & TUKAR OBJEK
        yield return new WaitForSecondsRealtime(jedaGelap);
        
        if (panelBubleName != null) panelBubleName.SetActive(false);

        Transform oldLayarHitamParent = null;

        if (gunakanLanjutan)
        {
            if (objekYangIkutMatiLanjutan != null) foreach (var obj in objekYangIkutMatiLanjutan) if (obj != null) obj.SetActive(false);
            if (objekYangDinyalakanLanjutan != null) foreach (var obj in objekYangDinyalakanLanjutan) if (obj != null) obj.SetActive(true);
            
            if (panelStoryUtama != null)
            {
                // Lepas layar hitam sementara agar tidak mati saat panel Story dimatikan
                oldLayarHitamParent = panelLayarHitam.transform.parent;
                panelLayarHitam.transform.SetParent(null);
                
                panelStoryUtama.SetActive(false);
            }

            // --- NYALAKAN LOADING SCREEN SAAT GELAP SEMPURNA ---
            if (panelLoadingScreen != null)
            {
                // Lepas parent Loading Screen jika dia berada di dalam Story
                panelLoadingScreen.transform.SetParent(null);
                panelLoadingScreen.SetActive(true);
                
                // Transisi tetap dilanjutkan ke Buka Mata agar tampilan 3D terlihat
            }
        }
        else
        {
            if (objekYangIkutMati != null) foreach (var obj in objekYangIkutMati) if (obj != null) obj.SetActive(false);
            if (objekYangDinyalakan != null) foreach (var obj in objekYangDinyalakan) if (obj != null) obj.SetActive(true);

            // Paksa matikan Handphone (atau objek trigger) agar tidak muncul selama Zoom Out
            if (objekNyalaSaatZoomIn != null)
            {
                objekNyalaSaatZoomIn.SetActive(false);
            }

            // SNAP INSTAN ZOOM OUT (Saat layar gelap)
            if (panelUntukZoom != null)
            {
                savedZoomInScale = gunakanPosisiAwalSebagaiZoomIn ? panelUntukZoom.localScale : skalaZoomIn;
                savedZoomInMin = gunakanPosisiAwalSebagaiZoomIn ? panelUntukZoom.offsetMin : offsetMinZoomIn;
                savedZoomInMax = gunakanPosisiAwalSebagaiZoomIn ? panelUntukZoom.offsetMax : offsetMaxZoomIn;

                if (zoomOutInstan)
                {
                    panelUntukZoom.localScale = skalaZoomOut;
                    if (ubahPosisiJuga)
                    {
                        panelUntukZoom.offsetMin = offsetMinZoomOut;
                        panelUntukZoom.offsetMax = offsetMaxZoomOut;
                    }
                }
            }
        }

        // 3. BUKA MATA
        waktu = 0f;
        while (waktu < durasiBukaMata)
        {
            waktu += Time.unscaledDeltaTime;
            if (blinkMat != null) blinkMat.SetFloat("_Blink", Mathf.Clamp01(1f - (waktu / durasiBukaMata)));
            yield return null;
        }
        if (blinkMat != null) blinkMat.SetFloat("_Blink", 0f);

        panelLayarHitam.SetActive(false);
        
        if (gunakanLanjutan && oldLayarHitamParent != null)
        {
            // Kembalikan ke parent asalnya
            panelLayarHitam.transform.SetParent(oldLayarHitamParent);
        }

        if (bgImage != null && originalMat != null) bgImage.material = originalMat;
        if (blinkMat != null) Destroy(blinkMat);
        
        sedangDitutup = false;

        // 4. JALANKAN ZOOM SETELAH MATA TERBUKA (Hanya jika bukan lanjutan)
        if (!gunakanLanjutan && panelUntukZoom != null)
        {
            // Buat objek sementara sebagai runner agar coroutine tidak mati
            GameObject tempRunnerObj = new GameObject("TempZoomRunner");
            MonoBehaviour zoomRunner = tempRunnerObj.AddComponent<AnimasiNotifikasiGanda>();
            zoomRunner.StartCoroutine(ProsesZoomSekuensial(tempRunnerObj));
        }
    }

    IEnumerator FadeOutLaluTutup()
    {
        if (!gunakanLanjutan)
        {
            // Paksa matikan Handphone agar tidak muncul selama Zoom Out
            if (objekNyalaSaatZoomIn != null)
            {
                objekNyalaSaatZoomIn.SetActive(false);
            }

            if (panelUntukZoom != null)
            {
                savedZoomInScale = gunakanPosisiAwalSebagaiZoomIn ? panelUntukZoom.localScale : skalaZoomIn;
                savedZoomInMin = gunakanPosisiAwalSebagaiZoomIn ? panelUntukZoom.offsetMin : offsetMinZoomIn;
                savedZoomInMax = gunakanPosisiAwalSebagaiZoomIn ? panelUntukZoom.offsetMax : offsetMaxZoomIn;

                if (zoomOutInstan)
                {
                    panelUntukZoom.localScale = skalaZoomOut;
                    if (ubahPosisiJuga)
                    {
                        panelUntukZoom.offsetMin = offsetMinZoomOut;
                        panelUntukZoom.offsetMax = offsetMaxZoomOut;
                    }
                }
            }
        }

        // 1. Jalankan animasi keluar untuk semua objek yang ikut mati
        GameObject[] matiTarget = gunakanLanjutan ? objekYangIkutMatiLanjutan : objekYangIkutMati;
        if (matiTarget != null)
        {
            foreach (var obj in matiTarget)
            {
                if (obj == null) continue;
                
                AnimasiTombolMenu anim = obj.GetComponent<AnimasiTombolMenu>();
                if (anim == null)
                {
                    anim = obj.gameObject.AddComponent<AnimasiTombolMenu>();
                    anim.gunakanAnimasiIn = false; // Cegah ter-trigger animasi IN
                    anim.gunakanAnimasiOut = true;
                    anim.ResetKePosisiAwal(); // Kembalikan ke wujud normal sebelum animasi out
                }
                anim.modeAnimasiOut = transisiObjekLain;
                anim.durasiAnimasiOut = 0.4f;
                // Paksa objek animasi turun lalu mati otomatis
                anim.JalankanAnimasiOut(null, true); 
            }
        }

        // 2. Animasi keluar untuk Buble Name
        if (panelBubleName != null)
        {
            if (transisiBuble == TransisiKeluar.HilangLangsung)
            {
                panelBubleName.SetActive(false);
            }
            else
            {
                CanvasGroup cg = panelBubleName.GetComponent<CanvasGroup>();
                if (cg == null) cg = panelBubleName.AddComponent<CanvasGroup>();

                float waktuMulai = Time.time;
                while (Time.time < waktuMulai + durasiTutupBuble)
                {
                    float progress = (Time.time - waktuMulai) / durasiTutupBuble;
                    
                    // Fade out selalu jalan
                    cg.alpha = Mathf.Lerp(1f, 0f, progress);
                    
                    // Scale down cuma kalau mode PopOut
                    if (transisiBuble == TransisiKeluar.PopOut)
                    {
                        float scale = Mathf.Lerp(1f, 0.2f, progress);
                        panelBubleName.transform.localScale = new Vector3(scale, scale, 1f);
                    }
                    
                    yield return null;
                }
                cg.alpha = 0f;
                panelBubleName.SetActive(false);
            }
        }

        // 3. Nyalakan objek yang harus hidup lagi (misal HP meja)
        GameObject[] nyalaTarget = gunakanLanjutan ? objekYangDinyalakanLanjutan : objekYangDinyalakan;
        if (nyalaTarget != null)
        {
            foreach (var obj in nyalaTarget)
            {
                if (obj != null) obj.SetActive(true);
            }
        }

        if (gunakanLanjutan && panelStoryUtama != null)
        {
            panelStoryUtama.SetActive(false);
        }

        sedangDitutup = false;

        // 4. JALANKAN ZOOM SETELAH TRANSISI SELESAI
        if (!gunakanLanjutan && panelUntukZoom != null)
        {
            GameObject tempRunnerObj = new GameObject("TempZoomRunner");
            MonoBehaviour zoomRunner = tempRunnerObj.AddComponent<AnimasiNotifikasiGanda>();
            zoomRunner.StartCoroutine(ProsesZoomSekuensial(tempRunnerObj));
        }
    }

    IEnumerator ProsesZoomSekuensial(GameObject tempRunnerObj = null)
    {
        if (panelUntukZoom == null)
        {
            if (tempRunnerObj != null) Destroy(tempRunnerObj);
            yield break;
        }

        // 1. Animasi Zoom Out (Dilewati jika instan)
        if (!zoomOutInstan)
        {
            float waktu = 0f;
            Vector3 awalScale = panelUntukZoom.localScale;
            Vector2 awalMin = panelUntukZoom.offsetMin;
            Vector2 awalMax = panelUntukZoom.offsetMax;

            while (waktu < durasiAnimasiZoom)
            {
                waktu += Time.unscaledDeltaTime;
                float progress = waktu / durasiAnimasiZoom;
                panelUntukZoom.localScale = Vector3.Lerp(awalScale, skalaZoomOut, progress);
                
                if (ubahPosisiJuga)
                {
                    panelUntukZoom.offsetMin = Vector2.Lerp(awalMin, offsetMinZoomOut, progress);
                    panelUntukZoom.offsetMax = Vector2.Lerp(awalMax, offsetMaxZoomOut, progress);
                }
                yield return null;
            }
            panelUntukZoom.localScale = skalaZoomOut;
            if (ubahPosisiJuga)
            {
                panelUntukZoom.offsetMin = offsetMinZoomOut;
                panelUntukZoom.offsetMax = offsetMaxZoomOut;
            }
        }


        yield return new WaitForSecondsRealtime(jedaSebelumZoomInLagi);

        float waktuIn = 0f;
        Vector3 awalScaleIn = panelUntukZoom.localScale;
        Vector2 awalMinIn = panelUntukZoom.offsetMin;
        Vector2 awalMaxIn = panelUntukZoom.offsetMax;

        while (waktuIn < durasiAnimasiZoom)
        {
            waktuIn += Time.unscaledDeltaTime;
            float progress = waktuIn / durasiAnimasiZoom;
            panelUntukZoom.localScale = Vector3.Lerp(awalScaleIn, savedZoomInScale, progress);
            
            if (ubahPosisiJuga)
            {
                panelUntukZoom.offsetMin = Vector2.Lerp(awalMinIn, savedZoomInMin, progress);
                panelUntukZoom.offsetMax = Vector2.Lerp(awalMaxIn, savedZoomInMax, progress);
            }
            yield return null;
        }
        panelUntukZoom.localScale = savedZoomInScale;
        if (ubahPosisiJuga)
        {
            panelUntukZoom.offsetMin = savedZoomInMin;
            panelUntukZoom.offsetMax = savedZoomInMax;
        }

  
        if (objekNyalaSaatZoomIn != null)
        {
            objekNyalaSaatZoomIn.SetActive(true);
        }

        if (objekTriggerSetelahZoom != null)
        {
            objekTriggerSetelahZoom.SetActive(true);
        }

        if (tempRunnerObj != null) Destroy(tempRunnerObj);
    }
}
