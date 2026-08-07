using UnityEngine;

public class SistemBlokirGerak : MonoBehaviour
{
    // Variabel global (static) untuk menghitung berapa panel yang sedang terbuka
    public static int jumlahPanelTerbuka = 0;

    // Saat panel diaktifkan (SetActive(true))
    void OnEnable()
    {
        jumlahPanelTerbuka++;
    }

    // Saat panel dimatikan (SetActive(false))
    void OnDisable()
    {
        jumlahPanelTerbuka--;
        // Jaga-jaga agar angkanya tidak pernah minus (bug-safe)
        if (jumlahPanelTerbuka < 0) jumlahPanelTerbuka = 0;
    }

    // Fungsi cek cepat dari script lain
    public static bool SedangBukaUI()
    {
        return jumlahPanelTerbuka > 0;
    }
}
