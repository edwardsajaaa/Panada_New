using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Script manajer yang mengunci pintu ruangan sampai sejumlah misi (interaksi barang) terpenuhi.
/// Ditempelkan pada objek Pintu Keluar.
/// </summary>
public class SistemMisiKamar : MonoBehaviour
{
    [Header("Pengaturan Syarat Keluar")]
    [Tooltip("Jumlah barang (PC, Koper, dll) yang wajib diinteraksi sebelum pemain bisa keluar.")]
    public int totalBarangWajib = 4;

    [Header("Event Pintu TERKUNCI")]
    [Tooltip("Dijalankan saat pemain mencoba keluar (klik pintu) tapi belum mengecek semua barang.")]
    public UnityEvent saatBelumBisaKeluar;

    [Header("Event Pintu TERBUKA")]
    [Tooltip("Dijalankan saat pemain mengklik pintu dan semua barang SUDAH dicek (Misal: memanggil fungsi pindah ruangan).")]
    public UnityEvent saatBisaKeluar;

    private int jumlahSudahDicek = 0;

    /// <summary>
    /// Dipanggil oleh script ItemMisi pada barang (PC, Koper) saat pemain menginteraksinya.
    /// </summary>
    public void TambahProgres()
    {
        jumlahSudahDicek++;
        Debug.Log($"Progres Misi Kamar: {jumlahSudahDicek} / {totalBarangWajib} barang telah dicek.");
    }

    /// <summary>
    /// Dipanggil oleh PopupInteraksi milik Pintu Keluar.
    /// Menggantikan sistem pindah ruangan langsung.
    /// </summary>
    public void CobaKeluar()
    {
        if (jumlahSudahDicek >= totalBarangWajib)
        {
            // Misi selesai, izinkan keluar!
            Debug.Log("Misi selesai! Pintu terbuka.");
            saatBisaKeluar?.Invoke();
        }
        else
        {
            // Misi belum selesai, tolak!
            int kurang = totalBarangWajib - jumlahSudahDicek;
            Debug.Log($"Pintu terkunci! Masih ada {kurang} barang yang belum dicek.");
            saatBelumSelesai?.Invoke();
        }
    }
}
