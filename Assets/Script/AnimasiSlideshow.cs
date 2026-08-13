using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimasiSlideshow : MonoBehaviour
{
    [Header("Pengaturan Slideshow")]
    [Tooltip("Berapa detik sebuah gambar ditampilkan sebelum berganti ke gambar berikutnya")]
    public float waktuTampil = 3f;

    [Tooltip("Centang agar urutan gambar terus berulang dari awal tanpa henti")]
    public bool putarTerusMenerus = true;

    [Header("Otomatisasi")]
    [Tooltip("Jika dicentang, script ini akan otomatis mengambil seluruh objek di dalam (anak) sebagai slide. Anda tidak perlu memasukkannya satu per satu secara manual.")]
    public bool ambilSemuaAnakOtomatis = true;

    [Header("Daftar Gambar")]
    [Tooltip("Abaikan kolom ini jika Anda mencentang 'Ambil Semua Anak Otomatis' di atas")]
    public List<GameObject> daftarGambar = new List<GameObject>();

    private int indeksSekarang = 0;

    void Awake()
    {
        // Ambil semua objek anak jika fitur otomatis menyala
        if (ambilSemuaAnakOtomatis)
        {
            daftarGambar.Clear();
            foreach (Transform anak in transform)
            {
                // Abaikan objek yang merupakan teks (mengandung TextMeshPro)
                if (anak.GetComponent<TMPro.TMP_Text>() != null)
                {
                    continue; // Jangan masukkan ke daftar slideshow
                }
                
                daftarGambar.Add(anak.gameObject);
            }
        }
    }

    // Dipanggil setiap kali panel berita / induk dari script ini dinyalakan
    void OnEnable()
    {
        if (daftarGambar.Count > 0)
        {
            // Sembunyikan seluruh gambar di awal agar tidak numpuk
            foreach (GameObject gambar in daftarGambar)
            {
                if (gambar != null) gambar.SetActive(false);
            }

            indeksSekarang = 0;
            StartCoroutine(JalankanSlideshow());
        }
    }

    IEnumerator JalankanSlideshow()
    {
        while (true)
        {
            // 1. Nyalakan gambar pada indeks saat ini
            if (daftarGambar[indeksSekarang] != null)
            {
                daftarGambar[indeksSekarang].SetActive(true);
            }

            // 2. Tunggu selama beberapa detik (Sesuai Waktu Tampil)
            yield return new WaitForSeconds(waktuTampil);

            // 3. Matikan gambar saat ini sebelum pindah ke gambar baru
            if (daftarGambar[indeksSekarang] != null)
            {
                daftarGambar[indeksSekarang].SetActive(false);
            }

            // 4. Maju ke gambar berikutnya
            indeksSekarang++;
            
            // 5. Jika sudah mencapai batas gambar terakhir
            if (indeksSekarang >= daftarGambar.Count)
            {
                if (putarTerusMenerus)
                {
                    // Ulangi kembali ke gambar pertama (loop)
                    indeksSekarang = 0;
                }
                else
                {
                    // Berhenti di gambar terakhir dan pastikan gambar tersebut tetap menyala
                    if (daftarGambar[daftarGambar.Count - 1] != null)
                    {
                        daftarGambar[daftarGambar.Count - 1].SetActive(true);
                    }
                    break; // Akhiri proses Coroutine
                }
            }
        }
    }
}
