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
    public DataDialog[] percakapan;

    [Header("Pengaturan Transisi Masuk")]
    public float durasiTransisiTeks = 0.3f; 

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

    [Header("Aksi Setelah Dialog Habis")]
    public GameObject[] objekYangIkutMati;
    public GameObject[] objekYangDinyalakan;
    [Tooltip("Animasi keluar untuk objek yang ikut mati (otomatis dipasangi script AnimasiTombolMenu)")]
    public AnimasiTombolMenu.ModeAnimasiIn transisiObjekLain = AnimasiTombolMenu.ModeAnimasiIn.PopInBawah;

    [Header("Pengaturan Efek Zoom (Opsional)")]
    [Tooltip("Masukkan Panel Meja atau background yang ingin di-zoom")]
    public RectTransform panelUntukZoom;
    public Vector3 skalaZoomOut = Vector3.one;
    public Vector3 skalaZoomIn = new Vector3(1.2f, 1.2f, 1f);
    public float durasiAnimasiZoom = 0.5f;
    [Tooltip("Waktu tunggu setelah zoom out sebelum kembali zoom in")]
    public float jedaSebelumZoomInLagi = 3f;
    [Tooltip("Objek yang akan diaktifkan setelah zoom in selesai (misal: untuk memunculkan notifikasi baru)")]
    public GameObject objekTriggerSetelahZoom;

    private int indeksDialog = 0;
    private bool sedangTransisi = false;
    private Coroutine transisiCoroutine;
    private bool sedangDitutup = false;

    void OnEnable()
    {
        indeksDialog = 0;
        sedangDitutup = false;
        
        if (panelBubleName != null)
        {
            StartCoroutine(PopupAwalObjek(panelBubleName.transform, durasiTransisiTeks));
        }

        if (percakapan != null && percakapan.Length > 0)
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
                SetTeksAlpha(1f);
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
        if (indeksDialog < percakapan.Length)
        {
            if (teksNamaKarakter != null)
                teksNamaKarakter.text = percakapan[indeksDialog].namaKarakter;

            teksIsiDialog.text = percakapan[indeksDialog].teksDialog;

            if (transisiCoroutine != null) StopCoroutine(transisiCoroutine);
            transisiCoroutine = StartCoroutine(FadeTeks(0f, 1f, durasiTransisiTeks));
        }
    }

    IEnumerator FadeTeks(float alphaAwal, float alphaAkhir, float durasi)
    {
        sedangTransisi = true;
        SetTeksAlpha(alphaAwal);
        
        float waktuMulai = Time.time;
        while (Time.time < waktuMulai + durasi)
        {
            float progress = (Time.time - waktuMulai) / durasi;
            SetTeksAlpha(Mathf.Lerp(alphaAwal, alphaAkhir, progress));
            yield return null;
        }
        
        SetTeksAlpha(alphaAkhir);
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
        if (indeksDialog < percakapan.Length)
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
        if (objekYangIkutMati != null) foreach (var obj in objekYangIkutMati) if (obj != null) obj.SetActive(false);
        if (objekYangDinyalakan != null) foreach (var obj in objekYangDinyalakan) if (obj != null) obj.SetActive(true);

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
        if (bgImage != null && originalMat != null) bgImage.material = originalMat;
        if (blinkMat != null) Destroy(blinkMat);
        
        sedangDitutup = false;

        // 4. JALANKAN ZOOM SETELAH MATA TERBUKA
        if (panelUntukZoom != null && runner != null)
        {
            runner.StartCoroutine(ProsesZoomSekuensial());
        }
    }

    IEnumerator FadeOutLaluTutup()
    {
        // ================= JIKA BUKAN BLINK =================

        // 1. Jalankan animasi keluar untuk semua objek yang ikut mati
        if (objekYangIkutMati != null)
        {
            foreach (var obj in objekYangIkutMati)
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
        if (objekYangDinyalakan != null)
        {
            foreach (var obj in objekYangDinyalakan)
            {
                if (obj != null) obj.SetActive(true);
            }
        }

        sedangDitutup = false;

        // 4. JALANKAN ZOOM SETELAH TRANSISI SELESAI
        if (panelUntukZoom != null)
        {
            StartCoroutine(ProsesZoomSekuensial());
        }
    }

    IEnumerator ProsesZoomSekuensial()
    {
        if (panelUntukZoom == null) yield break;

        // 1. Animasi Zoom Out
        float waktu = 0f;
        Vector3 awal = panelUntukZoom.localScale;
        while (waktu < durasiAnimasiZoom)
        {
            waktu += Time.unscaledDeltaTime;
            panelUntukZoom.localScale = Vector3.Lerp(awal, skalaZoomOut, waktu / durasiAnimasiZoom);
            yield return null;
        }
        panelUntukZoom.localScale = skalaZoomOut;

        // 2. Jeda
        yield return new WaitForSecondsRealtime(jedaSebelumZoomInLagi);

        // 3. Animasi Zoom In
        waktu = 0f;
        while (waktu < durasiAnimasiZoom)
        {
            waktu += Time.unscaledDeltaTime;
            panelUntukZoom.localScale = Vector3.Lerp(skalaZoomOut, skalaZoomIn, waktu / durasiAnimasiZoom);
            yield return null;
        }
        panelUntukZoom.localScale = skalaZoomIn;

        // 4. Aktifkan trigger notifikasi (jika ada)
        if (objekTriggerSetelahZoom != null)
        {
            objekTriggerSetelahZoom.SetActive(true);
        }
    }
}
