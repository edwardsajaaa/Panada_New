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
    public GameObject panelKontrol;

    [Header("Transisi")]
    public float jedaTransisi = 0.35f;

    private bool creditDibukaDariSetting = false;

    // dipanggil pas tombol Setting diklik
    public void BukaPanelSetting()
    {
        StopAllCoroutines();
        StartCoroutine(ProsesBukaPanel(panelSetting));
    }

    // dipanggil pas tombol Logo Panada diklik (Di Main Menu)
    public void BukaPanelCreditDariMainMenu()
    {
        creditDibukaDariSetting = false;
        StopAllCoroutines();
        StartCoroutine(ProsesBukaCreditDariMainMenuLuar());
    }

    // dipanggil pas tombol Kredit di dalam Setting diklik
    public void BukaPanelCreditDariSetting()
    {
        creditDibukaDariSetting = true;
        StopAllCoroutines();
        StartCoroutine(ProsesBukaSubPanel(panelCredit, objekPanelSetting));
    }

    // dipanggil pas tombol Audio di dalam Setting diklik
    public void BukaPanelAudio()
    {
        StopAllCoroutines();
        StartCoroutine(ProsesBukaSubPanel(panelAudio, objekPanelSetting));
    }

    // dipanggil pas tombol Kontrol di dalam Setting diklik
    public void BukaPanelKontrol()
    {
        StopAllCoroutines();
        StartCoroutine(ProsesBukaSubPanel(panelKontrol, objekPanelSetting));
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
        
        // Cek apakah kita sedang berada di dalam Sub-Panel (misal: Audio atau Kontrol)
        if (panelAudio != null && panelAudio.activeInHierarchy)
        {
            // Tutup Audio, kembali ke Setting
            StartCoroutine(ProsesTutupSubPanel(panelAudio, objekPanelSetting));
        }
        else if (panelKontrol != null && panelKontrol.activeInHierarchy)
        {
            // Tutup Kontrol, kembali ke Setting
            StartCoroutine(ProsesTutupSubPanel(panelKontrol, objekPanelSetting));
        }
        else if (panelCredit != null && panelCredit.activeInHierarchy)
        {
            if (creditDibukaDariSetting)
            {
                // Tutup Credit, kembali ke Setting (karena dibuka dari Setting)
                StartCoroutine(ProsesTutupSubPanel(panelCredit, objekPanelSetting));
            }
            else
            {
                // Tutup Credit, kembali ke Main Menu (karena dibuka dari Logo Panada)
                StartCoroutine(ProsesTutupCreditKeMainMenuLuar());
            }
        }
        else if (panelSetting != null && panelSetting.activeInHierarchy)
        {
            // Tutup Setting, kembali ke Main Menu
            StartCoroutine(ProsesKembaliKeMainMenu(panelSetting, objekPanelSetting));
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

        // PAKSA sub-panel mati jika yang mau dibuka adalah Setting Panel
        // Ini untuk mencegah bug di mana sub-panel diam-diam masih menyala di belakang layar
        if (targetPanel == panelSetting)
        {
            if (panelAudio != null) panelAudio.SetActive(false);
            if (panelKontrol != null) panelKontrol.SetActive(false);
            if (panelCredit != null) panelCredit.SetActive(false);
        }

        // munculin panel target
        targetPanel.SetActive(true);
        AnimasiTombolMenu[] animAnak = targetPanel.GetComponentsInChildren<AnimasiTombolMenu>(true);
        foreach (var a in animAnak)
        {
            // Jangan hidupkan sub-panel dan anak-anaknya secara tidak sengaja jika targetnya bukan mereka
            if (panelAudio != null && targetPanel != panelAudio && a.transform.IsChildOf(panelAudio.transform)) continue;
            if (panelKontrol != null && targetPanel != panelKontrol && a.transform.IsChildOf(panelKontrol.transform)) continue;
            if (panelCredit != null && targetPanel != panelCredit && a.transform.IsChildOf(panelCredit.transform)) continue;

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

    IEnumerator ProsesBukaCreditDariMainMenuLuar()
    {
        // 1. Hilangkan objek Main Menu
        System.Collections.Generic.List<GameObject> daftarKeluar = new System.Collections.Generic.List<GameObject>();
        if (objekMainMenu != null)
        {
            foreach (var o in objekMainMenu) if (o != null && !daftarKeluar.Contains(o)) daftarKeluar.Add(o);
        }

        foreach (GameObject obj in daftarKeluar)
        {
            if (obj == null || !obj.activeInHierarchy) continue;
            AnimasiTombolMenu anim = obj.GetComponent<AnimasiTombolMenu>();
            if (anim != null) anim.JalankanAnimasiOut(null, true);
        }

        yield return new WaitForSeconds(jedaTransisi);

        foreach (GameObject obj in daftarKeluar)
        {
            if (obj != null) obj.SetActive(false);
        }

        // 2. Munculkan Setting Panel, TAPI sembunyikan sticky notes (objekPanelSetting)
        if (panelSetting != null)
        {
            panelSetting.SetActive(true);
            
            foreach (Transform anak in panelSetting.transform)
            {
                if (anak.gameObject == panelAudio || anak.gameObject == panelKontrol || anak.gameObject == panelCredit) 
                    continue;

                // Cek apakah anak ini bagian dari objekPanelSetting
                bool harusDisembunyikan = false;
                if (objekPanelSetting != null)
                {
                    foreach (var obs in objekPanelSetting)
                    {
                        if (obs == anak.gameObject) { harusDisembunyikan = true; break; }
                    }
                }

                if (harusDisembunyikan)
                {
                    anak.gameObject.SetActive(false);
                }
                else
                {
                    anak.gameObject.SetActive(true);
                    AnimasiTombolMenu[] anims = anak.GetComponentsInChildren<AnimasiTombolMenu>(true);
                    foreach (var anim in anims)
                    {
                        if (!anim.gameObject.activeInHierarchy) anim.gameObject.SetActive(true);
                        anim.JalankanUlangAnimasiIn();
                    }
                }
            }
        }

        // 3. Munculkan Credit Panel
        if (panelCredit != null)
        {
            panelCredit.SetActive(true);
            AnimasiTombolMenu[] animCredit = panelCredit.GetComponentsInChildren<AnimasiTombolMenu>(true);
            foreach (var a in animCredit)
            {
                if (!a.gameObject.activeInHierarchy) a.gameObject.SetActive(true);
                a.JalankanUlangAnimasiIn();
            }
        }
    }

    IEnumerator ProsesTutupCreditKeMainMenuLuar()
    {
        // 1. Animasikan keluar Credit Panel dan sisa elemen Setting Panel
        System.Collections.Generic.List<GameObject> daftarKeluar = new System.Collections.Generic.List<GameObject>();
        
        if (panelCredit != null)
        {
            AnimasiTombolMenu[] animCredit = panelCredit.GetComponentsInChildren<AnimasiTombolMenu>(true);
            foreach (var a in animCredit)
            {
                if (a.gameObject.activeInHierarchy && !daftarKeluar.Contains(a.gameObject))
                    daftarKeluar.Add(a.gameObject);
            }
        }

        if (panelSetting != null)
        {
            AnimasiTombolMenu[] animAnak = panelSetting.GetComponentsInChildren<AnimasiTombolMenu>(true);
            foreach (var a in animAnak)
            {
                if (panelCredit != null && a.transform.IsChildOf(panelCredit.transform)) continue;
                
                if (a.gameObject.activeInHierarchy && !daftarKeluar.Contains(a.gameObject))
                    daftarKeluar.Add(a.gameObject);
            }
        }

        foreach (GameObject obj in daftarKeluar)
        {
            AnimasiTombolMenu anim = obj.GetComponent<AnimasiTombolMenu>();
            if (anim != null) anim.JalankanAnimasiOut(null, true);
        }

        yield return new WaitForSeconds(jedaTransisi);

        foreach (GameObject obj in daftarKeluar)
        {
            if (obj != null) obj.SetActive(false);
        }
        if (panelSetting != null) panelSetting.SetActive(false);
        if (panelCredit != null) panelCredit.SetActive(false); // pastikan credit panel juga ikut mati

        // 2. Munculkan Main Menu lagi
        if (objekMainMenu != null)
        {
            foreach (var o in objekMainMenu)
            {
                if (o != null)
                {
                    o.SetActive(true);
                    AnimasiTombolMenu anim = o.GetComponent<AnimasiTombolMenu>();
                    if (anim != null) anim.JalankanUlangAnimasiIn();
                }
            }
        }
    }
}

