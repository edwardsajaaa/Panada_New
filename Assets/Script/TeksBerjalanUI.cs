using UnityEngine;

public class TeksBerjalanUI : MonoBehaviour
{
    [Header("Pengaturan Gerak")]
    [Tooltip("Kecepatan bergeraknya teks (semakin besar semakin cepat)")]
    public float kecepatan = 150f;

    [Header("Batas Layar (Lihat Pos X di RectTransform)")]
    [Tooltip("Angka Pos X saat teks sudah menghilang sepenuhnya di sebelah KIRI")]
    public float batasKiri = -1000f;
    
    [Tooltip("Angka Pos X saat teks baru mau muncul dari sebelah KANAN")]
    public float batasKanan = 1000f;

    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
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
