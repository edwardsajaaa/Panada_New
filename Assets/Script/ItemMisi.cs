using UnityEngine;

/// <summary>
/// Script pelapor untuk barang yang wajib diinteraksi (PC, Koper, Laci).
/// Ditempelkan ke masing-class objek barang.
/// </summary>
public class ItemMisi : MonoBehaviour
{
    [Tooltip("Masukkan objek Pintu (yang memiliki script SistemMisiKamar) ke sini.")]
    public SistemMisiKamar manajerMisi;

    private bool sudahDicek = false;

    /// <summary>
    /// Dipanggil dari Unity Event PopupInteraksi pada barang ini.
    /// </summary>
    public void TandaiDiinteraksi()
    {
        // Hanya tambahkan progres JIKA belum pernah dicek sebelumnya
        if (!sudahDicek)
        {
            sudahDicek = true;
            
            if (manajerMisi != null)
            {
                manajerMisi.TambahProgres();
            }
            else
            {
                Debug.LogWarning("Kolom 'Manajer Misi' pada ItemMisi di objek " + gameObject.name + " masih kosong!");
            }
        }
    }
}
