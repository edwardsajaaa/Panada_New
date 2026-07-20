using UnityEngine;
using UnityEngine.UI;

public class BukaPanelPesan : MonoBehaviour
{
    [Header("Panel Target")]
    public GameObject panelPesan;

    void Start()
    {
        // pastiin panel pesan mati pas game baru mulai
        if (panelPesan != null)
        {
            panelPesan.SetActive(false);
        }

        // pasang event klik otomatis kalau ada komponen Button
        Button tombol = GetComponent<Button>();
        if (tombol != null)
        {
            tombol.onClick.AddListener(TampilkanPesan);
        }
    }

    // fungsi ini dipanggil pas tombol notifikasi diklik
    public void TampilkanPesan()
    {
        if (panelPesan != null)
        {
            panelPesan.SetActive(true); // nyalain panel HP gede
            
            // sembunyiin notifikasi ini biar ga menuh-menuhin layar / ga diklik 2x
            gameObject.SetActive(false); 
        }
    }
}
