using UnityEngine;
using UnityEngine.Events;

// Script manajer yang mengunci pintu ruangan sampai sejumlah misi (interaksi barang) terpenuhi.
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
    
    [Tooltip("Centang ini jika ingin menggunakan efek transisi hitam ber-pixel saat pintu berhasil dibuka.")]
    public bool gunakanTransisi = true;

    private int jumlahSudahDicek = 0;

    // Dipanggil oleh script ItemMisi pada barang (PC, Koper) saat pemain menginteraksinya.
    public void TambahProgres()
    {
        jumlahSudahDicek++;
        Debug.Log($"Progres Misi Kamar: {jumlahSudahDicek} / {totalBarangWajib} barang telah dicek.");
    }

    // Dipanggil oleh PopupInteraksi milik Pintu Keluar.
    public void CobaKeluar()
    {
        if (jumlahSudahDicek >= totalBarangWajib)
        {
            Debug.Log("Misi selesai! Pintu terbuka.");
            
            if (gunakanTransisi && TransisiRuangan.Instance != null)
            {
                TransisiRuangan.Instance.Jalankan(saatBisaKeluar);
            }
            else
            {
                saatBisaKeluar?.Invoke();
            }
        }
        else
        {
            int kurang = totalBarangWajib - jumlahSudahDicek;
            Debug.Log($"Pintu terkunci! Masih ada {kurang} barang yang belum dicek.");
            saatBelumBisaKeluar?.Invoke();
        }
    }
}
