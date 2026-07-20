using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BukaPanelPesan : MonoBehaviour
{
    [Header("Panel Target")]
    public GameObject panelPesan;

    [Header("Animasi Kedip")]
    public bool gunakanKedip = true;
    public float durasiTutupMata = 0.3f;
    public float durasiBukaMata = 0.4f;
    [Tooltip("Masukkan layar hitam (opsional). Jika ada material _Blink, akan pakai efek kelopak mata!")]
    public GameObject panelLayarHitam;

    void Start()
    {
        if (panelPesan != null)
        {
            panelPesan.SetActive(false);
        }

        Button tombol = GetComponent<Button>();
        if (tombol != null)
        {
            tombol.onClick.AddListener(TampilkanPesan);
        }
    }

    public void TampilkanPesan()
    {
        if (gunakanKedip)
        {
            StartCoroutine(ProsesAnimasiKedip());
        }
        else
        {
            BukaLangsung();
        }
    }

    void BukaLangsung()
    {
        if (panelPesan != null) panelPesan.SetActive(true);
        gameObject.SetActive(false); 
    }

    IEnumerator ProsesAnimasiKedip()
    {
        GameObject layarHitam = panelLayarHitam;
        Image imgHitam = null;
        Material blinkMat = null;
        CanvasGroup cg = null;
        bool buatLayarDadakan = false;

        Canvas canvasUtama = GetComponentInParent<Canvas>();
        if (canvasUtama == null) canvasUtama = FindObjectOfType<Canvas>();

        // Cek apakah layarHitam adalah prefab dari Project (bukan dari Hierarchy scene)
        if (layarHitam != null && string.IsNullOrEmpty(layarHitam.scene.name))
        {
            layarHitam = Instantiate(layarHitam);
            if (canvasUtama != null) layarHitam.transform.SetParent(canvasUtama.transform, false);
            buatLayarDadakan = true; // agar nanti dihapus
        }

        // 1. Siapkan layar hitam
        if (layarHitam == null)
        {
            layarHitam = new GameObject("LayarHitamKedip");
            if (canvasUtama != null) layarHitam.transform.SetParent(canvasUtama.transform, false);
            layarHitam.transform.SetAsLastSibling();

            imgHitam = layarHitam.AddComponent<Image>();
            imgHitam.color = new Color(0, 0, 0, 0); 
            
            RectTransform rect = imgHitam.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            
            buatLayarDadakan = true;
        }
        else
        {
            layarHitam.SetActive(true);
            layarHitam.transform.SetAsLastSibling();
            imgHitam = layarHitam.GetComponent<Image>();
            
            if (imgHitam != null && imgHitam.material != null && imgHitam.material.HasProperty("_Blink"))
            {
                // Paksa alpha color menjadi 1 agar material shader tetap terlihat walau image-nya diset transparan
                Color c = imgHitam.color;
                c.a = 1f;
                imgHitam.color = c;

                blinkMat = new Material(imgHitam.material);
                imgHitam.material = blinkMat;
            }
        }

        // Matikan interaksi dan sembunyikan visual notif supaya tidak diklik 2x
        Image imgTombol = GetComponent<Image>();
        if (imgTombol != null) imgTombol.enabled = false;
        Button btnTombol = GetComponent<Button>();
        if (btnTombol != null) btnTombol.interactable = false;

        // 2. Mata tertutup (Fade In)
        float waktu = 0f;
        while (waktu < durasiTutupMata)
        {
            waktu += Time.deltaTime;
            float t = Mathf.Clamp01(waktu / durasiTutupMata);
            
            if (blinkMat != null) blinkMat.SetFloat("_Blink", t); // 0 (buka) ke 1 (tutup)
            else if (imgHitam != null)
            {
                Color c = imgHitam.color;
                c.a = t;
                imgHitam.color = c;
            }
            
            yield return null;
        }

        // Pastikan mentok tertutup
        if (blinkMat != null) blinkMat.SetFloat("_Blink", 1f);
        else if (imgHitam != null)
        {
            Color c = imgHitam.color;
            c.a = 1f;
            imgHitam.color = c;
        }

        // Jeda bentar pas mata nutup
        yield return new WaitForSeconds(0.15f);

        // (Panel Pesan DITUNDA agar baru aktif setelah mata terbuka penuh)

        // 4. Mata terbuka lagi (Fade Out)
        waktu = 0f;
        while (waktu < durasiBukaMata)
        {
            waktu += Time.deltaTime;
            float t = Mathf.Clamp01(1f - (waktu / durasiBukaMata));
            
            if (blinkMat != null) blinkMat.SetFloat("_Blink", t); // 1 (tutup) ke 0 (buka)
            else if (imgHitam != null)
            {
                Color c = imgHitam.color;
                c.a = t;
                imgHitam.color = c;
            }
            
            yield return null;
        }

        // Pastikan mentok terbuka
        if (blinkMat != null) blinkMat.SetFloat("_Blink", 0f);
        else if (imgHitam != null)
        {
            Color c = imgHitam.color;
            c.a = 0f;
            imgHitam.color = c;
        }

        // 5. BUKA PANEL PESAN! (saat layar sudah terang sepenuhnya)
        // Hal ini akan memicu animasi tangan "SlideDariBawah" muncul tepat setelah mata terbuka!
        if (panelPesan != null)
        {
            panelPesan.SetActive(true);
        }

        // 5. Bersih-bersih
        if (buatLayarDadakan)
        {
            Destroy(layarHitam);
        }
        else
        {
            layarHitam.SetActive(false);
        }
        
        // Akhirnya, matikan diri sendiri setelah semua animasi selesai
        gameObject.SetActive(false);
    }
}
