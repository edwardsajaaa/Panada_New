using UnityEngine;

public class TutupPanel : MonoBehaviour
{
    [Tooltip("Panel yang akan ditutup. Kosongkan jika script ini ditempel langsung di objek panelnya.")]
    public GameObject panelYangDitutup;
    
    [Tooltip("Tombol keyboard untuk menutup panel secara cepat (contoh: Escape)")]
    public KeyCode tombolTutup = KeyCode.Escape;

    void Start()
    {
        // Jika kolom panel dikosongkan, anggap objek tempat script ini berada adalah panelnya
        if (panelYangDitutup == null)
        {
            panelYangDitutup = this.gameObject;
        }
    }

    void Update()
    {
        // Deteksi jika pemain menekan tombol keyboard (misal ESC)
        if (Input.GetKeyDown(tombolTutup))
        {
            Tutup();
        }
    }

    // Fungsi ini sengaja dibuat 'public' agar bisa dikaitkan dengan Tombol UI (Button OnClick)
    public void Tutup()
    {
        if (panelYangDitutup != null)
        {
            panelYangDitutup.SetActive(false);
        }
    }
}
