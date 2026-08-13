using UnityEngine;

/// <summary>
/// Script pemicu sederhana untuk dipasang di dalam Scene (misal di Main Camera atau GameManager).
/// Saat Scene terbuka, script ini akan otomatis menyuruh BGMManager memainkan lagu yang dipilih.
/// </summary>
public class BGMTrigger : MonoBehaviour
{
    [Tooltip("Lagu BGM yang ingin diputar di Scene ini")]
    public AudioClip laguBGM;
    
    [Tooltip("Berapa lama efek fade transisi pergantian lagu (dalam detik)")]
    public float durasiFade = 2f;

    [Tooltip("Centang jika ingin BGM langsung diputar otomatis saat Scene/Panel terbuka")]
    public bool putarOtomatisSaatMulai = true;

    void OnEnable()
    {
        if (putarOtomatisSaatMulai)
        {
            StartCoroutine(TungguDanPutar());
        }
    }

    private System.Collections.IEnumerator TungguDanPutar()
    {
        // Tunggu sebentar sampai BGMManager selesai memuat dirinya (mencegah error jika panel ini aktif duluan)
        while (BGMManager.Instance == null)
        {
            yield return null; // Tunggu frame berikutnya
        }
        
        PutarBGM();
    }

    /// <summary>
    /// Bisa dipanggil secara manual lewat UnityEvent (misalnya di tombol atau akhir cutscene)
    /// </summary>
    public void PutarBGM()
    {
        if (BGMManager.Instance != null && laguBGM != null)
        {
            BGMManager.Instance.PutarLagu(laguBGM, durasiFade);
        }
        else if (BGMManager.Instance == null)
        {
            Debug.LogWarning("BGMTrigger: Tidak menemukan BGMManager di scene! Pastikan ada objek BGMManager.");
        }
    }

    /// <summary>
    /// Mematikan lagu yang sedang menyala secara perlahan
    /// </summary>
    public void HentikanBGMSaatIni()
    {
        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.HentikanBGM(durasiFade);
        }
    }
}
