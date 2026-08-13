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
    
    [Header("Referensi Teks/Dialog Tertunda")]
    [Tooltip("Gunakan ini jika teksnya hanya 1 objek (seperti Text TMP biasa)")]
    public Graphic teksTertunda; 
    
    [Tooltip("Gunakan ini jika teksnya berupa Grup/Gelembung (seperti Buble Name) yang berisi banyak objek. (Pastikan objek tersebut dipasangi CanvasGroup)")]
    public CanvasGroup grupTertunda; 

    [Header("Pengaturan Animasi")]
    [Tooltip("Jarak foto saat mulai muncul (dari bawah)")]
    public float jarakMunculDariBawah = 300f; 
    
    [Tooltip("Berapa lama waktu panel dan foto muncul penuh (detik)")]
    public float durasiMunculPanel = 0.5f;
    
    [Tooltip("Waktu tunggu sebelum teks/dialog mulai muncul (detik)")]
    public float jedaSebelumText = 0.5f;
    
    [Tooltip("Berapa lama waktu teks/dialog memudar hingga muncul penuh (detik)")]
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
        panelGroup.alpha = 0f;
        
        if (fotoObjek != null)
        {
            fotoObjek.anchoredPosition = posisiAsliFoto - new Vector2(0, jarakMunculDariBawah);
        }

        if (teksTertunda != null)
        {
            Color c = teksTertunda.color;
            c.a = 0f;
            teksTertunda.color = c;
        }
        
        if (grupTertunda != null)
        {
            grupTertunda.alpha = 0f;
        }

        // --- 2. ANIMASI PANEL FADE & FOTO NAIK BERSAMAAN ---
        float timer = 0;
        Vector2 posisiBawah = posisiAsliFoto - new Vector2(0, jarakMunculDariBawah);
        
        while (timer < durasiMunculPanel)
        {
            timer += Time.deltaTime;
            float persentase = timer / durasiMunculPanel;
            
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

        yield return new WaitForSeconds(jedaSebelumText);

        // --- 4. ANIMASI FADE TEXT / GRUP TERTUNDA ---
        if (teksTertunda != null || grupTertunda != null)
        {
            timer = 0;
            while (timer < durasiFadeText)
            {
                timer += Time.deltaTime;
                float persentase = timer / durasiFadeText;
                
                if (teksTertunda != null)
                {
                    Color c = teksTertunda.color;
                    c.a = Mathf.Lerp(0f, 1f, persentase);
                    teksTertunda.color = c;
                }
                
                if (grupTertunda != null)
                {
                    grupTertunda.alpha = Mathf.Lerp(0f, 1f, persentase);
                }
                
                yield return null;
            }
            
            if (teksTertunda != null)
            {
                Color finalColor = teksTertunda.color;
                finalColor.a = 1f;
                teksTertunda.color = finalColor;
            }
            if (grupTertunda != null)
            {
                grupTertunda.alpha = 1f;
            }
        }
    }

    // Fungsi ini dipanggil dari luar (misalnya dari script TutupPanel) untuk menutup dengan elegan
    public void TutupDenganAnimasi()
    {
        StopAllCoroutines();
        StartCoroutine(AnimasiTutup());
    }

    IEnumerator AnimasiTutup()
    {
        float timer = 0;
        float durasiTutup = durasiMunculPanel / 1.5f;
        
        Vector2 posisiSekarang = fotoObjek != null ? fotoObjek.anchoredPosition : posisiAsliFoto;
        Vector2 posisiBawah = posisiAsliFoto - new Vector2(0, jarakMunculDariBawah);
        
        float alphaTeksSekarang = teksTertunda != null ? teksTertunda.color.a : 0f;
        float alphaGrupSekarang = grupTertunda != null ? grupTertunda.alpha : 0f;

        while (timer < durasiTutup)
        {
            timer += Time.deltaTime;
            float persentase = timer / durasiTutup;
            
            panelGroup.alpha = Mathf.Lerp(1f, 0f, persentase);
            
            if (fotoObjek != null)
            {
                float easeIn = persentase * persentase;
                fotoObjek.anchoredPosition = Vector2.Lerp(posisiSekarang, posisiBawah, easeIn);
            }

            if (teksTertunda != null)
            {
                Color c = teksTertunda.color;
                c.a = Mathf.Lerp(alphaTeksSekarang, 0f, persentase);
                teksTertunda.color = c;
            }
            
            if (grupTertunda != null)
            {
                grupTertunda.alpha = Mathf.Lerp(alphaGrupSekarang, 0f, persentase);
            }
            
            yield return null;
        }
        
        gameObject.SetActive(false);
    }
}
