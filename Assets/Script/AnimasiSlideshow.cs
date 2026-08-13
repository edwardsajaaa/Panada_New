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
        if (ambilSemuaAnakOtomatis)
        {
            daftarGambar.Clear();
            foreach (Transform anak in transform)
            {
                if (anak.GetComponent<TMPro.TMP_Text>() != null)
                {
                    continue;
                }
                
                daftarGambar.Add(anak.gameObject);
            }
        }
    }

    void OnEnable()
    {
        if (daftarGambar.Count > 0)
        {
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
            if (daftarGambar[indeksSekarang] != null)
            {
                daftarGambar[indeksSekarang].SetActive(true);
            }

            yield return new WaitForSeconds(waktuTampil);

            if (daftarGambar[indeksSekarang] != null)
            {
                daftarGambar[indeksSekarang].SetActive(false);
            }

            indeksSekarang++;
            
            if (indeksSekarang >= daftarGambar.Count)
            {
                if (putarTerusMenerus)
                {
                    indeksSekarang = 0;
                }
                else
                {
                    if (daftarGambar[daftarGambar.Count - 1] != null)
                    {
                        daftarGambar[daftarGambar.Count - 1].SetActive(true);
                    }
                    break;
                }
            }
        }
    }
}
