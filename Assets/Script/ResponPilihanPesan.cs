using UnityEngine;
using System.Collections;

public class ResponPilihanPesan : MonoBehaviour
{
    [Header("UI Handphone")]
    [Tooltip("Panel utama Handphone (Panel Pesan) yang berisi Tangan")]
    public GameObject panelPesan;
    [Tooltip("Panel pilihan 'Apa yang akan Nathan Lakukan?'")]
    public GameObject panelPilihan;

    [Header("Reaksi: Nanti Saja")]
    [Tooltip("Gambar/Panel Nathan")]
    public GameObject nathanObj;
    [Tooltip("UI Buble Name (Dialog)")]
    public GameObject bubleNameObj;
    [Tooltip("Jeda sebelum Buble Name muncul setelah Nathan muncul")]
    public float jedaBubleName = 0.5f;

    // Dipanggil saat tombol "Nanti Saja" ditekan
    public void KlikNantiSaja()
    {
        // 1. Matikan UI HP dan panel pilihan
        if (panelPesan != null) panelPesan.SetActive(false);
        if (panelPilihan != null) panelPilihan.SetActive(false);

        // 2. Aktifkan Nathan
        if (nathanObj != null) nathanObj.SetActive(true);

        // 3. Aktifkan Buble Name dengan sedikit jeda agar terasa natural
        if (bubleNameObj != null)
        {
            StartCoroutine(ProsesMunculBuble());
        }
    }

    IEnumerator ProsesMunculBuble()
    {
        bubleNameObj.SetActive(false);
        yield return new WaitForSeconds(jedaBubleName);
        bubleNameObj.SetActive(true);
    }

    // Dipanggil saat tombol "Balas Sekarang" ditekan
    public void KlikBalasSekarang()
    {
        Debug.Log("Merespon: Balas Sekarang");
        
        // Sementara kita matikan panel pilihan saja
        if (panelPilihan != null) panelPilihan.SetActive(false);
        
        // TODO: Tambahkan aksi selanjutnya untuk membalas pesan
    }
}
