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

    [Header("Reaksi: Balas Sekarang")]
    [Tooltip("Objek balasan Nathan (warna biru) yang akan dimunculkan")]
    public GameObject pesanBalasanNathan;
    [Tooltip("Scroll Rect HP agar bisa otomatis geser ke bawah")]
    public UnityEngine.UI.ScrollRect scrollRectPesan;

    [Header("Perubahan HP (Disiapkan untuk dibuka lagi nanti)")]
    [Tooltip("Pesan baru yang akan langsung muncul saat HP dibuka lagi")]
    public GameObject[] pesanBaru;
    [Tooltip("Tombol yang akan dimatikan permanen (misal: Nanti Saja)")]
    public GameObject tombolDimatikan;
    [Tooltip("Tombol yang akan digeser ke tengah (misal: Balas Sekarang)")]
    public RectTransform tombolDitengah;
    public Vector2 posisiTengah = new Vector2(-16f, -129.359f);

    public void KlikNantiSaja()
    {
        // Siapkan perubahan UI HP di belakang layar untuk nanti
        if (pesanBaru != null)
        {
            foreach (var pesan in pesanBaru)
            {
                if (pesan != null) pesan.SetActive(true);
            }
        }
        if (tombolDimatikan != null) tombolDimatikan.SetActive(false);
        if (tombolDitengah != null) 
        {
            tombolDitengah.anchoredPosition = posisiTengah;
            
            // Perbarui posisi awal di script AnimasiTombolMenu agar tidak ter-reset
            AnimasiTombolMenu anim = tombolDitengah.GetComponent<AnimasiTombolMenu>();
            if (anim != null)
            {
                anim.PerbaruiPosisiAwal(posisiTengah);
            }
        }

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

    public void KlikBalasSekarang()
    {
        // 1. Matikan panel opsi agar tombol menghilang
        if (panelPilihan != null) panelPilihan.SetActive(false);

        // 2. Munculkan pesan balasan Nathan
        if (pesanBalasanNathan != null)
        {
            pesanBalasanNathan.SetActive(true);
            
            // 3. Scroll ke bawah jika ScrollRect diisi
            if (scrollRectPesan != null)
            {
                StartCoroutine(ScrollKeBawah());
            }
        }
    }

    private IEnumerator ScrollKeBawah()
    {
        // Tunggu 1 frame agar Layout Group dan Content Size Fitter merefresh ukurannya
        yield return new WaitForEndOfFrame();
        
        // Paksa scroll menempel di paling bawah (0)
        if (scrollRectPesan != null)
        {
            scrollRectPesan.verticalNormalizedPosition = 0f;
        }
    }

    IEnumerator ProsesBlinkKeNathan()
    {
        panelLayarHitam.SetActive(true);
        panelLayarHitam.transform.SetAsLastSibling();

        UnityEngine.UI.Image bgImage = panelLayarHitam.GetComponent<UnityEngine.UI.Image>();
        Material originalMat = null;
        Material blinkMat = null;

        if (bgImage != null && bgImage.material != null && bgImage.material.HasProperty("_Blink"))
        {
            originalMat = bgImage.material;
            blinkMat = new Material(originalMat);
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
        if (bgImage != null && originalMat != null)
        {
            bgImage.material = originalMat;
        }
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
