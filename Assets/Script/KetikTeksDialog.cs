using UnityEngine;
using TMPro;
using System.Collections;

public class KetikTeksDialog : MonoBehaviour
{
    [Header("Referensi")]
    [Tooltip("Masukkan Text (TMP) yang akan dijadikan tempat teks dialog berjalan")]
    public TextMeshProUGUI teksDialog;

    [Header("Pengaturan Dialog")]
    [Tooltip("Samakan dengan angka Jeda Sebelum Text di AnimasiPanelFoto agar teks mulai mengetik tepat saat gelembung muncul")]
    public float waktuTungguMulai = 3.5f;
    
    [Tooltip("Kecepatan mesin tik (semakin kecil semakin cepat)")]
    public float kecepatanKetik = 0.04f;

    [TextArea(2, 5)]
    [Tooltip("Tuliskan dialog-dialog Anda di sini. Tekan tombol + untuk menambah kalimat baru.")]
    public string[] daftarKalimat;

    private int indeksKalimat = 0;
    private bool sedangNgetik = false;
    private bool sudahMulai = false;

    void OnEnable()
    {
        if (daftarKalimat.Length > 0 && teksDialog != null)
        {
            teksDialog.text = ""; // Kosongkan teks di awal
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
                // Jika masih ngetik, langsung tampilkan semua teks secara utuh (Skip animasi ketik)
                StopAllCoroutines();
                teksDialog.text = daftarKalimat[indeksKalimat];
                sedangNgetik = false;
            }
            else
            {
                // Jika sudah selesai ngetik, lanjut ke kalimat/halaman berikutnya
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
        if (indeksKalimat < daftarKalimat.Length - 1)
        {
            indeksKalimat++;
            StartCoroutine(KetikKalimat());
        }
        else
        {
            // Jika dialog sudah habis, opsional: Anda bisa menutup panel atau membiarkan pemain membaca
            Debug.Log("Dialog sudah habis. Pemain bisa menutup panel menggunakan tombol kembali/ESC.");
        }
    }

    IEnumerator KetikKalimat()
    {
        sedangNgetik = true;
        teksDialog.text = "";
        
        string kalimatTarget = daftarKalimat[indeksKalimat];
        
        // Memunculkan teks huruf demi huruf (Efek Mesin Tik)
        foreach (char huruf in kalimatTarget.ToCharArray())
        {
            teksDialog.text += huruf;
            yield return new WaitForSeconds(kecepatanKetik);
        }
        
        sedangNgetik = false;
    }
}
