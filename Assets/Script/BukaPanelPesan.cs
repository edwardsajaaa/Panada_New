using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BukaPanelPesan : MonoBehaviour
{
    [Header("Panel Target")]
    public GameObject panelPesan;

    [Header("Animasi Kedip")]
    public float durasiTutupMata = 0.25f;
    public float jedaGelap = 0.15f;
    public float durasiBukaMata = 0.4f;

    [Tooltip("Masukkan BlackScreenPanel yang sudah ada di scene (panel Image dengan material EyeBlinkMat).")]
    public GameObject panelLayarHitam;

    [Header("Panel Lanjutan (Opsional)")]
    [Tooltip("Masukkan game object yang ingin dimunculkan SETELAH tangan selesai meluncur (misal: panel pilihan).")]
    public GameObject panelLanjutan;
    [Tooltip("Waktu tunggu sebelum panel lanjutan muncul (disesuaikan dengan durasi animasi tangan).")]
    public float jedaPanelLanjutan = 0.5f;

    [Header("Pengaturan Tambahan Pesan Kedua (Opsional)")]
    [Tooltip("Objek pesan tambahan yang akan dimunculkan (misal: Pesan (1) & Pesan (2))")]
    public GameObject[] pesanBaru;
    [Tooltip("Tombol yang ingin dinonaktifkan (misal: Nanti Saja)")]
    public GameObject tombolDimatikan;
    [Tooltip("Tombol yang ingin dipindah ke tengah (misal: Balas Sekarang)")]
    public RectTransform tombolDitengah;
    [Tooltip("Posisi tujuan untuk tombol ditengah (X, Y)")]
    public Vector2 posisiTengah = new Vector2(-16f, -129.359f);

    void Start()
    {
        if (panelPesan != null)
            panelPesan.SetActive(false);

        // Pastikan layar hitam mati di awal agar tidak menghalangi
        if (panelLayarHitam != null)
            panelLayarHitam.SetActive(false);

        // Pastikan panel lanjutan juga mati di awal
        if (panelLanjutan != null)
            panelLanjutan.SetActive(false);

        Button tombol = GetComponent<Button>();
        if (tombol != null)
            tombol.onClick.AddListener(OnKlik);
    }

    void OnKlik()
    {
        // Jalankan coroutine di object ini sendiri
        StartCoroutine(ProsesKedip());
    }

    IEnumerator ProsesKedip()
    {
        // 1. Sembunyikan visual dan interaksi tombol notif agar tidak diklik 2x
        Image imgTombol = GetComponent<Image>();
        if (imgTombol != null) imgTombol.enabled = false;
        
        Button btnTombol = GetComponent<Button>();
        if (btnTombol != null) btnTombol.enabled = false;

        // 2. Nyalakan BlackScreenPanel
        if (panelLayarHitam == null) yield break;
        panelLayarHitam.SetActive(true);
        panelLayarHitam.transform.SetAsLastSibling();

        // 3. Ambil material blink dari Image yang sudah terpasang di BlackScreenPanel
        Image bgImage = panelLayarHitam.GetComponent<Image>();
        Material originalMat = null;
        Material blinkMat = null;

        if (bgImage != null && bgImage.material != null && bgImage.material.HasProperty("_Blink"))
        {
            originalMat = bgImage.material;
            blinkMat = new Material(originalMat);
            bgImage.material = blinkMat;
            blinkMat.SetFloat("_Blink", 0f); // mulai dari mata terbuka
        }

        // 4. TUTUP MATA (0 -> 1)
        float waktu = 0f;
        while (waktu < durasiTutupMata)
        {
            waktu += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(waktu / durasiTutupMata);

            if (blinkMat != null)
                blinkMat.SetFloat("_Blink", t);

            yield return null;
        }
        if (blinkMat != null) blinkMat.SetFloat("_Blink", 1f);

        // 5. JEDA GELAP
        yield return new WaitForSecondsRealtime(jedaGelap);

        // 5.5. AKTIFKAN PANEL BARU (TANGAN MELUNCUR) SAAT LAYAR MASIH GELAP
        // Sehingga ketika mata mulai terbuka, panel baru sudah dalam proses transisi masuk
        if (panelPesan != null)
            panelPesan.SetActive(true);

        // -- MUNCULKAN PESAN KEDUA & UBAH POSISI TOMBOL (JIKA ADA) --
        if (pesanBaru != null)
        {
            foreach (var pesan in pesanBaru)
            {
                if (pesan != null) pesan.SetActive(true);
            }
        }
        if (tombolDimatikan != null) tombolDimatikan.SetActive(false);
        if (tombolDitengah != null) tombolDitengah.anchoredPosition = posisiTengah;

        // 6. BUKA MATA (1 -> 0)
        waktu = 0f;
        while (waktu < durasiBukaMata)
        {
            waktu += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(1f - (waktu / durasiBukaMata));

            if (blinkMat != null)
                blinkMat.SetFloat("_Blink", t);

            yield return null;
        }
        if (blinkMat != null) blinkMat.SetFloat("_Blink", 0f);

        // 7. Matikan BlackScreenPanel
        panelLayarHitam.SetActive(false);

        // 9. Bersihkan material instance dan kembalikan originalnya
        if (bgImage != null && originalMat != null)
        {
            bgImage.material = originalMat;
        }
        if (blinkMat != null) Destroy(blinkMat);

        // 10. Tunggu sampai animasi tangan meluncur selesai, lalu munculkan panel pilihan
        if (panelLanjutan != null)
        {
            yield return new WaitForSecondsRealtime(jedaPanelLanjutan);
            panelLanjutan.SetActive(true);
        }

        // 11. Matikan tombol ini sepenuhnya setelah semua proses selesai
        gameObject.SetActive(false);
    }
}
