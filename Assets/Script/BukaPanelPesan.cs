using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BukaPanelPesan : MonoBehaviour
{
    [Header("Panel Target")]
    public GameObject panelPesan;

    [Header("Animasi Kedip")]
    public float durasiTutupMata = 0.3f;
    public float jedaGelap = 0.15f;
    public float durasiBukaMata = 0.4f;

    [Tooltip("Masukkan material EyeBlinkMat dari folder Shaders (opsional). Kalau kosong, pakai fade hitam biasa.")]
    public Material materialBlink;

    void Start()
    {
        if (panelPesan != null)
            panelPesan.SetActive(false);

        Button tombol = GetComponent<Button>();
        if (tombol != null)
            tombol.onClick.AddListener(OnKlik);
    }

    void OnKlik()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindObjectOfType<Canvas>();

        MonoBehaviour runner = canvas.GetComponent<MonoBehaviour>();
        runner.StartCoroutine(ProsesKedipLaluBukaPanel());
    }

    IEnumerator ProsesKedipLaluBukaPanel()
    {
        gameObject.SetActive(false);

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) yield break;

        // --- BUAT LAYAR BLINK BARU ---
        GameObject layarHitam = new GameObject("LayarKedip");
        layarHitam.transform.SetParent(canvas.transform, false);
        layarHitam.transform.SetAsLastSibling();

        Image img = layarHitam.AddComponent<Image>();
        img.raycastTarget = false;

        // Pakai sprite built-in "Background" persis seperti di BlackScreenPanel Pengenalan
        Sprite bgSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Background.psd");
        if (bgSprite != null)
        {
            img.sprite = bgSprite;
            img.type = Image.Type.Sliced;
            img.fillCenter = true;
        }

        RectTransform rect = img.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        // --- PASANG MATERIAL BLINK JIKA ADA ---
        Material matInstance = null;
        bool pakaiShader = false;

        if (materialBlink != null && materialBlink.HasProperty("_Blink"))
        {
            matInstance = new Material(materialBlink);
            img.material = matInstance;
            img.color = Color.black;
            matInstance.SetFloat("_Blink", 0f); // mulai dari mata terbuka
            pakaiShader = true;
        }
        else
        {
            img.color = new Color(0, 0, 0, 0); // mulai dari transparan
        }

        // --- FASE 1: TUTUP MATA ---
        float waktu = 0f;
        while (waktu < durasiTutupMata)
        {
            waktu += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(waktu / durasiTutupMata);

            if (pakaiShader)
                matInstance.SetFloat("_Blink", t);
            else
                img.color = new Color(0, 0, 0, t);

            yield return null;
        }

        // Pastikan mentok tertutup
        if (pakaiShader)
            matInstance.SetFloat("_Blink", 1f);
        else
            img.color = Color.black;

        // --- JEDA GELAP ---
        yield return new WaitForSecondsRealtime(jedaGelap);

        // --- FASE 2: BUKA MATA ---
        waktu = 0f;
        while (waktu < durasiBukaMata)
        {
            waktu += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(1f - (waktu / durasiBukaMata));

            if (pakaiShader)
                matInstance.SetFloat("_Blink", t);
            else
                img.color = new Color(0, 0, 0, t);

            yield return null;
        }

        // Pastikan mentok terbuka
        if (pakaiShader)
            matInstance.SetFloat("_Blink", 0f);
        else
            img.color = new Color(0, 0, 0, 0);

        // --- BUKA PANEL PESAN (tangan meluncur) ---
        if (panelPesan != null)
            panelPesan.SetActive(true);

        // --- BERSIH-BERSIH ---
        if (matInstance != null) Destroy(matInstance);
        Destroy(layarHitam);
    }
}
