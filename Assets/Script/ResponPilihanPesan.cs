using UnityEngine;
using System.Collections;

public class ResponPilihanPesan : MonoBehaviour
{
    [Header("UI Handphone")]
    public GameObject panelPesan;
    public GameObject panelPilihan;

    [Header("Reaksi: Nanti Saja")]
    public GameObject nathanObj;
    public GameObject bubleNameObj;
    public float jedaBubleName = 0.5f;

    [Header("Objek Lingkungan (Opsional)")]
    [Tooltip("Objek HP di atas meja yang ingin dimatikan")]
    public GameObject handphoneMeja;

    [Header("Transisi Blink (Opsional)")]
    [Tooltip("Masukkan BlackScreenPanel di sini jika ingin menggunakan efek kedip saat ganti ke Nathan")]
    public GameObject panelLayarHitam;
    public float durasiTutupMata = 0.25f;
    public float jedaGelap = 0.15f;
    public float durasiBukaMata = 0.4f;

    public void KlikNantiSaja()
    {
        // Tutup opsi
        if (panelPilihan != null) panelPilihan.SetActive(false);

        if (panelLayarHitam != null)
        {
            // Gunakan transisi blink
            StartCoroutine(ProsesBlinkKeNathan());
        }
        else
        {
            // Langsung ganti tanpa blink
            MunculkanNathan();
        }
    }

    IEnumerator ProsesBlinkKeNathan()
    {
        panelLayarHitam.SetActive(true);
        panelLayarHitam.transform.SetAsLastSibling();

        UnityEngine.UI.Image bgImage = panelLayarHitam.GetComponent<UnityEngine.UI.Image>();
        Material blinkMat = null;

        if (bgImage != null && bgImage.material != null && bgImage.material.HasProperty("_Blink"))
        {
            blinkMat = new Material(bgImage.material);
            bgImage.material = blinkMat;
            blinkMat.SetFloat("_Blink", 0f);
        }

        // TUTUP MATA
        float waktu = 0f;
        while (waktu < durasiTutupMata)
        {
            waktu += Time.unscaledDeltaTime;
            if (blinkMat != null) blinkMat.SetFloat("_Blink", Mathf.Clamp01(waktu / durasiTutupMata));
            yield return null;
        }
        if (blinkMat != null) blinkMat.SetFloat("_Blink", 1f);

        // JEDA GELAP & PERGANTIAN ADEGAN
        yield return new WaitForSecondsRealtime(jedaGelap);
        MunculkanNathan();

        // BUKA MATA
        waktu = 0f;
        while (waktu < durasiBukaMata)
        {
            waktu += Time.unscaledDeltaTime;
            if (blinkMat != null) blinkMat.SetFloat("_Blink", Mathf.Clamp01(1f - (waktu / durasiBukaMata)));
            yield return null;
        }
        if (blinkMat != null) blinkMat.SetFloat("_Blink", 0f);

        panelLayarHitam.SetActive(false);
        if (blinkMat != null) Destroy(blinkMat);
    }

    void MunculkanNathan()
    {
        // Matikan HP di meja jika ada
        if (handphoneMeja != null) handphoneMeja.SetActive(false);

        // Munculin char
        if (nathanObj != null) nathanObj.SetActive(true);

        if (bubleNameObj != null) StartCoroutine(ProsesMunculBuble());
    }

    IEnumerator ProsesMunculBuble()
    {
        bubleNameObj.SetActive(false);
        yield return new WaitForSeconds(jedaBubleName);
        bubleNameObj.SetActive(true);
    }

    public void KlikBalasSekarang()
    {
        Debug.Log("Balas Sekarang diklik");
        if (panelPilihan != null) panelPilihan.SetActive(false);
        // TODO: Lanjutin alur balas pesan
    }
}
