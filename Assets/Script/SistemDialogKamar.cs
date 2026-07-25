using System.Collections;
using UnityEngine;
using TMPro;

public class SistemDialogKamar : MonoBehaviour
{
    [Header("Referensi UI")]
    public GameObject panelBubleName; // Keseluruhan objek Buble Name
    public TMP_Text teksNamaKarakter;
    public TMP_Text teksIsiDialog;

    [Header("Data Percakapan")]
    [Tooltip("Isi dengan dialog-dialog yang akan muncul")]
    public DataDialog[] percakapan;

    [Header("Pengaturan")]
    public float durasiKetikAnimasi = 0.02f; // Kecepatan efek ngetik (ketik per huruf)

    private int indeksDialog = 0;
    private bool sedangNgetik = false;
    private Coroutine ngetikCoroutine;

    void OnEnable()
    {
        // Mulai dialog dari awal setiap kali Buble Name diaktifkan
        indeksDialog = 0;
        if (percakapan != null && percakapan.Length > 0)
        {
            TampilkanDialogSekarang();
        }
    }

    void Update()
    {
        // Deteksi klik (Bisa klik kiri mouse atau tap layar)
        if (Input.GetMouseButtonDown(0))
        {
            // Jika teks masih sedang diketik, langsung tampilkan semua secara instan
            if (sedangNgetik)
            {
                if (ngetikCoroutine != null) StopCoroutine(ngetikCoroutine);
                teksIsiDialog.text = percakapan[indeksDialog].teksDialog;
                sedangNgetik = false;
            }
            // Jika teks sudah selesai diketik, lanjut ke dialog berikutnya
            else
            {
                LanjutKeDialogBerikutnya();
            }
        }
    }

    void TampilkanDialogSekarang()
    {
        if (indeksDialog < percakapan.Length)
        {
            // Set nama karakter
            if (teksNamaKarakter != null)
                teksNamaKarakter.text = percakapan[indeksDialog].namaKarakter;

            // Mulai efek ngetik
            if (ngetikCoroutine != null) StopCoroutine(ngetikCoroutine);
            ngetikCoroutine = StartCoroutine(EfekNgetik(percakapan[indeksDialog].teksDialog));
        }
    }

    IEnumerator EfekNgetik(string teksLengkap)
    {
        sedangNgetik = true;
        teksIsiDialog.text = "";

        foreach (char huruf in teksLengkap.ToCharArray())
        {
            teksIsiDialog.text += huruf;
            yield return new WaitForSeconds(durasiKetikAnimasi);
        }

        sedangNgetik = false;
    }

    void LanjutKeDialogBerikutnya()
    {
        indeksDialog++;

        // Jika dialog masih ada, tampilkan
        if (indeksDialog < percakapan.Length)
        {
            TampilkanDialogSekarang();
        }
        else
        {
            // Dialog habis, tutup Buble Name
            TutupDialog();
        }
    }

    void TutupDialog()
    {
        if (panelBubleName != null)
            panelBubleName.SetActive(false);
            
        // TODO: Taruh logika transisi scene/aksi lanjutan di sini jika diperlukan
        Debug.Log("Dialog Selesai!");
    }
}
