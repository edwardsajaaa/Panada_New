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
        // Jika sudah rapi (hijau), baju tidak bisa ditarik lagi
        if (faseSaatIni >= warnaFase.Length - 1) return;

        // Tembus raycast agar saat di-drop bisa mendeteksi area di belakangnya
        canvasGroup.blocksRaycasts = false; 
        canvasGroup.alpha = 0.8f; // Bikin agak transparan saat ditarik
    }

    // Dipanggil terus-menerus saat mouse bergerak membawa baju
    public void OnDrag(PointerEventData eventData)
    {
        if (faseSaatIni >= warnaFase.Length - 1) return;

        // Menggerakkan UI mengikuti mouse
        rectTransform.anchoredPosition += eventData.delta / GetComponentInParent<Canvas>().scaleFactor;
    }

    // Dipanggil saat klik/drag dilepas
    public void OnEndDrag(PointerEventData eventData)
    {
        if (faseSaatIni >= warnaFase.Length - 1) return;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        // Cek apakah mouse dilepas di atas kotak "Area ngerapihin baju"
        if (areaLipat != null && RectTransformUtility.RectangleContainsScreenPoint(areaLipat, Input.mousePosition, eventData.pressEventCamera))
        {
            // Sukses masuk area, posisikan ke tengah area
            sedangDiArea = true;
            rectTransform.position = areaLipat.position;
        }
        else
        {
            // Dilepas di luar area, kembalikan ke posisi awal
            sedangDiArea = false;
            rectTransform.anchoredPosition = posisiAwal;
            
            // Reset kembali jadi berantakan (merah) jika ditarik keluar
            faseSaatIni = 0; 
            UpdateVisual();
        }
    }

    // Dipanggil saat baju DIKLIK
    public void OnPointerClick(PointerEventData eventData)
    {
        // Baju hanya bisa dilipat (diklik) jika sudah ditaruh di dalam area lipat
        if (sedangDiArea && faseSaatIni < warnaFase.Length - 1)
        {
            faseSaatIni++; // Naik ke fase berikutnya (Merah -> Kuning -> Hijau)
            UpdateVisual();

            // Jika sudah mencapai fase terakhir (Hijau)
            if (faseSaatIni >= warnaFase.Length - 1)
            {
                saatBajuSelesaiDirapikan?.Invoke();
            }
        }
    }

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
        }
    }
}
