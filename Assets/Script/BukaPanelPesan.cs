using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BukaPanelPesan : MonoBehaviour
{
    [Header("Panel Target")]
    public GameObject panelPesan;

    [Header("Animasi Kedip")]
    public float durasiTutupMata = 0.25f;
    public float jedaGelap = 0.15f;
    public float durasiBukaMata = 0.4f;

    [Tooltip("Masukkan BlackScreenPanel yang sudah ada di scene (panel Image dengan material EyeBlinkMat).")]
    public GameObject panelLayarHitam;

    void Start()
    {
        if (panelPesan != null)
            panelPesan.SetActive(false);

        // Pastikan layar hitam mati di awal agar tidak menghalangi
        if (panelLayarHitam != null)
            panelLayarHitam.SetActive(false);

        Button tombol = GetComponent<Button>();
        if (tombol != null)
            tombol.onClick.AddListener(OnKlik);
    }

    void OnKlik()
    {
        // Jalankan coroutine di object ini sendiri
        StartCoroutine(ProsesKedip());
    }

    IEnumerator ProsesKedip()
    {
        // 1. Sembunyikan visual dan interaksi tombol notif agar tidak diklik 2x
        Image imgTombol = GetComponent<Image>();
        if (imgTombol != null) imgTombol.enabled = false;
        
        Button btnTombol = GetComponent<Button>();
        if (btnTombol != null) btnTombol.enabled = false;

        // 2. Nyalakan BlackScreenPanel
        if (panelLayarHitam == null) yield break;
        panelLayarHitam.SetActive(true);
        panelLayarHitam.transform.SetAsLastSibling();

        // 3. Ambil material blink dari Image yang sudah terpasang di BlackScreenPanel
        Image bgImage = panelLayarHitam.GetComponent<Image>();
        Material blinkMat = null;

        if (bgImage != null && bgImage.material != null && bgImage.material.HasProperty("_Blink"))
        {
            blinkMat = new Material(bgImage.material);
            bgImage.material = blinkMat;
            blinkMat.SetFloat("_Blink", 0f); // mulai dari mata terbuka
        }

        // 4. TUTUP MATA (0 → 1)
        float waktu = 0f;
        while (waktu < durasiTutupMata)
        {
            waktu += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(waktu / durasiTutupMata);

            if (blinkMat != null)
                blinkMat.SetFloat("_Blink", t);

            yield return null;
        }
        if (blinkMat != null) blinkMat.SetFloat("_Blink", 1f);

        // 5. JEDA GELAP
        yield return new WaitForSecondsRealtime(jedaGelap);

        // 5.5. AKTIFKAN PANEL BARU (TANGAN MELUNCUR) SAAT LAYAR MASIH GELAP
        // Sehingga ketika mata mulai terbuka, panel baru sudah dalam proses transisi masuk
        if (panelPesan != null)
            panelPesan.SetActive(true);

        // 6. BUKA MATA (1 → 0)
        waktu = 0f;
        while (waktu < durasiBukaMata)
        {
            waktu += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(1f - (waktu / durasiBukaMata));

            if (blinkMat != null)
                blinkMat.SetFloat("_Blink", t);

            yield return null;
        }
        if (blinkMat != null) blinkMat.SetFloat("_Blink", 0f);

        // 7. Matikan BlackScreenPanel
        panelLayarHitam.SetActive(false);

        // 8. Bersihkan material instance
        if (blinkMat != null) Destroy(blinkMat);

        // 10. Matikan tombol ini sepenuhnya setelah semua proses selesai
        gameObject.SetActive(false);
    }
}


