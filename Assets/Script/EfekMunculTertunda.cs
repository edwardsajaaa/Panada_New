using UnityEngine;
using System.Collections;

// Memastikan objek ini memiliki CanvasGroup agar bisa mengatur transparansi seluruh isinya sekaligus
[RequireComponent(typeof(CanvasGroup))]
public class EfekMunculTertunda : MonoBehaviour
{
    [Header("Pengaturan Waktu")]
    [Tooltip("Berapa detik objek ini harus menunggu/bersembunyi sebelum mulai muncul?")]
    public float waktuTunggu = 3f;
    
    [Tooltip("Berapa lama durasi transisi fade-in nya? (0 = langsung muncul tanpa animasi)")]
    public float durasiFade = 0.5f;

    private CanvasGroup grupVisual;

    void Awake()
    {
        // Mengambil komponen CanvasGroup
        grupVisual = GetComponent<CanvasGroup>();
    }

    // Dipanggil setiap kali panel induknya dinyalakan
    void OnEnable()
    {
        if (grupVisual != null)
        {
            // Jadikan transparan sepenuhnya di awal
            grupVisual.alpha = 0f;
            
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
                grupVisual.alpha = Mathf.Lerp(0f, 1f, timer / durasiFade);
                yield return null;
            }
        }
        
        // 3. Pastikan opasitas mentok 100% di akhir
        grupVisual.alpha = 1f;
    }
}
