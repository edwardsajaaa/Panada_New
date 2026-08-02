using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TransisiMenuUI : MonoBehaviour
{
    [Header("Main Menu")]
    public GameObject[] objekMainMenu;

    [Header("Panel Setting")]
    public GameObject panelSetting;
    public GameObject[] objekPanelSetting;

    [Header("Panel Credit")]
    public GameObject panelCredit;
    public GameObject[] objekPanelCredit;

    [Header("Sub-Panel (Di Dalam Setting)")]
    public GameObject panelAudio;

    [Header("Transisi")]
    public float jedaTransisi = 0.35f;

    // dipanggil pas tombol Setting diklik
    public void BukaPanelSetting()
    {
        StopAllCoroutines();
        StartCoroutine(ProsesBukaPanel(panelSetting));
    }

    // dipanggil pas tombol Logo Panada diklik
    public void BukaPanelCredit()
    {
        StopAllCoroutines();
        StartCoroutine(ProsesBukaPanel(panelCredit));
    }

    // dipanggil pas tombol Audio di dalam Setting diklik
    public void BukaPanelAudio()
    {
        StopAllCoroutines();
        StartCoroutine(ProsesBukaSubPanel(panelAudio, objekPanelSetting));
    }

    // buat pindah scene pakai loading screen
    public void BukaSceneDenganLoading(string namaScene)
    {
        LoadingScreenController lsc = DapatkanLoadingScreenController();
        if (lsc != null)
        {
            lsc.MuatScene(namaScene);
        }
        else
        {
            Debug.LogWarning("LoadingScreenController ga ketemu, langsung load scene aja...");
            UnityEngine.SceneManagement.SceneManager.LoadScene(namaScene);
        }
    }

    // load scene pakai index build
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

    // shortcut buat buka kamar
    public void BukaSceneKamar()
    {
        BukaSceneDenganLoading("Kamar");
    }

    // cari controller loading (bisa jadi nonaktif)
    private LoadingScreenController DapatkanLoadingScreenController()
    {
        if (LoadingScreenController.Instance != null) return LoadingScreenController.Instance;

        LoadingScreenController lsc = FindObjectOfType<LoadingScreenController>(true);
        if (lsc != null)
        {
            if (!lsc.gameObject.activeInHierarchy) lsc.gameObject.SetActive(true);
            return lsc;
        }

        return null;
    }

    // dipanggil pas tombol Kembali diklik

    public void KembaliKeMainMenu()
    {
        StopAllCoroutines();
        
        // Cek apakah kita sedang berada di dalam Sub-Panel (misal: Audio)
        if (panelAudio != null && panelAudio.activeInHierarchy)
        {
            // Tutup Audio, kembali ke Setting
            StartCoroutine(ProsesTutupSubPanel(panelAudio, objekPanelSetting));
        }
        else if (panelSetting != null && panelSetting.activeInHierarchy)
        {
            // Tutup Setting, kembali ke Main Menu
            StartCoroutine(ProsesKembaliKeMainMenu(panelSetting, objekPanelSetting));
        }
        else if (panelCredit != null && panelCredit.activeInHierarchy)
        {
            // Tutup Credit, kembali ke Main Menu
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

        // kumpulin semua objek yang mau dihilangin
        System.Collections.Generic.List<GameObject> daftarKeluar = new System.Collections.Generic.List<GameObject>();
        if (objekMainMenu != null)
        {
            foreach (var o in objekMainMenu) if (o != null && !daftarKeluar.Contains(o)) daftarKeluar.Add(o);
        }
        foreach (Transform anak in transform)
        {
            if (anak.gameObject == panelSetting || anak.gameObject == panelCredit || !anak.gameObject.activeInHierarchy) continue;

            // biarin loading screen
            if (LoadingScreenController.Instance != null)
            {
                if (anak.gameObject == LoadingScreenController.Instance.gameObject || 
                    anak.gameObject == LoadingScreenController.Instance.panelLoading)
                {
                    continue;
                }
            }

            if (!daftarKeluar.Contains(anak.gameObject)) daftarKeluar.Add(anak.gameObject);
        }

        // play animasi keluar
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

        // tunggu selesai
        yield return new WaitForSeconds(jedaTransisi);

        // pastiin mati semua
        foreach (GameObject obj in daftarKeluar)
        {
            if (obj != null) obj.SetActive(false);
        }

        // munculin panel target
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
        System.Collections.Generic.List<GameObject> daftarKeluar = new System.Collections.Generic.List<GameObject>();
        
        // 1. Ambil dari array (jika ada yang manual dimasukkan)
        if (objekTargetPanel != null && objekTargetPanel.Length > 0)
        {
            foreach (GameObject obj in objekTargetPanel)
            {
                if (obj != null && obj.activeInHierarchy && !daftarKeluar.Contains(obj))
                    daftarKeluar.Add(obj);
            }
        }

        // 2. Ambil otomatis dari seluruh anak panel target (ini yang membuat dekorasi baru otomatis tertutup!)
        if (targetPanel != null)
        {
            AnimasiTombolMenu[] animAnak = targetPanel.GetComponentsInChildren<AnimasiTombolMenu>(true);
            foreach (var a in animAnak)
            {
                if (a.gameObject.activeInHierarchy && !daftarKeluar.Contains(a.gameObject))
                    daftarKeluar.Add(a.gameObject);
            }
        }

        // 3. Mainkan animasi keluar untuk semua yang terdaftar
        foreach (GameObject obj in daftarKeluar)
        {
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

        // Mainkan animasi untuk parent panel itu sendiri (jika ada)
        if (targetPanel != null)
        {
            AnimasiTombolMenu animPanel = targetPanel.GetComponent<AnimasiTombolMenu>();
            if (animPanel != null && !daftarKeluar.Contains(targetPanel)) 
            {
                animPanel.JalankanAnimasiOut(() => targetPanel.SetActive(false), true);
            }
        }

        // tunggu beres
        yield return new WaitForSeconds(jedaTransisi);
        if (targetPanel != null) targetPanel.SetActive(false);

        // idupin menu utama lagi
        System.Collections.Generic.List<GameObject> daftarMasuk = new System.Collections.Generic.List<GameObject>();
        if (objekMainMenu != null)
        {
            foreach (var o in objekMainMenu) if (o != null && !daftarMasuk.Contains(o)) daftarMasuk.Add(o);
        }
        foreach (Transform anak in transform)
        {
            if (anak.gameObject == panelSetting || anak.gameObject == panelCredit) continue;
            
            // biarin loading screen
            if (LoadingScreenController.Instance != null)
            {
                if (anak.gameObject == LoadingScreenController.Instance.gameObject || 
                    anak.gameObject == LoadingScreenController.Instance.panelLoading)
                {
                    continue;
                }
            }

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

    IEnumerator ProsesBukaSubPanel(GameObject targetSubPanel, GameObject[] objekYangDisembunyikan)
    {
        if (targetSubPanel == null) yield break;

        // play animasi keluar untuk objek utama setting
        if (objekYangDisembunyikan != null)
        {
            foreach (GameObject obj in objekYangDisembunyikan)
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

        yield return new WaitForSeconds(jedaTransisi);

        // PAKSA matikan semua objek yang harus disembunyikan, tidak peduli animasinya berhasil atau tidak!
        if (objekYangDisembunyikan != null)
        {
            foreach (GameObject obj in objekYangDisembunyikan)
            {
                if (obj != null) obj.SetActive(false);
            }
        }

        // munculin sub-panel target
        targetSubPanel.SetActive(true);
        AnimasiTombolMenu[] animAnak = targetSubPanel.GetComponentsInChildren<AnimasiTombolMenu>(true);
        foreach (var a in animAnak)
        {
            if (!a.gameObject.activeInHierarchy) a.gameObject.SetActive(true);
            a.JalankanUlangAnimasiIn();
        }
    }

    IEnumerator ProsesTutupSubPanel(GameObject targetSubPanel, GameObject[] objekYangDimunculkan)
    {
        // play animasi keluar untuk isi sub-panel
        if (targetSubPanel != null)
        {
            AnimasiTombolMenu[] animAnak = targetSubPanel.GetComponentsInChildren<AnimasiTombolMenu>(true);
            foreach (var a in animAnak)
            {
                if (a.gameObject.activeInHierarchy) a.JalankanAnimasiOut(null, true);
            }
        }

        yield return new WaitForSeconds(jedaTransisi);
        if (targetSubPanel != null) targetSubPanel.SetActive(false);

        // munculin kembali objek utama setting
        if (objekYangDimunculkan != null)
        {
            foreach (GameObject obj in objekYangDimunculkan)
            {
                if (obj == null) continue;
                obj.SetActive(true);
                AnimasiTombolMenu anim = obj.GetComponent<AnimasiTombolMenu>();
                if (anim != null) anim.JalankanUlangAnimasiIn();
            }
        }
    }
}

