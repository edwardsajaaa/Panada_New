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

    [Header("Pengaturan Looping")]
    [Tooltip("Centang agar teks terus menyambung tanpa jeda kosong (seamless loop)")]
    public bool seamlessLoop = true;

    private RectTransform rectTransform;
    private RectTransform parentRect;
    private float lebarTeks;
    private float lebarWadah;

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
            // Untuk seamless loop, duplikasi teks agar selalu ada yang terlihat di layar
            string gabunganAsli = string.Join(teksPemisah, daftarBerita);
            string gabungan = seamlessLoop 
                ? gabunganAsli + teksPemisah + gabunganAsli 
                : gabunganAsli;
            
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

        // 3. Simpan ukuran
        lebarTeks = rectTransform.rect.width;
        if (parentRect != null) lebarWadah = parentRect.rect.width;

        // 4. Menghitung batas secara pintar
        if (hitungBatasOtomatis && parentRect != null)
        {
            // Teks muncul mulai dari ujung kanan wadah
            batasKanan = lebarWadah;
            
            if (seamlessLoop)
            {
                // Untuk seamless: reset posisi ketika setengah teks (bagian asli pertama) sudah lewat
                // sehingga bagian duplikat yang masih terlihat akan menyambung dengan mulus
                batasKiri = -(lebarTeks / 2f);
            }
            else
            {
                // Teks dianggap lenyap ketika posisinya sudah melewati minus dari panjang teksnya sendiri
                batasKiri = -lebarTeks;
            }

            // Posisikan teks di ujung kanan pada awal permainan
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
            if (seamlessLoop)
            {
                // Geser posisi ke kanan sebanyak setengah lebar teks (panjang teks asli)
                // Ini membuat perpindahan terjadi secara halus tanpa lompatan visual
                float selisih = rectTransform.anchoredPosition.x - batasKiri;
                rectTransform.anchoredPosition = new Vector2(batasKanan + selisih, rectTransform.anchoredPosition.y);
            }
            else
            {
                // Teleportasi (kembalikan) teks ke batas paling kanan
                rectTransform.anchoredPosition = new Vector2(batasKanan, rectTransform.anchoredPosition.y);
            }
        }
    }
}
