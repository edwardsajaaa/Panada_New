using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Pengelola transisi animasi Out & In antara Main Menu dan Setting Panel (atau panel lainnya).
/// Pasang script ini di GameObject MainMenu atau Canvas utama.
/// </summary>
public class TransisiMenuUI : MonoBehaviour
{
    [Header("=== 1. Referensi Objek Main Menu ===")]
    [Tooltip("Daftar semua GameObject di Main Menu yang ingin meluncur KELUAR berbarengan saat masuk ke Setting/Credit (misal: Koran, HP, Kopi, Logo Rerantau)")]
    public GameObject[] objekMainMenu;

    [Header("=== 2. Referensi Panel Setting ===")]
    [Tooltip("Panel Setting yang akan muncul setelah Main Menu meluncur keluar (misal: Setting Panel)")]
    public GameObject panelSetting;

    [Tooltip("Daftar tombol/objek di dalam Setting Panel yang akan meluncur KELUAR saat tombol Kembali ditekan (opsional)")]
    public GameObject[] objekPanelSetting;

    [Header("=== 3. Referensi Panel Credit ===")]
    [Tooltip("Panel Credit yang akan muncul setelah Main Menu meluncur keluar saat Logo Panada diklik (misal: Credit Panel)")]
    public GameObject panelCredit;

    [Tooltip("Daftar tombol/objek di dalam Panel Credit yang akan meluncur KELUAR saat tombol Kembali ditekan (opsional)")]
    public GameObject[] objekPanelCredit;

    [Header("=== 4. Pengaturan Transisi ===")]
    [Tooltip("Waktu tunggu (durasi Animasi Out) sebelum Panel Setting/Credit dibuka")]
    public float jedaTransisi = 0.35f;

    /// <summary>
    /// Panggil fungsi ini pada OnClick() tombol Setting di Main Menu
    /// </summary>
    public void BukaPanelSetting()
    {
        StopAllCoroutines();
        StartCoroutine(ProsesBukaPanel(panelSetting));
    }

    /// <summary>
    /// Panggil fungsi ini saat Logo Panada diklik untuk membuka Credit Panel
    /// </summary>
    public void BukaPanelCredit()
    {
        StopAllCoroutines();
        StartCoroutine(ProsesBukaPanel(panelCredit));
    }

    /// <summary>
    /// Panggil fungsi ini pada OnClick() tombol Mulai / Play di Main Menu untuk pindah ke scene permainan dengan animasi Loading Screen (GIF/Video).
    /// </summary>
    public void BukaSceneDenganLoading(string namaScene)
    {
        LoadingScreenController lsc = DapatkanLoadingScreenController();
        if (lsc != null)
        {
            lsc.MuatScene(namaScene);
        }
        else
        {
            Debug.LogWarning("LoadingScreenController tidak ditemukan di adegan! Langsung memuat scene...");
            UnityEngine.SceneManagement.SceneManager.LoadScene(namaScene);
        }
    }

    /// <summary>
    /// Panggil fungsi ini jika ingin memuat scene berdasarkan Index (nomor urut di Build Settings).
    /// </summary>
    public void BukaSceneDenganLoadingIndex(int indeksScene)
    {
        LoadingScreenController lsc = DapatkanLoadingScreenController();
        if (lsc != null)
        {
            lsc.MuatSceneIndex(indeksScene);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(indeksScene);
        }
    }

    /// <summary>
    /// Panggil fungsi ini dari OnClick() tombol Play di Main Menu untuk langsung memuat scene Kamar dengan Loading Screen.
    /// </summary>
    public void BukaSceneKamar()
    {
        BukaSceneDenganLoading("Kamar");
    }

    /// <summary>
    /// Mencari LoadingScreenController — cek Instance dulu, jika null cari manual di scene (termasuk di GameObject nonaktif).
    /// </summary>
    private LoadingScreenController DapatkanLoadingScreenController()
    {
        if (LoadingScreenController.Instance != null) return LoadingScreenController.Instance;

        // Instance null karena Loading Screen Panel nonaktif di scene dan Awake belum pernah jalan.
        // Cari secara manual di seluruh scene, termasuk yang nonaktif.
        LoadingScreenController lsc = FindObjectOfType<LoadingScreenController>(true);
        if (lsc != null)
        {
            // Aktifkan dulu agar Awake() berjalan dan Instance ter-set
            if (!lsc.gameObject.activeInHierarchy) lsc.gameObject.SetActive(true);
            return lsc;
        }

        return null;
    }


    /// <summary>
    /// Panggil fungsi ini pada OnClick() tombol Kembali/Tutup di dalam Setting Panel atau Credit Panel
    /// </summary>
    public void KembaliKeMainMenu()
    {
        StopAllCoroutines();
        if (panelSetting != null && panelSetting.activeInHierarchy)
        {
            StartCoroutine(ProsesKembaliKeMainMenu(panelSetting, objekPanelSetting));
        }
        else if (panelCredit != null && panelCredit.activeInHierarchy)
        {
            StartCoroutine(ProsesKembaliKeMainMenu(panelCredit, objekPanelCredit));
        }
        else
        {
            StartCoroutine(ProsesKembaliKeMainMenu(panelSetting, objekPanelSetting));
        }
    }

    IEnumerator ProsesBukaPanel(GameObject targetPanel)
    {
        if (targetPanel == null) yield break;

        // Kumpulkan semua objek yang akan di-animasikan keluar (dari array + otomatis semua anak MainMenu selain panelSetting dan panelCredit)
        System.Collections.Generic.List<GameObject> daftarKeluar = new System.Collections.Generic.List<GameObject>();
        if (objekMainMenu != null)
        {
            foreach (var o in objekMainMenu) if (o != null && !daftarKeluar.Contains(o)) daftarKeluar.Add(o);
        }
        foreach (Transform anak in transform)
        {
            if (anak.gameObject == panelSetting || anak.gameObject == panelCredit || !anak.gameObject.activeInHierarchy) continue;
            if (!daftarKeluar.Contains(anak.gameObject)) daftarKeluar.Add(anak.gameObject);
        }

        // 1. Jalankan Animasi Out secara berbarengan pada semua objek
        foreach (GameObject obj in daftarKeluar)
        {
            if (obj == null || !obj.activeInHierarchy) continue;
            AnimasiTombolMenu anim = obj.GetComponent<AnimasiTombolMenu>();
            if (anim == null)
            {
                anim = obj.AddComponent<AnimasiTombolMenu>();
                anim.modeAnimasiOut = AnimasiTombolMenu.ModeAnimasiIn.PopInBawah;
                anim.durasiAnimasiOut = jedaTransisi;
                anim.gunakanAnimasiOut = true;
            }
            anim.JalankanAnimasiOut(null, true);
        }

        // 2. Tunggu sampai semua objek Main Menu selesai meluncur keluar
        yield return new WaitForSeconds(jedaTransisi);

        // Pastikan tidak ada tombol yang masih tertinggal aktif di layar
        foreach (GameObject obj in daftarKeluar)
        {
            if (obj != null) obj.SetActive(false);
        }

        // 3. Aktifkan Panel Target (Setting atau Credit) dan jalankan animasi In-nya secara berbarengan
        targetPanel.SetActive(true);
        AnimasiTombolMenu[] animAnak = targetPanel.GetComponentsInChildren<AnimasiTombolMenu>(true);
        foreach (var a in animAnak)
        {
            if (!a.gameObject.activeInHierarchy) a.gameObject.SetActive(true);
            a.JalankanUlangAnimasiIn();
        }
    }

    IEnumerator ProsesKembaliKeMainMenu(GameObject targetPanel, GameObject[] objekTargetPanel)
    {
        // 1. Jalankan Animasi Out pada Panel Target (Setting/Credit) beserta anak-anaknya
        if (objekTargetPanel != null && objekTargetPanel.Length > 0)
        {
            foreach (GameObject obj in objekTargetPanel)
            {
                if (obj == null || !obj.activeInHierarchy) continue;
                AnimasiTombolMenu anim = obj.GetComponent<AnimasiTombolMenu>();
                if (anim == null)
                {
                    anim = obj.AddComponent<AnimasiTombolMenu>();
                    anim.modeAnimasiOut = AnimasiTombolMenu.ModeAnimasiIn.PopInBawah;
                    anim.durasiAnimasiOut = jedaTransisi;
                    anim.gunakanAnimasiOut = true;
                }
                anim.JalankanAnimasiOut(null, true);
            }
        }
        else if (targetPanel != null)
        {
            AnimasiTombolMenu[] animAnak = targetPanel.GetComponentsInChildren<AnimasiTombolMenu>(true);
            foreach (var a in animAnak)
            {
                if (a.gameObject.activeInHierarchy) a.JalankanAnimasiOut(null, true);
            }
            AnimasiTombolMenu animPanel = targetPanel.GetComponent<AnimasiTombolMenu>();
            if (animPanel != null) animPanel.JalankanAnimasiOut(() => targetPanel.SetActive(false), true);
            else targetPanel.SetActive(false);
        }

        // 2. Tunggu sampai Panel Target selesai keluar
        yield return new WaitForSeconds(jedaTransisi);
        if (targetPanel != null) targetPanel.SetActive(false);

        // 3. Aktifkan kembali semua objek Main Menu secara berbarengan (baik dari array maupun semua anak MainMenu selain panelSetting dan panelCredit)
        System.Collections.Generic.List<GameObject> daftarMasuk = new System.Collections.Generic.List<GameObject>();
        if (objekMainMenu != null)
        {
            foreach (var o in objekMainMenu) if (o != null && !daftarMasuk.Contains(o)) daftarMasuk.Add(o);
        }
        foreach (Transform anak in transform)
        {
            if (anak.gameObject == panelSetting || anak.gameObject == panelCredit) continue;
            if (!daftarMasuk.Contains(anak.gameObject)) daftarMasuk.Add(anak.gameObject);
        }

        foreach (GameObject obj in daftarMasuk)
        {
            if (obj == null) continue;
            obj.SetActive(true);
            AnimasiTombolMenu anim = obj.GetComponent<AnimasiTombolMenu>();
            if (anim != null) anim.JalankanUlangAnimasiIn();
        }
    }
}
