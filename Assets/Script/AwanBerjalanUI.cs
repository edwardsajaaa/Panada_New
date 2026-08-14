using UnityEngine;

public class AwanBerjalanUI : MonoBehaviour
{
    [Tooltip("Kecepatan gerak awan (pixel per detik). Kasih nilai positif biar awan jalan ke kiri.")]
    public float kecepatan = 50f;

    [Tooltip("Batas titik X sebelah kiri. Kalau awan ngelewatin titik ini, dia bakal teleport ke kanan.")]
    public float batasKiri = -1920f;

    [Tooltip("Titik X sebelah kanan untuk tempat awan muncul kembali.")]
    public float titikResetKanan = 1920f;

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (rectTransform == null) return;

        // Geser awan ke kiri secara terus-menerus
        rectTransform.anchoredPosition += Vector2.left * kecepatan * Time.deltaTime;

        // Kalau awan udah kelewat batas kiri (menghilang dari layar), pindahkan ke ujung kanan
        if (rectTransform.anchoredPosition.x <= batasKiri)
        {
            Vector2 posBaru = rectTransform.anchoredPosition;
            // Gunakan += titikResetKanan + batasKiri agar transisinya lebih mulus jika ada sisa frame
            // Tapi yang paling simpel dan aman: langsung set X ke titikResetKanan
            posBaru.x = titikResetKanan; 
            rectTransform.anchoredPosition = posBaru;
        }
    }
}
