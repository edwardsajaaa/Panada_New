using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class EfekFadeUI : MonoBehaviour
{
    [Header("Pengaturan Fade")]
    [Tooltip("Centang jika ingin animasi langsung jalan pas objek ini aktif. Hilangkan centang jika ingin memanggilnya manual lewat script/Event")]
    public bool mulaiOtomatis = true;
    [Tooltip("Waktu tunggu sebelum mulai muncul (harus sama dengan durasi gelap layar transisi)")]
    public float jedaSebelumMuncul = 1f;
    [Tooltip("Berapa lama proses fade-in berlangsung")]
    public float durasiFade = 1.5f;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        // Langsung bikin transparan di frame pertama
        canvasGroup.alpha = 0f;
        if (mulaiOtomatis) 
        {
            MulaiFadeIn();
        }
    }

    public void MulaiFadeIn()
    {
        StartCoroutine(ProsesFadeIn());
    }

    IEnumerator ProsesFadeIn()
    {
        // Tunggu layar gelap dari transisi pixel selesai
        yield return new WaitForSeconds(jedaSebelumMuncul);

        float waktu = 0f;
        while (waktu < durasiFade)
        {
            waktu += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, waktu / durasiFade);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }
}
