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

        // 1. Siapkan layar hitam
        if (layarHitam == null)
        {
            Canvas canvasUtama = GetComponentInParent<Canvas>();
            if (canvasUtama == null) canvasUtama = FindObjectOfType<Canvas>();
            
            layarHitam = new GameObject("LayarHitamKedip");
            layarHitam.transform.SetParent(canvasUtama.transform, false);
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
                blinkMat = new Material(imgHitam.material);
                imgHitam.material = blinkMat;
            }
            else
            {
                cg = layarHitam.GetComponent<CanvasGroup>();
                if (cg == null) cg = layarHitam.AddComponent<CanvasGroup>();
            }
        }

        // 2. Mata tertutup (Fade In)
        float waktu = 0f;
        while (waktu < durasiTutupMata)
        {
            waktu += Time.deltaTime;
            float t = Mathf.Clamp01(waktu / durasiTutupMata);
            
            if (blinkMat != null) blinkMat.SetFloat("_Blink", t); // 0 (buka) ke 1 (tutup)
            else if (cg != null) cg.alpha = t;
            else if (imgHitam != null) imgHitam.color = new Color(0, 0, 0, t);
            
            yield return null;
        }

        // Pastikan mentok tertutup
        if (blinkMat != null) blinkMat.SetFloat("_Blink", 1f);
        else if (cg != null) cg.alpha = 1f;
        else if (imgHitam != null) imgHitam.color = Color.black;

        // Jeda bentar pas mata nutup
        yield return new WaitForSeconds(0.15f);

        // 3. BUKA PANEL PESAN! (saat layar gelap total)
        if (panelPesan != null)
        {
            panelPesan.SetActive(true);
        }
        // Sembunyikan notif ini
        gameObject.SetActive(false);

        // 4. Mata terbuka lagi (Fade Out)
        waktu = 0f;
        while (waktu < durasiBukaMata)
        {
            waktu += Time.deltaTime;
            float t = Mathf.Clamp01(1f - (waktu / durasiBukaMata));
            
            if (blinkMat != null) blinkMat.SetFloat("_Blink", t); // 1 (tutup) ke 0 (buka)
            else if (cg != null) cg.alpha = t;
            else if (imgHitam != null) imgHitam.color = new Color(0, 0, 0, t);
            
            yield return null;
        }

        // Pastikan mentok terbuka
        if (blinkMat != null) blinkMat.SetFloat("_Blink", 0f);
        else if (cg != null) cg.alpha = 0f;
        else if (imgHitam != null) imgHitam.color = new Color(0, 0, 0, 0);

        // 5. Bersih-bersih
        if (buatLayarDadakan)
        {
            Destroy(layarHitam);
        }
        else
        {
            layarHitam.SetActive(false);
        }
    }
}
