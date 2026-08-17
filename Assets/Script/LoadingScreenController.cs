using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingScreenController : MonoBehaviour
{
    public static LoadingScreenController Instance { get; private set; }

    [Header("Panel Loading")]
    public GameObject panelLoading;

    [Header("Transisi & Jeda")]
    public bool transisiKeluarMenuDulu = true;
    public float durasiKeluarMenu = 0.35f;
    public float minimalWaktuLoading = 1.2f;
    [Tooltip("Lama layar ditahan warna hitam SEBELUM mulai transisi keluar (memberi waktu agar panel baru siap)")]
    public float jedaSebelumKeluar = 0.5f;

    [Header("Efek Transisi Pixel")]
    public Material materialTransisiPixel;
    public float durasiTransisiPixel = 0.5f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // sembunyiin panel loading tapi jangan di nonaktifin (SetActive false)
        if (panelLoading != null)
        {
            CanvasGroup cg = panelLoading.GetComponent<CanvasGroup>();
            if (cg == null) cg = panelLoading.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable = false;
        }
    }

    public void MuatScene(string namaSceneTujuan)
    {
        if (!gameObject.activeInHierarchy) gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(ProsesLoading(namaSceneTujuan));
    }

    public void MuatSceneIndex(int indeksScene)
    {
        if (!gameObject.activeInHierarchy) gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(ProsesLoadingIndex(indeksScene));
    }

    public void BukaSceneKamar()
    {
        MuatScene("Kamar");
    }

    // --- TRANSISI LOKAL (PINDAH RUANGAN TANPA PINDAH SCENE) ---
    public void TransisiLokalEvent(UnityEngine.Events.UnityEvent eventDitengahTransisi)
    {
        if (!gameObject.activeInHierarchy) gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(ProsesTransisiLokal(eventDitengahTransisi, minimalWaktuLoading));
    }

    public void TransisiLokalEventDenganDurasi(UnityEngine.Events.UnityEvent eventDitengahTransisi, float durasiGelap)
    {
        if (!gameObject.activeInHierarchy) gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(ProsesTransisiLokal(eventDitengahTransisi, durasiGelap));
    }

    IEnumerator ProsesTransisiLokal(UnityEngine.Events.UnityEvent eventTengah, float durasiTahanLayarGelap)
    {
        // 1. Siapkan panel dan mulai putar video di belakang layar (tersembunyi dulu)
        AktifkanPanelDanMulaiGif();

        // 2. Transisi masuk (layar memudar perlahan menjadi hitam/pixelated)
        if (materialTransisiPixel != null)
        {
            yield return StartCoroutine(JalankanTransisiPixel(true));
            
            // 3. SETELAH LAYAR BENAR-BENAR HITAM, MUNCULKAN VIDEO LOADING!
            AturVisibilitasVideo(true);
        }

        // 4. Tahan layar loading agar pemain bisa melihat animasi GIF/Video-nya atau untuk jeda cerita
        yield return new WaitForSeconds(durasiTahanLayarGelap);

        // 5. Jalankan Event perpindahan ruangan (Kamar mati, Outdoor nyala) secara instan di balik layar gelap
        eventTengah?.Invoke();

        // 5b. Jeda tambahan: Tahan layar gelap sebentar agar objek/panel baru sempat ter-render dan tidak kaget
        yield return new WaitForSeconds(jedaSebelumKeluar);

        AturVisibilitasVideo(false);

        // 6. Transisi keluar (layar kembali jernih dan menampilkan ruangan baru)
        if (materialTransisiPixel != null)
        {
            yield return StartCoroutine(JalankanTransisiPixel(false));
        }
        else
        {
            yield return new WaitForSeconds(0.3f);
        }

        if (panelLoading != null)
        {
            panelLoading.SetActive(false);
        }
    }

    IEnumerator ProsesLoading(string namaScene)
    {
        // 1. Jalankan animasi keluar pada semua tombol di Main Menu terlebih dahulu
        if (transisiKeluarMenuDulu)
        {
            yield return StartCoroutine(JalankanAnimasiOutSemuaMenu());
        }

        // 2. Aktifkan Panel Loading (muncul penuh atau dengan Transisi Pixel)
        AktifkanPanelDanMulaiGif();
        
        if (materialTransisiPixel != null)
        {
            yield return StartCoroutine(JalankanTransisiPixel(true));
            
            // 2b. MUNCULKAN VIDEO SETELAH LAYAR BENAR-BENAR HITAM SEPENUHNYA!
            AturVisibilitasVideo(true);
        }

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

        if (materialTransisiPixel != null)
        {
            yield return StartCoroutine(JalankanTransisiPixel(true));
            
            AturVisibilitasVideo(true);
        }

        AsyncOperation operasi = SceneManager.LoadSceneAsync(indeksScene);
        yield return StartCoroutine(PantauProgressDanSelesaikan(operasi));
    }

    IEnumerator JalankanTransisiPixel(bool isMasuk)
    {
        if (materialTransisiPixel == null) yield break;

        // isMasuk = True: Transisi Muncul (Invisible -> Visible) (Progress 1 -> 0)
        // isMasuk = False: Transisi Keluar (Visible -> Invisible) (Progress 0 -> 1)
        float startVal = isMasuk ? 1f : 0f;
        float endVal = isMasuk ? 0f : 1f;
        float timer = 0f;

        materialTransisiPixel.SetFloat("_Invert", 0f);

        while (timer < durasiTransisiPixel)
        {
            timer += Time.deltaTime;
            float p = Mathf.Lerp(startVal, endVal, timer / durasiTransisiPixel);
            materialTransisiPixel.SetFloat("_Progress", p);
            yield return null;
        }

        materialTransisiPixel.SetFloat("_Progress", endVal);
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

            // Garansi 100% panel Loading dipindah ke urutan paling bawah di Hierarchy Canvas agar berada DI LAPISAN PALING DEPAN
            panelLoading.transform.SetAsLastSibling();

            // SANGAT PENTING: Jika objek ditaruh sembarangan (bukan di dalam Canvas), UI akan menjadi transparan/hilang!
            // Kita garansi objek ini memiliki Canvas-nya sendiri dan menutupi layar sepenuhnya.
            Canvas c = panelLoading.GetComponent<Canvas>();
            if (c == null)
            {
                c = panelLoading.AddComponent<Canvas>();
                panelLoading.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 9999;

            CanvasGroup cgPanel = panelLoading.GetComponent<CanvasGroup>();
            if (cgPanel != null) { cgPanel.alpha = 1f; cgPanel.blocksRaycasts = true; }

            // Beri animasi pop-in halus pada panel loading jika memiliki script AnimasiTombolMenu
            AnimasiTombolMenu animPanel = panelLoading.GetComponent<AnimasiTombolMenu>();
            if (animPanel != null)
            {
                animPanel.JalankanUlangAnimasiIn();
            }

            // Cari semua VideoPlayer di panelLoading (termasuk pada anak seperti RawImage)
            UnityEngine.Video.VideoPlayer[] videoPlayers = panelLoading.GetComponentsInChildren<UnityEngine.Video.VideoPlayer>(true);
            foreach (var vp in videoPlayers)
            {
                if (vp == null) continue;

                if (!vp.gameObject.activeInHierarchy) vp.gameObject.SetActive(true);

                // Periksa apakah VideoPlayer menggunakan Render Mode : Render Texture tetapi Target Texture-nya masih kosong (None)
                if (vp.renderMode == UnityEngine.Video.VideoRenderMode.RenderTexture && vp.targetTexture == null)
                {
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

                // Sembunyikan visual RawImage sementara sebelum layar benar-benar hitam (transisi selesai)
                RawImage rImg = vp.GetComponent<RawImage>();
                if (rImg != null && materialTransisiPixel != null) rImg.enabled = false;

                vp.Stop();
                vp.Play();
            }
        }
    }

    void AturVisibilitasVideo(bool terlihat)
    {
        if (panelLoading == null) return;
        
        // HANYA matikan RawImage yang digunakan untuk memutar Video. 
        // Jangan matikan semua RawImage, karena RawImage mungkin digunakan untuk Material Transisi Pixel!
        UnityEngine.Video.VideoPlayer[] videoPlayers = panelLoading.GetComponentsInChildren<UnityEngine.Video.VideoPlayer>(true);
        foreach (var vp in videoPlayers)
        {
            if (vp != null)
            {
                RawImage rawImg = vp.GetComponent<RawImage>();
                if (rawImg != null)
                {
                    rawImg.enabled = terlihat;
                }
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
                AturVisibilitasVideo(false);

                // Agar script ini dan layar loading tidak hancur saat pindah scene:
                if (panelLoading != null)
                {
                    panelLoading.transform.SetParent(null);
                    
                    // Beri komponen Canvas sendiri agar tetap terlihat di layar
                    Canvas c = panelLoading.GetComponent<Canvas>();
                    if (c == null)
                    {
                        c = panelLoading.AddComponent<Canvas>();
                        c.renderMode = RenderMode.ScreenSpaceOverlay;
                        c.sortingOrder = 9999;
                        panelLoading.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                    }

                    DontDestroyOnLoad(panelLoading);
                    
                    // Jika script ini tidak menempel di panelLoading, bawa juga script ini!
                    if (this.gameObject != panelLoading) DontDestroyOnLoad(this.gameObject);
                }

                // 1. IZINKAN BERPINDAH KE SCENE BARU (KAMAR) SEKARANG!
                operasi.allowSceneActivation = true;

                // Tunggu sampai scene baru benar-benar sudah aktif di layar
                yield return new WaitUntil(() => operasi.isDone);
                yield return new WaitForEndOfFrame();

                // 2. JALANKAN TRANSISI OUT (Layar terbuka menampilkan Scene Baru!)
                if (materialTransisiPixel != null)
                {
                    yield return StartCoroutine(JalankanTransisiPixel(false));
                }
                else
                {
                    yield return new WaitForSeconds(0.3f);
                }

                // 3. SETELAH SELESAI, HANCURKAN PANEL LOADING AGAR TIDAK MENGGANGGU GAME
                if (panelLoading != null) Destroy(panelLoading);
                if (this.gameObject != panelLoading) Destroy(this.gameObject);

                yield break;
            }

            yield return null;
        }
    }
}
