using UnityEngine;
using System.Collections;

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
        grupVisual = GetComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        if (grupVisual != null)
        {
            grupVisual.alpha = 0f;
            
            StartCoroutine(ProsesMuncul());
        }
    }

    IEnumerator ProsesMuncul()
    {
        yield return new WaitForSeconds(waktuTunggu);

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
        
        grupVisual.alpha = 1f;
    }
}
