using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

        if (daftarBerita != null && daftarBerita.Length > 0)
        {
            string gabunganAsli = string.Join(teksPemisah, daftarBerita);
            string gabungan = seamlessLoop 
                ? gabunganAsli + teksPemisah + gabunganAsli 
                : gabunganAsli;
            
            TMP_Text tmpText = GetComponent<TMP_Text>();
            if (tmpText != null) tmpText.text = gabungan;
            else
            {
                Text uiText = GetComponent<Text>();
                if (uiText != null) uiText.text = gabungan;
            }
        }

        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

        lebarTeks = rectTransform.rect.width;
        if (parentRect != null) lebarWadah = parentRect.rect.width;

        if (hitungBatasOtomatis && parentRect != null)
        {
            batasKanan = lebarWadah;
            
            if (seamlessLoop)
            {
                batasKiri = -(lebarTeks / 2f);
            }
            else
            {
                batasKiri = -lebarTeks;
            }

            rectTransform.anchoredPosition = new Vector2(batasKanan, rectTransform.anchoredPosition.y);
        }
    }

    void Update()
    {
        rectTransform.anchoredPosition += Vector2.left * kecepatan * Time.deltaTime;

        if (rectTransform.anchoredPosition.x <= batasKiri)
        {
            if (seamlessLoop)
            {
                float selisih = rectTransform.anchoredPosition.x - batasKiri;
                rectTransform.anchoredPosition = new Vector2(batasKanan + selisih, rectTransform.anchoredPosition.y);
            }
            else
            {
                rectTransform.anchoredPosition = new Vector2(batasKanan, rectTransform.anchoredPosition.y);
            }
        }
    }
}
