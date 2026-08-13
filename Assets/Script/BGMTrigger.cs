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

    [Tooltip("Volume khusus untuk lagu ini (0 sampai 1). Biarkan 1 untuk volume normal.")]
    [Range(0f, 1f)]
    public float volumeKhusus = 1f;

    [Tooltip("Centang jika ingin BGM langsung diputar otomatis saat Scene/Panel terbuka")]
    public bool putarOtomatisSaatMulai = true;

    [Tooltip("Centang ini HANYA JIKA Anda ingin panel ini HENING (mematikan semua musik yang sedang menyala).")]
    public bool jadikanHening = false;

    void OnEnable()
    {
        if (putarOtomatisSaatMulai || jadikanHening)
        {
            StartCoroutine(TungguDanPutar());
        }
    }

    private System.Collections.IEnumerator TungguDanPutar()
    {
        while (BGMManager.Instance == null)
        {
            yield return null;
        }
        
        PutarBGM();
    }

    /// <summary>
    /// Bisa dipanggil secara manual lewat UnityEvent (misalnya di tombol atau akhir cutscene)
    /// </summary>
    public void PutarBGM()
    {
        if (BGMManager.Instance == null)
        {
            Debug.LogWarning("BGMTrigger: Tidak menemukan BGMManager di scene! Pastikan ada objek BGMManager.");
            return;
        }

        if (jadikanHening)
        {
            BGMManager.Instance.HentikanBGM(durasiFade);
        }
        else if (laguBGM != null)
        {
            BGMManager.Instance.PutarLagu(laguBGM, durasiFade, volumeKhusus);
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
