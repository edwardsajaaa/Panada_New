using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Pengelola Loading Screen (memutar animasi GIF dari VideoPlayer atau Sprite Sequence, 
/// transisi keluar dari Main Menu, serta menampilkan Progress Bar & Teks Persentase).
/// Pasang script ini di Canvas utama atau di GameObject khusus LoadingScreenController.
/// </summary>
public class LoadingScreenController : MonoBehaviour
{
    public static LoadingScreenController Instance { get; private set; }

    [Header("=== 1. Referensi Panel Loading ===")]
    [Tooltip("Panel UI Loading Screen (misalnya GameObject Panel Loading yang berisi RawImage VideoPlayer)")]
    public GameObject panelLoading;

    [Header("=== 2. Pengaturan Transisi & Jeda ===")]
    [Tooltip("Apakah jalankan dulu animasi keluar (Out) pada Main Menu sebelum Loading Screen muncul?")]
    public bool transisiKeluarMenuDulu = true;
    [Tooltip("Durasi animasi keluar Main Menu (detik)")]
    public float durasiKeluarMenu = 0.35f;
    [Tooltip("Waktu jeda minimal (detik) agar loading screen tidak terkesan berkedip terlalu cepat jika komputer pemain sangat cepat")]
    public float minimalWaktuLoading = 1.2f;

    void Awake()
    {
        // Setup Singleton agar mudah dipanggil dari tombol mana saja
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Sembunyikan panel loading secara visual menggunakan CanvasGroup (alpha = 0)
        // JANGAN gunakan SetActive(false) karena script ini terpasang di panel loading itu sendiri —
        // jika dimatikan, Coroutine tidak bisa dijalankan saat tombol Play diklik!
        if (panelLoading != null)
        {
            CanvasGroup cg = panelLoading.GetComponent<CanvasGroup>();
            if (cg == null) cg = panelLoading.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable = false;
        }
    }

    /// <summary>
    /// Panggil fungsi ini dari OnClick() Tombol Mulai / Play di Inspector atau dari script lain.
    /// Contoh: LoadingScreenController.Instance.MuatScene("NamaSceneTujuan");
    /// </summary>
    public void MuatScene(string namaSceneTujuan)
    {
        StopAllCoroutines();
        StartCoroutine(ProsesLoading(namaSceneTujuan));
    }

    /// <summary>
    /// Panggil fungsi ini jika ingin memuat scene berdasarkan Index (nomor urut di Build Settings).
    /// </summary>
    public void MuatSceneIndex(int indeksScene)
    {
        StopAllCoroutines();
        StartCoroutine(ProsesLoadingIndex(indeksScene));
    }

    /// <summary>
    /// Panggil fungsi ini dari OnClick() tombol Play di Main Menu untuk memuat scene Kamar dengan Loading Screen.
    /// </summary>
    public void BukaSceneKamar()
    {
        MuatScene("Kamar");
    }

    IEnumerator ProsesLoading(string namaScene)
    {
        // 1. Jalankan animasi keluar pada semua tombol di Main Menu terlebih dahulu
        if (transisiKeluarMenuDulu)
        {
            yield return StartCoroutine(JalankanAnimasiOutSemuaMenu());
        }

        // 2. Aktifkan Panel Loading (muncul penuh)
        AktifkanPanelDanMulaiGif();

        // 3. Mulai proses load scene di belakang layar (Async)
        AsyncOperation operasi = SceneManager.LoadSceneAsync(namaScene);
        yield return StartCoroutine(PantauProgressDanSelesaikan(operasi));
    }

    IEnumerator ProsesLoadingIndex(int indeksScene)
    {
        if (transisiKeluarMenuDulu)
        {
            yield return StartCoroutine(JalankanAnimasiOutSemuaMenu());
        }

        AktifkanPanelDanMulaiGif();

        AsyncOperation operasi = SceneManager.LoadSceneAsync(indeksScene);
        yield return StartCoroutine(PantauProgressDanSelesaikan(operasi));
    }

    IEnumerator JalankanAnimasiOutSemuaMenu()
    {
        TransisiMenuUI transisiMenu = FindObjectOfType<TransisiMenuUI>();
        System.Collections.Generic.List<GameObject> daftarKeluar = new System.Collections.Generic.List<GameObject>();

        if (transisiMenu != null)
        {
            if (transisiMenu.objekMainMenu != null)
            {
                foreach (var o in transisiMenu.objekMainMenu) if (o != null && o != panelLoading && !daftarKeluar.Contains(o)) daftarKeluar.Add(o);
            }
            foreach (Transform anak in transisiMenu.transform)
            {
                if (anak.gameObject == transisiMenu.panelSetting || anak.gameObject == transisiMenu.panelCredit || anak.gameObject == panelLoading || !anak.gameObject.activeInHierarchy) continue;
                if (!daftarKeluar.Contains(anak.gameObject)) daftarKeluar.Add(anak.gameObject);
            }
        }

        if (daftarKeluar.Count > 0)
        {
            foreach (GameObject obj in daftarKeluar)
            {
                if (obj == null || !obj.activeInHierarchy) continue;
                AnimasiTombolMenu anim = obj.GetComponent<AnimasiTombolMenu>();
                if (anim == null)
                {
                    anim = obj.AddComponent<AnimasiTombolMenu>();
                    anim.modeAnimasiOut = AnimasiTombolMenu.ModeAnimasiIn.PopInBawah;
                    anim.durasiAnimasiOut = durasiKeluarMenu;
                    anim.gunakanAnimasiOut = true;
                }
                anim.JalankanAnimasiOut(null, true);
            }
            yield return new WaitForSeconds(durasiKeluarMenu);
            foreach (GameObject obj in daftarKeluar)
            {
                if (obj != null) obj.SetActive(false);
            }
        }
        else
        {
            // Jika tidak menggunakan TransisiMenuUI, cari semua AnimasiTombolMenu di root Canvas saat ini
            AnimasiTombolMenu[] semuaTombol = FindObjectsOfType<AnimasiTombolMenu>();
            foreach (var anim in semuaTombol)
            {
                if (anim != null && anim.gameObject.activeInHierarchy && anim.gameObject != panelLoading)
                {
                    anim.JalankanAnimasiOut(null, true);
                }
            }
            yield return new WaitForSeconds(durasiKeluarMenu);
        }
    }

    void AktifkanPanelDanMulaiGif()
    {
        if (panelLoading != null)
        {
            panelLoading.SetActive(true);

            // Garansi 100% panel Loading dipindah ke urutan paling bawah di Hierarchy Canvas agar berada DI LAPISAN PALING DEPAN dan tidak terhalang oleh panel lain (seperti STARTING MENU / SETTING)!
            panelLoading.transform.SetAsLastSibling();

            CanvasGroup cgPanel = panelLoading.GetComponent<CanvasGroup>();
            if (cgPanel != null) { cgPanel.alpha = 1f; cgPanel.blocksRaycasts = true; }

            // Beri animasi pop-in halus pada panel loading jika memiliki script AnimasiTombolMenu
            AnimasiTombolMenu animPanel = panelLoading.GetComponent<AnimasiTombolMenu>();
            if (animPanel != null)
            {
                animPanel.JalankanUlangAnimasiIn();
            }

            // --- OTOMATISASI & PERBAIKAN VIDEO PLAYER ---
            // Cari semua VideoPlayer di panelLoading (termasuk pada anak seperti RawImage)
            UnityEngine.Video.VideoPlayer[] videoPlayers = panelLoading.GetComponentsInChildren<UnityEngine.Video.VideoPlayer>(true);
            foreach (var vp in videoPlayers)
            {
                if (vp == null) continue;

                // Pastikan GameObject VideoPlayer aktif
                if (!vp.gameObject.activeInHierarchy) vp.gameObject.SetActive(true);

                // Periksa apakah VideoPlayer menggunakan Render Mode : Render Texture tetapi Target Texture-nya masih kosong (None)
                if (vp.renderMode == UnityEngine.Video.VideoRenderMode.RenderTexture && vp.targetTexture == null)
                {
                    // Cari RawImage pada objek yang sama
                    RawImage rawImg = vp.GetComponent<RawImage>();
                    if (rawImg != null)
                    {
                        if (rawImg.texture is RenderTexture rt)
                        {
                            // Hubungkan secara otomatis RenderTexture yang ada di RawImage ke TargetTexture VideoPlayer!
                            vp.targetTexture = rt;
                        }
                        else
                        {
                            // Jika di RawImage belum ada RenderTexture, buat RenderTexture baru 1920x1080 secara dinamis dan pasang ke keduanya!
                            RenderTexture baruRT = new RenderTexture(1920, 1080, 16);
                            baruRT.Create();
                            vp.targetTexture = baruRT;
                            rawImg.texture = baruRT;
                        }
                    }
                }

                // Mulai putar video dari awal
                vp.Stop();
                vp.Play();
            }
        }
    }

    IEnumerator PantauProgressDanSelesaikan(AsyncOperation operasi)
    {
        // Tahan perpindahan scene sampai progress 100% dan minimalWaktuLoading terpenuhi
        operasi.allowSceneActivation = false;

        float waktuMulaiLoading = Time.time;

        while (!operasi.isDone)
        {
            // Jika progress sudah mencapai 100% (0.9 pada Unity) dan waktu minimal loading sudah terpenuhi
            if (operasi.progress >= 0.9f && (Time.time - waktuMulaiLoading) >= minimalWaktuLoading)
            {
                // Jeda pendek sebelum scene baru terbuka agar animasi selesai dengan sempurna
                yield return new WaitForSeconds(0.3f);
                operasi.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
