using UnityEngine;

// Script pelapor untuk barang yang wajib diinteraksi (PC, Koper, Laci).
public class ItemMisi : MonoBehaviour
{
    [Tooltip("Masukkan objek Pintu (yang memiliki script SistemMisiKamar) ke sini.")]
    public SistemMisiKamar manajerMisi;

    private bool sudahDicek = false;

    // Dipanggil dari Unity Event PopupInteraksi pada barang ini.
    public void TandaiDiinteraksi()
    {
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
