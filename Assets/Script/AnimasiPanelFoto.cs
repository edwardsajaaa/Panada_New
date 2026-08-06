using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Memastikan objek ini punya CanvasGroup untuk mengatur transparansi (Fade)
[RequireComponent(typeof(CanvasGroup))]
public class AnimasiPanelFoto : MonoBehaviour
{
    [Header("Referensi Objek")]
    [Tooltip("Masukkan objek 'Foto' ke sini")]
    public RectTransform fotoObjek;
    
    [Tooltip("Masukkan objek 'Text (TMP)' ke sini")]
    public Graphic textObjek; 

    [Header("Pengaturan Animasi")]
    [Tooltip("Jarak foto saat mulai muncul (dari bawah)")]
    public float jarakMunculDariBawah = 300f; 
    
    [Tooltip("Berapa lama waktu panel dan foto muncul penuh (detik)")]
    public float durasiMunculPanel = 0.5f;
    
    [Tooltip("Waktu tunggu sebelum teks mulai muncul (detik)")]
    public float jedaSebelumText = 0.5f;
    
    [Tooltip("Berapa lama waktu teks memudar hingga muncul penuh (detik)")]
    public float durasiFadeText = 0.5f;

    private CanvasGroup panelGroup;
    private Vector2 posisiAsliFoto;

    void Awake()
    {
        panelGroup = GetComponent<CanvasGroup>();
        if (fotoObjek != null)
        {
            posisiAsliFoto = fotoObjek.anchoredPosition;
        }
    }

    // OnEnable dipanggil setiap kali panel ini diaktifkan (SetActive(true))
    void OnEnable()
    {
        StartCoroutine(MainkanAnimasi());
    }

    IEnumerator MainkanAnimasi()
    {
        // --- 1. PERSIAPAN AWAL (Sembunyikan semua) ---
        panelGroup.alpha = 0f; // Panel transparan
        
        if (fotoObjek != null)
        {
            // Tarik foto ke bawah
            fotoObjek.anchoredPosition = posisiAsliFoto - new Vector2(0, jarakMunculDariBawah);
        }

        if (textObjek != null)
        {
            // Buat teks transparan
            Color c = textObjek.color;
            c.a = 0f;
            textObjek.color = c;
        }

        // --- 2. ANIMASI PANEL FADE & FOTO NAIK BERSAMAAN ---
        float timer = 0;
        Vector2 posisiBawah = posisiAsliFoto - new Vector2(0, jarakMunculDariBawah);
        
        while (timer < durasiMunculPanel)
        {
            timer += Time.deltaTime;
            float persentase = timer / durasiMunculPanel;
            
            // Fade In Panel
            panelGroup.alpha = Mathf.Lerp(0f, 1f, persentase);
            
            // Foto bergerak naik (menggunakan Ease Out agar melambat mulus di akhir)
            if (fotoObjek != null)
            {
                float easeOut = 1f - (1f - persentase) * (1f - persentase);
                fotoObjek.anchoredPosition = Vector2.Lerp(posisiBawah, posisiAsliFoto, easeOut);
            }
            
            yield return null;
        }
        
        // Pastikan nilainya mentok di 100% pada akhir animasi
        panelGroup.alpha = 1f;
        if (fotoObjek != null) fotoObjek.anchoredPosition = posisiAsliFoto;

        // --- 3. JEDA TUNGGU ---
        yield return new WaitForSeconds(jedaSebelumText);

        // --- 4. ANIMASI FADE TEXT ---
        if (textObjek != null)
        {
            timer = 0;
            while (timer < durasiFadeText)
            {
                timer += Time.deltaTime;
                float persentase = timer / durasiFadeText;
                
                Color c = textObjek.color;
                c.a = Mathf.Lerp(0f, 1f, persentase);
                textObjek.color = c;
                
                yield return null;
            }
            
            // Pastikan teks 100% muncul
            Color finalColor = textObjek.color;
            finalColor.a = 1f;
            textObjek.color = finalColor;
        }
    }
}
