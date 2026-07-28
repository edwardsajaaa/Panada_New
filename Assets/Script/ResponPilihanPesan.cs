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
    [Tooltip("Pesan yang muncul jika pemain LANGSUNG membalas")]
    public GameObject[] pesanBalasLangsung;
    [Tooltip("Pesan yang muncul jika pemain membalas SETELAH menekan Nanti Saja")]
    public GameObject[] pesanBalasSetelahNanti;
    [Tooltip("Scroll Rect HP agar bisa otomatis geser ke bawah")]
    public UnityEngine.UI.ScrollRect scrollRectPesan;

    [Header("Transisi Kembali Ke Nathan (Setelah Balas)")]
    [Tooltip("Centang jika ingin layar berkedip dan kembali ke Nathan setelah semua pesan terkirim")]
    public bool kembaliKeNathanSetelahBalas = false;
    public float jedaSebelumKembali = 2f;
    [Tooltip("Objek Nathan yang akan diaktifkan (bisa pakai objek yang sama atau beda)")]
    public GameObject nathanObjLanjutan;
    [Tooltip("Objek Dialog (Buble Name) Lanjutan yang berisi teks baru")]
    public GameObject bubleNameObjLanjutan;

    // Variabel statis untuk merekam riwayat pilihan pemain di scene ini
    public static bool pernahPilihNantiSaja = false;

    void Awake()
    {
        // Reset status setiap kali scene dimuat
        pernahPilihNantiSaja = false;
    }

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
        // Catat bahwa pemain pernah menekan Nanti Saja
        pernahPilihNantiSaja = true;

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
            StartCoroutine(ProsesBlinkKeNathan(nathanObj, bubleNameObj));
        }
        else
        {
            // Langsung ganti tanpa blink
            MunculkanNathan(nathanObj, bubleNameObj);
        }
    }

    [Tooltip("Jeda waktu (detik) antar kemunculan pesan balasan")]
    public float jedaAntarPesanBalasan = 2f;

    public void KlikBalasSekarang()
    {
        // 1. Matikan panel opsi agar tombol menghilang
        if (panelPilihan != null) panelPilihan.SetActive(false);

        // 2. Mulai coroutine untuk memunculkan pesan satu per satu
        StartCoroutine(ProsesPesanSatuSatu());
    }

    private IEnumerator ProsesPesanSatuSatu()
    {
        GameObject[] pesanYangDigunakan = pernahPilihNantiSaja ? pesanBalasSetelahNanti : pesanBalasLangsung;

        if (pesanYangDigunakan != null)
        {
            foreach (var pesan in pesanYangDigunakan)
            {
                if (pesan != null) 
                {
                    pesan.SetActive(true);
                    
                    // Otomatis scroll tiap kali ada pesan baru yang muncul
                    if (scrollRectPesan != null)
                    {
                        StartCoroutine(ScrollKeBawah());
                    }

                    // Jeda sebelum pesan berikutnya muncul
                    yield return new WaitForSeconds(jedaAntarPesanBalasan);
                }
            }
        }

        // 3. Kembali ke Nathan jika diaktifkan
        if (kembaliKeNathanSetelahBalas)
        {
            yield return new WaitForSeconds(jedaSebelumKembali);
            
            if (panelLayarHitam != null)
            {
                StartCoroutine(ProsesBlinkKeNathan(nathanObjLanjutan, bubleNameObjLanjutan));
            }
            else
            {
                MunculkanNathan(nathanObjLanjutan, bubleNameObjLanjutan);
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

    IEnumerator ProsesBlinkKeNathan(GameObject targetNathan, GameObject targetBuble)
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

        // MATIKAN UI TANGAN/HP SAAT LAYAR SEDANG GELAP
        if (panelPesan != null) panelPesan.SetActive(false);

        MunculkanNathan(targetNathan, targetBuble);

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

    void MunculkanNathan(GameObject targetNathan, GameObject targetBuble)
    {
        // Matikan UI Tangan/HP
        if (panelPesan != null) panelPesan.SetActive(false);

        // Matikan HP di meja jika ada
        if (handphoneMeja != null) handphoneMeja.SetActive(false);

        // Munculin char
        if (targetNathan != null) targetNathan.SetActive(true);

        if (targetBuble != null) 
        {
            // Set flag agar SistemDialogKamar menggunakan percakapan lanjutan
            SistemDialogKamar dialogSys = targetBuble.GetComponent<SistemDialogKamar>();
            if (dialogSys != null)
            {
                dialogSys.gunakanLanjutan = true;
            }

            StartCoroutine(ProsesMunculBuble(targetBuble));
        }
    }

    IEnumerator ProsesMunculBuble(GameObject targetBuble)
    {
        targetBuble.SetActive(false);
        yield return new WaitForSeconds(jedaBubleName);
        targetBuble.SetActive(true);
    }

}
