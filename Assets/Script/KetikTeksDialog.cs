using UnityEngine;
using TMPro;
using System.Collections;

// Membuat format data baru agar setiap kalimat bisa memiliki namanya sendiri-sendiri
[System.Serializable]
public class BarisCeritaDialog
{
    [Tooltip("Nama karakter yang sedang berbicara")]
    public string namaKarakter = "Penyiar TV";
    
    [TextArea(2, 4)]
    [Tooltip("Kalimat yang diucapkan oleh karakter tersebut")]
    public string kalimat;
}

public class KetikTeksDialog : MonoBehaviour
{
    [Header("Referensi UI")]
    [Tooltip("Masukkan Text (TMP) yang akan dijadikan tempat teks dialog berjalan")]
    public TextMeshProUGUI teksDialog;
    
    [Tooltip("Masukkan Text (TMP) untuk nama karakter (Opsional)")]
    public TextMeshProUGUI teksNama;

    [Header("Pengaturan Animasi")]
    [Tooltip("Samakan dengan angka Jeda Sebelum Text agar teks mulai mengetik tepat saat gelembung muncul")]
    public float waktuTungguMulai = 3.5f;
    
    [Tooltip("Kecepatan mesin tik (semakin kecil semakin cepat)")]
    public float kecepatanKetik = 0.04f;

    [Header("Isi Cerita")]
    [Tooltip("Daftar percakapan Anda. Tekan tombol + untuk menambah dialog, dan Anda bisa mengganti nama karakter di tiap baris!")]
    public BarisCeritaDialog[] percakapan;

    private int indeksKalimat = 0;
    private bool sedangNgetik = false;
    private bool sudahMulai = false;

    void OnEnable()
    {
        if (percakapan.Length > 0 && teksDialog != null)
        {
            teksDialog.text = ""; // Kosongkan teks di awal
            if (teksNama != null) teksNama.text = ""; // Kosongkan nama di awal
            sudahMulai = false;
            StartCoroutine(TungguDanMulai());
        }
    }

    IEnumerator TungguDanMulai()
    {
        yield return new WaitForSeconds(waktuTungguMulai);
        sudahMulai = true;
        MulaiDialog(0);
    }

    void Update()
    {
        // Hanya bisa lanjut atau skip jika dialog sudah mulai muncul
        if (!sudahMulai) return;

        // Jika pemain mengeklik kiri (mouse) atau menekan Spasi
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (sedangNgetik)
            {
                // Skip animasi ketik
                StopAllCoroutines();
                teksDialog.text = percakapan[indeksKalimat].kalimat;
                sedangNgetik = false;
            }
            else
            {
                // Lanjut ke kalimat berikutnya
                LanjutDialog();
            }
        }
    }

    public void MulaiDialog(int index)
    {
        indeksKalimat = index;
        StartCoroutine(KetikKalimat());
    }

    void LanjutDialog()
    {
        if (indeksKalimat < percakapan.Length - 1)
        {
            indeksKalimat++;
            StartCoroutine(KetikKalimat());
        }
        else
        {
            Debug.Log("Dialog sudah habis. Pemain bisa menutup panel menggunakan tombol kembali/ESC.");
        }
    }

    IEnumerator KetikKalimat()
    {
        sedangNgetik = true;
        teksDialog.text = "";
        
        // Ambil data (nama & kalimat) dari baris saat ini
        BarisCeritaDialog dataSaatIni = percakapan[indeksKalimat];
        
        // Perbarui nama karakter di UI jika ada
        if (teksNama != null)
        {
            teksNama.text = dataSaatIni.namaKarakter;
        }
        
        // Memunculkan teks huruf demi huruf (Efek Mesin Tik)
        foreach (char huruf in dataSaatIni.kalimat.ToCharArray())
        {
            teksDialog.text += huruf;
            yield return new WaitForSeconds(kecepatanKetik);
        }
        
        sedangNgetik = false;
    }
}
