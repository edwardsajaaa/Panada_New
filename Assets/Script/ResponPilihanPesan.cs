using UnityEngine;
using System.Collections;

public class ResponPilihanPesan : MonoBehaviour
{
    [Header("UI Handphone")]
    public GameObject panelPesan;
    public GameObject panelPilihan;

    [Header("Reaksi: Nanti Saja")]
    public GameObject nathanObj;
    [Tooltip("Pilih efek transisi masuk untuk gambar Nathan")]
    public AnimasiTombolMenu.ModeAnimasiIn transisiNathan = AnimasiTombolMenu.ModeAnimasiIn.Fade;
    public GameObject bubleNameObj;
    public float jedaBubleName = 0.5f;

    [Header("Objek Lingkungan (Opsional)")]
    [Tooltip("Objek HP di atas meja yang ingin dimatikan")]
    public GameObject handphoneMeja;

    public void KlikNantiSaja()
    {
        // Tutup opsi aja, HP biarin
        if (panelPilihan != null) panelPilihan.SetActive(false);

        // Matikan HP di meja jika ada
        if (handphoneMeja != null) handphoneMeja.SetActive(false);

        // Munculin char dengan animasi transisi
        if (nathanObj != null)
        {
            // Pasang atau atur animasi sebelum diaktifkan agar transisi In berjalan mulus
            AnimasiTombolMenu anim = nathanObj.GetComponent<AnimasiTombolMenu>();
            if (anim == null) anim = nathanObj.AddComponent<AnimasiTombolMenu>();
            
            anim.modeAnimasiIn = transisiNathan;
            anim.durasiAnimasiIn = 0.4f;
            anim.gunakanAnimasiIn = true;

            nathanObj.SetActive(true);
        }

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
