using UnityEngine;
using UnityEngine.UI;
using TMPro; // Untuk dukungan TextMeshPro

public class TeksBerjalanUI : MonoBehaviour
{
    [Header("Konten Berita Dinamis (Opsional)")]
    [Tooltip("Isi dengan berita-berita. Script otomatis akan menggabungkannya menjadi 1 teks panjang.")]
    [TextArea(2, 4)]
    public string[] daftarBerita;
    [Tooltip("Teks pemisah antar berita (misal: '   |   ' atau '   ***   ')")]
    public string teksPemisah = "   |   ";

    [Header("Pengaturan Gerak")]
    [Tooltip("Kecepatan bergeraknya teks (semakin besar semakin cepat)")]
    public float kecepatan = 150f;

    [Header("Pengaturan Batas Layar")]
    [Tooltip("Centang agar batas dihitung otomatis menyesuaikan seberapa panjang teks berita Anda.")]
    public bool hitungBatasOtomatis = true;

    [Tooltip("Digunakan jika Hitung Batas Otomatis dimatikan.")]
    public float batasKiri = -1000f;
    [Tooltip("Digunakan jika Hitung Batas Otomatis dimatikan.")]
    public float batasKanan = 1000f;

    private RectTransform rectTransform;
    private RectTransform parentRect;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (transform.parent != null)
        {
            parentRect = transform.parent.GetComponent<RectTransform>();
        }

        // 1. Gabungkan teks jika daftarBerita diisi
        if (daftarBerita != null && daftarBerita.Length > 0)
        {
            string gabungan = string.Join(teksPemisah, daftarBerita);
            
            // Mencari komponen teks dan menimpanya
            TMP_Text tmpText = GetComponent<TMP_Text>();
            if (tmpText != null) tmpText.text = gabungan;
            else
            {
                Text uiText = GetComponent<Text>();
                if (uiText != null) uiText.text = gabungan;
            }
        }

        // 2. Memaksa pembaruan ukuran agar ukuran 'Lebar (Width)' baru diketahui
        // (Sangat disarankan menggunakan komponen Content Size Fitter di objek ini)
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

        // 3. Menghitung batas secara pintar
        if (hitungBatasOtomatis && parentRect != null)
        {
            float lebarTeks = rectTransform.rect.width;
            float lebarWadah = parentRect.rect.width;

            // Logika dasar: (Asumsi Pivot X Teks adalah 0 / Kiri)
            // Teks muncul mulai dari ukuran lebar wadahnya (paling kanan)
            batasKanan = lebarWadah;
            
            // Teks dianggap lenyap ketika posisinya sudah melewati minus dari panjang teksnya sendiri
            batasKiri = -lebarTeks;

            // Kita posisikan teks di ujung kanan pada awal permainan
            rectTransform.anchoredPosition = new Vector2(batasKanan, rectTransform.anchoredPosition.y);
        }
    }

    void Update()
    {
        // Menggeser teks ke arah kiri secara konstan
        rectTransform.anchoredPosition += Vector2.left * kecepatan * Time.deltaTime;

        // Jika teks sudah melewati batas paling kiri
        if (rectTransform.anchoredPosition.x <= batasKiri)
        {
            // Teleportasi (kembalikan) teks ke batas paling kanan agar mengulang terus menerus
            rectTransform.anchoredPosition = new Vector2(batasKanan, rectTransform.anchoredPosition.y);
        }
    }
}
