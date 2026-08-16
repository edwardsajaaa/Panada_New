using UnityEngine;
using UnityEngine.Events;

public class PemicuTransisi : MonoBehaviour
{
    [Header("Pengaturan Transisi")]
    [Tooltip("Event ini akan dieksekusi saat layar sudah tertutup pixel hitam secara penuh (di tengah-tengah transisi). Tempat yang pas untuk menyembunyikan/memunculkan objek.")]
    public UnityEvent saatLayarGelap;

    // Fungsi ini dipanggil dari PopupInteraksi (Saat Diinteraksi)
    public void JalankanTransisi()
    {
        if (TransisiRuangan.Instance != null)
        {
            // Minta TransisiRuangan untuk mainkan efeknya, dan jalankan event kita pas layar gelap
            TransisiRuangan.Instance.Jalankan(saatLayarGelap);
        }
        else
        {
            Debug.LogError("PemicuTransisi: Objek dengan script 'TransisiRuangan' belum ditambahkan ke Scene ini! Event langsung dijalankan tanpa animasi.");
            saatLayarGelap?.Invoke();
        }
    }
}
