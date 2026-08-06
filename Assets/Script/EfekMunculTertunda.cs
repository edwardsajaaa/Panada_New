using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Script ini wajib ditempel di objek yang memiliki komponen visual (Text, Image, dll)
[RequireComponent(typeof(Graphic))]
public class EfekMunculTertunda : MonoBehaviour
{
    [Header("Pengaturan Waktu")]
    [Tooltip("Berapa detik objek ini harus menunggu/bersembunyi sebelum mulai muncul?")]
    public float waktuTunggu = 3f;
    
    [Tooltip("Berapa lama durasi transisi fade-in nya? (0 = langsung muncul tanpa animasi)")]
    public float durasiFade = 0.5f;

    private Graphic objekVisual;

    void Awake()
    {
        // Mengambil komponen Text/Image dari objek ini
        objekVisual = GetComponent<Graphic>();
    }

    // Dipanggil setiap kali panel induknya dinyalakan
    void OnEnable()
    {
        if (objekVisual != null)
        {
            // Jadikan transparan sepenuhnya di awal
            Color c = objekVisual.color;
            c.a = 0f;
            objekVisual.color = c;
            
            // Mulai penghitung waktu mundur
            StartCoroutine(ProsesMuncul());
        }
    }

    IEnumerator ProsesMuncul()
    {
        // 1. Tunggu selama 3-4 detik (sesuai input Anda)
        yield return new WaitForSeconds(waktuTunggu);

        // 2. Lakukan animasi Fade In
        if (durasiFade > 0)
        {
            float timer = 0;
            while (timer < durasiFade)
            {
                timer += Time.deltaTime;
                Color c = objekVisual.color;
                c.a = Mathf.Lerp(0f, 1f, timer / durasiFade);
                objekVisual.color = c;
                yield return null;
            }
        }
        
        // 3. Pastikan opasitas mentok 100% di akhir
        Color final = objekVisual.color;
        final.a = 1f;
        objekVisual.color = final;
    }
}
