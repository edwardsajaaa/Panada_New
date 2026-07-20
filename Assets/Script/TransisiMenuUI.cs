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

    [Header("Transisi")]
    public float jedaTransisi = 0.35f;

    // dipanggil pas tombol Setting diklik
    public void BukaPanelSetting()
    {
        StopAllCoroutines();
        StartCoroutine(ProsesBukaPanel(panelSetting));
    }

    // dipanggil pas Logo Panada diklik
    public void BukaPanelCredit()
    {
        StopAllCoroutines();
        StartCoroutine(ProsesBukaPanel(panelCredit));
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
        // play animasi keluar panel target
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
}
