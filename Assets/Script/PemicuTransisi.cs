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
        // Prioritaskan LoadingScreenController jika ada
        if (LoadingScreenController.Instance != null)
        {
            LoadingScreenController.Instance.TransisiLokalEvent(saatLayarGelap);
        }
        else if (TransisiRuangan.Instance != null)
        {
            TransisiRuangan.Instance.Jalankan(saatLayarGelap);
        }
        else
        {
            Debug.LogError("PemicuTransisi: Objek dengan script LoadingScreenController/TransisiRuangan belum ditambahkan ke Scene! Event langsung dijalankan.");
            saatLayarGelap?.Invoke();
        }
    }
}
