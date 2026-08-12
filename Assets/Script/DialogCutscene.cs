using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class BarisDialogCutscene
{
    [TextArea(3, 5)]
    public string kalimat;
    
    [Tooltip("Kecepatan ketik khusus untuk baris ini. Biarkan 0 jika ingin menggunakan kecepatan bawaan.")]
    public float kecepatanKetikKhusus = 0f;
    
    [Space(5)]
    [Tooltip("Aksi atau event yang dipicu berbarengan saat teks ini mulai diketik. (Misal: Menyalakan panel awan)")]
    public UnityEvent aksiSaatKalimatMulai;
}

public class DialogCutscene : MonoBehaviour
{
    [Header("Referensi UI")]
    public GameObject panelUtamaDialog;
    public TextMeshProUGUI teksNamaKarakter;
    public TextMeshProUGUI teksIsiDialog;
    
    [Header("Pengaturan Dialog")]
    public string namaKarakter = "Nathan";
    public float kecepatanKetikBawaan = 0.03f;
    [Tooltip("Waktu tunggu sebelum dialog pertama dimulai")]
    public float jedaAwal = 1f;

    [Header("Data Percakapan")]
    public BarisDialogCutscene[] percakapan;

    [Header("Transisi Selesai (Opsional)")]
    [Tooltip("Panel hitam untuk efek fade out di akhir dialog")]
    public GameObject panelLayarHitam;
    public float durasiFadeOut = 1.5f;

    [Header("Event Akhir")]
    [Tooltip("Dijalankan setelah seluruh dialog selesai (dan setelah layar memudar jika ada)")]
    public UnityEvent saatSemuaSelesai;

    private int indeks = 0;
    private bool sedangNgetik = false;
    private bool aktif = false;
    private Coroutine prosesKetik;
    private Material blinkMat;

    void Start()
    {
        // Matikan panel dialog di awal
        if (panelUtamaDialog != null) panelUtamaDialog.SetActive(false);
        if (teksNamaKarakter != null) teksNamaKarakter.text = "";
        if (teksIsiDialog != null) teksIsiDialog.text = "";

        // Siapkan layar hitam (transparan di awal)
        if (panelLayarHitam != null)
        {
            Image bgImage = panelLayarHitam.GetComponent<Image>();
            if (bgImage != null)
            {
                Color c = bgImage.color;
                c.a = 0f; // Transparan di awal
                bgImage.color = c;
            }
            panelLayarHitam.SetActive(false);
        }

        // Mulai otomatis
        StartCoroutine(MulaiDialogBerjeda());
    }

    IEnumerator MulaiDialogBerjeda()
    {
        yield return new WaitForSeconds(jedaAwal);
        
        if (panelUtamaDialog != null) panelUtamaDialog.SetActive(true);
        if (teksNamaKarakter != null) teksNamaKarakter.text = namaKarakter;
        
        aktif = true;
        MulaiKetik();
    }

    void Update()
    {
        if (!aktif) return;

        // Klik kiri atau spasi untuk lanjut
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (sedangNgetik)
            {
                // Jika sedang mengetik, paksa langsung selesai
                if (prosesKetik != null) StopCoroutine(prosesKetik);
                teksIsiDialog.text = percakapan[indeks].kalimat;
                sedangNgetik = false;
            }
            else
            {
                // Jika sudah selesai mengetik, lanjut ke kalimat berikutnya
                LanjutDialog();
            }
        }
    }

    void MulaiKetik()
    {
        if (indeks >= percakapan.Length)
        {
            AkhiriDialog();
            return;
        }

        // Panggil event khusus untuk kalimat ini (jika ada)
        percakapan[indeks].aksiSaatKalimatMulai?.Invoke();

        prosesKetik = StartCoroutine(KetikTeks(percakapan[indeks]));
    }

    IEnumerator KetikTeks(BarisDialogCutscene baris)
    {
        sedangNgetik = true;
        teksIsiDialog.text = "";
        
        float kecepatan = baris.kecepatanKetikKhusus > 0f ? baris.kecepatanKetikKhusus : kecepatanKetikBawaan;

        foreach (char huruf in baris.kalimat.ToCharArray())
        {
            teksIsiDialog.text += huruf;
            yield return new WaitForSeconds(kecepatan);
        }

        sedangNgetik = false;
    }

    void LanjutDialog()
    {
        indeks++;
        if (indeks < percakapan.Length)
        {
            MulaiKetik();
        }
        else
        {
            AkhiriDialog();
        }
    }

    void AkhiriDialog()
    {
        aktif = false;
        if (panelUtamaDialog != null) panelUtamaDialog.SetActive(false);

        if (panelLayarHitam != null)
        {
            StartCoroutine(ProsesFadeOutLaluSelesai());
        }
        else
        {
            saatSemuaSelesai?.Invoke();
        }
    }

    IEnumerator ProsesFadeOutLaluSelesai()
    {
        panelLayarHitam.SetActive(true);
        
        Image bgImage = panelLayarHitam.GetComponent<Image>();
        if (bgImage != null)
        {
            float waktu = 0f;
            Color warnaAsli = bgImage.color;
            warnaAsli.a = 0f;
            
            while (waktu < durasiFadeOut)
            {
                waktu += Time.deltaTime;
                warnaAsli.a = Mathf.Clamp01(waktu / durasiFadeOut);
                bgImage.color = warnaAsli;
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(durasiFadeOut);
        }

        saatSemuaSelesai?.Invoke();
    }
}
