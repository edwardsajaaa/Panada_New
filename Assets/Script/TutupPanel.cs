using UnityEngine;

public class TutupPanel : MonoBehaviour
{
    [Tooltip("Panel yang akan ditutup. Kosongkan jika script ini ditempel langsung di objek panelnya.")]
    public GameObject panelYangDitutup;
    
    [Tooltip("Tombol keyboard untuk menutup panel secara cepat (contoh: Escape)")]
    public KeyCode tombolTutup = KeyCode.Escape;

    void Start()
    {
        if (panelYangDitutup == null)
        {
            panelYangDitutup = this.gameObject;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(tombolTutup))
        {
            Tutup();
        }
    }

    public void Tutup()
    {
        if (panelYangDitutup != null)
        {
            AnimasiPanelFoto efekAnimasi = panelYangDitutup.GetComponent<AnimasiPanelFoto>();
            
            if (efekAnimasi != null)
            {
                efekAnimasi.TutupDenganAnimasi();
            }
            else
            {
                panelYangDitutup.SetActive(false);
            }
        }
    }
}
