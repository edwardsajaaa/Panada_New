using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class AnimasiHandphoneKamar : MonoBehaviour
{
    [Header("Waktu")]
    public float waktuTunggu = 4.0f; // di set 4 detik sesuai request

    [Header("Visual Notifikasi")]
    public GameObject visualHpMenyala;
    
    [Header("Zoom Setup")]
    public RectTransform panelMeja;
    public RectTransform titikFokusZoom;
    public float durasiZoom = 1.5f;
    public float targetSkalaZoom = 3f;

    [Space(10)]
    public UnityEvent eventSaatHpMenyala;

    void Start()
    {
        // matiin layar pas mulai
        if (visualHpMenyala != null) visualHpMenyala.SetActive(false);
        StartCoroutine(ProsesMulai());
    }

    IEnumerator ProsesMulai()
    {
        // 1. Tunggu beberapa detik
        yield return new WaitForSeconds(waktuTunggu);

        // 2. Jalankan zoom meja dan tunggu sampai beres
        if (panelMeja != null)
            yield return StartCoroutine(AnimasiZoomPanel());

        // 3. Jeda 1 detik
        yield return new WaitForSeconds(1f);

        // 4. Nyalain notifikasi
        if (visualHpMenyala != null) visualHpMenyala.SetActive(true);
        if (eventSaatHpMenyala != null) eventSaatHpMenyala.Invoke();
    }

    IEnumerator AnimasiZoomPanel()
    {
        RectTransform target = titikFokusZoom != null ? titikFokusZoom : GetComponent<RectTransform>();
        Canvas canvas = GetComponentInParent<Canvas>();
        
        Vector3 layarTengah = canvas != null ? 
            canvas.transform.TransformPoint(canvas.GetComponent<RectTransform>().rect.center) : 
            new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);

        Vector3 skalaAwal = panelMeja.localScale;
        Vector3 posisiAwal = panelMeja.position;
        Vector3 skalaAkhir = skalaAwal * targetSkalaZoom;
        float rasio = targetSkalaZoom / skalaAwal.x;
        
        // hitung posisi biar target ada di tengah layar
        Vector3 offset = target.position - posisiAwal;
        Vector3 posisiAkhir = layarTengah - (offset * rasio);

        // cegah gambar panel jebol/keluar layar hitam (clamping)
        Vector3[] batas = new Vector3[4];
        canvas.GetComponent<RectTransform>().GetWorldCorners(batas);
        float lebarLayar = batas[2].x - batas[0].x;
        float tinggiLayar = batas[2].y - batas[0].y;

        panelMeja.GetWorldCorners(batas);
        float lebarPanel = (batas[2].x - batas[0].x) * rasio;
        float tinggiPanel = (batas[2].y - batas[0].y) * rasio;

        float batasX = Mathf.Max(0, (lebarPanel - lebarLayar) / 2f);
        float batasY = Mathf.Max(0, (tinggiPanel - tinggiLayar) / 2f);

        Vector3 pusatPanel = (batas[2] + batas[0]) / 2f;
        Vector3 offsetPusat = (pusatPanel - posisiAwal) * rasio;
        Vector3 posisiNetral = layarTengah - offsetPusat;

        posisiAkhir.x = Mathf.Clamp(posisiAkhir.x, posisiNetral.x - batasX, posisiNetral.x + batasX);
        posisiAkhir.y = Mathf.Clamp(posisiAkhir.y, posisiNetral.y - batasY, posisiNetral.y + batasY);

        // animasi jalan
        float waktu = 0f;
        while (waktu < durasiZoom)
        {
            waktu += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, waktu / durasiZoom);

            panelMeja.localScale = Vector3.Lerp(skalaAwal, skalaAkhir, t);
            panelMeja.position = Vector3.Lerp(posisiAwal, posisiAkhir, t);
            yield return null;
        }
        
        panelMeja.localScale = skalaAkhir;
        panelMeja.position = posisiAkhir;
    }
}
