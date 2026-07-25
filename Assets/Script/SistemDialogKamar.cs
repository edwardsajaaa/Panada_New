using System.Collections;
using UnityEngine;
using TMPro;

public class SistemDialogKamar : MonoBehaviour
{
    [Header("Referensi UI")]
    public GameObject panelBubleName;
    public TMP_Text teksNamaKarakter;
    public TMP_Text teksIsiDialog;

    [Header("Data Percakapan")]
    [Tooltip("Isi dengan dialog-dialog yang akan muncul")]
    public DataDialog[] percakapan;

    [Header("Pengaturan Transisi")]
    public float durasiTransisiTeks = 0.3f; 

    private int indeksDialog = 0;
    private bool sedangTransisi = false;
    private Coroutine transisiCoroutine;

    void OnEnable()
    {
        indeksDialog = 0;
        
        // Animasi popup awal untuk keseluruhan panel
        if (panelBubleName != null)
        {
            StartCoroutine(PopupAwalObjek(panelBubleName.transform, durasiTransisiTeks));
        }

        if (percakapan != null && percakapan.Length > 0)
        {
            TampilkanDialogSekarang();
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (sedangTransisi)
            {
                if (transisiCoroutine != null) StopCoroutine(transisiCoroutine);
                SetTeksAlpha(1f);
                sedangTransisi = false;
            }
            else
            {
                LanjutKeDialogBerikutnya();
            }
        }
    }

    void TampilkanDialogSekarang()
    {
        if (indeksDialog < percakapan.Length)
        {
            if (teksNamaKarakter != null)
                teksNamaKarakter.text = percakapan[indeksDialog].namaKarakter;

            teksIsiDialog.text = percakapan[indeksDialog].teksDialog;

            if (transisiCoroutine != null) StopCoroutine(transisiCoroutine);
            transisiCoroutine = StartCoroutine(FadeTeks(0f, 1f, durasiTransisiTeks));
        }
    }

    IEnumerator FadeTeks(float alphaAwal, float alphaAkhir, float durasi)
    {
        sedangTransisi = true;
        SetTeksAlpha(alphaAwal);
        
        float waktuMulai = Time.time;
        while (Time.time < waktuMulai + durasi)
        {
            float progress = (Time.time - waktuMulai) / durasi;
            SetTeksAlpha(Mathf.Lerp(alphaAwal, alphaAkhir, progress));
            yield return null;
        }
        
        SetTeksAlpha(alphaAkhir);
        sedangTransisi = false;
    }

    void SetTeksAlpha(float alpha)
    {
        if (teksIsiDialog != null)
        {
            Color c = teksIsiDialog.color;
            c.a = alpha;
            teksIsiDialog.color = c;
        }
    }

    IEnumerator PopupAwalObjek(Transform obj, float durasi)
    {
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.gameObject.AddComponent<CanvasGroup>();
        
        cg.alpha = 0f;
        obj.localScale = new Vector3(0.2f, 0.2f, 1f);
        
        float waktuMulai = Time.time;
        while (Time.time < waktuMulai + durasi)
        {
            float progress = (Time.time - waktuMulai) / durasi;
            float t = progress - 1f;
            float s = 2.0f;
            float easeOutBack = (t * t * ((s + 1f) * t + s) + 1f);
            
            float scale = Mathf.Lerp(0.2f, 1f, easeOutBack);
            obj.localScale = new Vector3(scale, scale, 1f);
            cg.alpha = Mathf.Lerp(0f, 1f, progress);
            
            yield return null;
        }
        
        obj.localScale = Vector3.one;
        cg.alpha = 1f;
    }

    void LanjutKeDialogBerikutnya()
    {
        indeksDialog++;
        if (indeksDialog < percakapan.Length)
        {
            TampilkanDialogSekarang();
        }
        else
        {
            TutupDialog();
        }
    }

    void TutupDialog()
    {
        // Jalankan animasi fade out sebelum menutup
        StartCoroutine(FadeOutLaluTutup());
    }

    IEnumerator FadeOutLaluTutup()
    {
        if (panelBubleName != null)
        {
            CanvasGroup cg = panelBubleName.GetComponent<CanvasGroup>();
            if (cg == null) cg = panelBubleName.AddComponent<CanvasGroup>();

            float waktuMulai = Time.time;
            while (Time.time < waktuMulai + durasiTransisiTeks)
            {
                float progress = (Time.time - waktuMulai) / durasiTransisiTeks;
                cg.alpha = Mathf.Lerp(1f, 0f, progress);
                yield return null;
            }
            cg.alpha = 0f;
            panelBubleName.SetActive(false);
        }
    }
}
