using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/// <summary>
/// Script untuk mekanik Drag & Drop baju serta sistem melipat (Klik untuk mengganti warna/gambar).
/// Harus ditempel pada objek Baju (Image).
/// </summary>
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class ItemBaju : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("Referensi Area")]
    [Tooltip("Masukkan objek 'Area ngerapihin baju' (warna Cyan) ke sini")]
    public RectTransform areaLipat;
    
    [Tooltip("Opsional: Masukkan objek area di kanan (tempat baju rapi) ke sini. Baju otomatis pindah ke sini saat selesai.")]
    public RectTransform areaSelesai;

    [Header("Pengaturan Fase (Warna / Gambar)")]
    [Tooltip("Urutan warna: 0 = Merah (Berantakan), 1 = Kuning (Setengah Rapih), 2 = Hijau (Rapih)")]
    public Color[] warnaFase = new Color[] { Color.red, Color.yellow, Color.green };
    
    [Tooltip("Opsional: Jika Anda punya gambar/sprite sendiri untuk tiap fase, masukkan di sini. Jika kosong, script hanya mengubah warna.")]
    public Sprite[] spriteFase;

    [Header("Event (Opsional)")]
    [Tooltip("Jalankan fungsi tertentu saat baju sudah hijau (fase terakhir). Misal: Mematikan baju, menambah skor, dll.")]
    public UnityEvent saatBajuSelesaiDirapikan;

    private int faseSaatIni = 0;
    private bool sedangDiArea = false;
    private bool sudahDisimpanSelesai = false;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 posisiAwal;
    private Image img;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        img = GetComponent<Image>();
        
        // Simpan posisi awal di tumpukan
        posisiAwal = rectTransform.anchoredPosition;
        
        UpdateVisual();
    }

    // Dipanggil saat baju pertama kali ditarik (Drag)
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Jika sudah ditaruh di area kanan (selesai), kunci mati tidak bisa ditarik
        if (sudahDisimpanSelesai) return;

        // Tembus raycast agar saat di-drop bisa mendeteksi area di belakangnya
        canvasGroup.blocksRaycasts = false; 
        canvasGroup.alpha = 0.8f; // Bikin agak transparan saat ditarik
    }

    // Dipanggil terus-menerus saat mouse bergerak membawa baju
    public void OnDrag(PointerEventData eventData)
    {
        if (sudahDisimpanSelesai) return;

        // Menggerakkan UI mengikuti mouse
        rectTransform.anchoredPosition += eventData.delta / GetComponentInParent<Canvas>().scaleFactor;
    }

    // Dipanggil saat klik/drag dilepas
    public void OnEndDrag(PointerEventData eventData)
    {
        if (sudahDisimpanSelesai) return;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        // Jika baju SUDAH HIJAU (sudah rapi), target drop-nya adalah areaSelesai (kanan)
        if (faseSaatIni >= warnaFase.Length - 1)
        {
            if (areaSelesai != null && RectTransformUtility.RectangleContainsScreenPoint(areaSelesai, Input.mousePosition, eventData.pressEventCamera))
            {
                // Sukses ditaruh di area kanan!
                sudahDisimpanSelesai = true;
                rectTransform.position = areaSelesai.position;
                
                // Memicu event selesai HANYA saat sudah berhasil ditaruh di kanan
                saatBajuSelesaiDirapikan?.Invoke();
            }
            else
            {
                // Jika salah taruh (tidak kena area kanan), kembalikan ke tengah
                if (areaLipat != null) rectTransform.position = areaLipat.position;
            }
        }
        else
        {
            // Jika baju BELUM HIJAU (merah/kuning), target drop-nya adalah areaLipat (tengah)
            if (areaLipat != null && RectTransformUtility.RectangleContainsScreenPoint(areaLipat, Input.mousePosition, eventData.pressEventCamera))
            {
                // Sukses masuk area tengah
                sedangDiArea = true;
                rectTransform.position = areaLipat.position;
            }
            else
            {
                // Dilepas di luar area tengah, kembalikan ke posisi awal di tumpukan
                sedangDiArea = false;
                rectTransform.anchoredPosition = posisiAwal;
                
                // Reset kembali jadi berantakan (merah)
                faseSaatIni = 0; 
                UpdateVisual();
            }
        }
    }

    // Dipanggil saat baju DIKLIK
    public void OnPointerClick(PointerEventData eventData)
    {
        if (sudahDisimpanSelesai) return; // Kalau sudah dikunci di kanan, ga bisa diklik

        // Baju hanya bisa dilipat (diklik) jika sudah ditaruh di dalam area lipat
        if (sedangDiArea && faseSaatIni < warnaFase.Length - 1)
        {
            faseSaatIni++; // Naik ke fase berikutnya (Merah -> Kuning -> Hijau)
            UpdateVisual();
        }
    }

    [Tooltip("Centang agar ukuran baju otomatis menyesuaikan gambar aslinya (tidak gepeng) saat berganti fase")]
    public bool gunakanUkuranAsliGambar = true;

    // Memperbarui visual sesuai fase saat ini
    void UpdateVisual()
    {
        // 1. Update warna
        if (warnaFase.Length > faseSaatIni)
        {
            img.color = warnaFase[faseSaatIni];
        }

        // 2. Update gambar (jika Anda mengisi spriteFase di Inspector)
        if (spriteFase != null && spriteFase.Length > faseSaatIni && spriteFase[faseSaatIni] != null)
        {
            img.sprite = spriteFase[faseSaatIni];
            
            // 3. Reset ukuran ke ukuran asli gambar agar tidak gepeng
            if (gunakanUkuranAsliGambar)
            {
                img.SetNativeSize();
            }
        }
    }
}
