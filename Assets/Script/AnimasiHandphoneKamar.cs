using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AnimasiHandphoneKamar : MonoBehaviour
{
    [Header("Waktu")]
    public float waktuTunggu = 4.5f;

    [Header("Visual")]
    public GameObject visualHpMenyala;
    
    [Header("Zoom & Event")]
    public bool gunakanZoomOtomatisScriptIni = true;
    
    public Camera kameraYangAkanDizoom;
    public RectTransform panelMejaYangAkanDizoom;
    public RectTransform titikFokusZoom;

    public float durasiZoom = 1.5f;
    public float targetSkalaZoom = 3f;

    [Space(10)]
    public UnityEvent eventSaatHpMenyala;

    void Start()
    {
        // matiin layar pas mulai
        if (visualHpMenyala != null)
            visualHpMenyala.SetActive(false);

        StartCoroutine(ProsesHandphoneMenyala());
    }

    IEnumerator ProsesHandphoneMenyala()
    {
        yield return new WaitForSeconds(waktuTunggu);

        if (visualHpMenyala != null)
            visualHpMenyala.SetActive(true);

        if (eventSaatHpMenyala != null)
            eventSaatHpMenyala.Invoke();

        // trigger klik biar tombol jalan otomatis
        Button tombolHp = GetComponent<Button>();
        if (tombolHp != null)
            tombolHp.onClick.Invoke();

        if (gunakanZoomOtomatisScriptIni)
        {
            if (kameraYangAkanDizoom != null)
                StartCoroutine(AnimasiZoomKamera());
            else if (panelMejaYangAkanDizoom != null)
                StartCoroutine(AnimasiZoomPanelMeja());
        }
    }

    IEnumerator AnimasiZoomPanelMeja()
    {
        RectTransform targetRect = titikFokusZoom != null ? titikFokusZoom : GetComponent<RectTransform>();
        Canvas canvas = GetComponentInParent<Canvas>();
        
        Vector3 posisiTengahLayar = canvas != null ? 
            canvas.transform.TransformPoint(canvas.GetComponent<RectTransform>().rect.center) : 
            new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);

        Vector3 skalaAwal = panelMejaYangAkanDizoom.localScale;
        Vector3 posisiAwal = panelMejaYangAkanDizoom.position;
        Vector3 skalaAkhir = skalaAwal * targetSkalaZoom;

        // cari target posisi
        Vector3 offsetTargetAwal = targetRect.position - posisiAwal;
        float rasioSkala = targetSkalaZoom / skalaAwal.x;
        Vector3 posisiAkhir = posisiTengahLayar - (offsetTargetAwal * rasioSkala);

        // cegah panel keluar dari batas layar
        Vector3[] canvasCorners = new Vector3[4];
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.GetWorldCorners(canvasCorners);
        float screenW = canvasCorners[2].x - canvasCorners[0].x;
        float screenH = canvasCorners[2].y - canvasCorners[0].y;

        Vector3[] panelCorners = new Vector3[4];
        panelMejaYangAkanDizoom.GetWorldCorners(panelCorners);
        float panelW = (panelCorners[2].x - panelCorners[0].x) * rasioSkala;
        float panelH = (panelCorners[2].y - panelCorners[0].y) * rasioSkala;

        float marginX = Mathf.Max(0, (panelW - screenW) / 2f);
        float marginY = Mathf.Max(0, (panelH - screenH) / 2f);

        Vector3 currentPanelCenter = (panelCorners[2] + panelCorners[0]) / 2f;
        Vector3 pivotToCenterAwal = currentPanelCenter - posisiAwal;
        Vector3 pivotToCenterAkhir = pivotToCenterAwal * rasioSkala;
        Vector3 neutralPos = posisiTengahLayar - pivotToCenterAkhir;

        posisiAkhir.x = Mathf.Clamp(posisiAkhir.x, neutralPos.x - marginX, neutralPos.x + marginX);
        posisiAkhir.y = Mathf.Clamp(posisiAkhir.y, neutralPos.y - marginY, neutralPos.y + marginY);

        // animasi jalan
        float elapsed = 0f;
        while (elapsed < durasiZoom)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / durasiZoom);

            panelMejaYangAkanDizoom.localScale = Vector3.Lerp(skalaAwal, skalaAkhir, t);
            panelMejaYangAkanDizoom.position = Vector3.Lerp(posisiAwal, posisiAkhir, t);

            yield return null;
        }
        
        panelMejaYangAkanDizoom.localScale = skalaAkhir;
        panelMejaYangAkanDizoom.position = posisiAkhir;
    }

    IEnumerator AnimasiZoomKamera()
    {
        float sizeAwal = kameraYangAkanDizoom.orthographicSize;
        float sizeAkhir = sizeAwal / targetSkalaZoom;

        Vector3 posisiAwalKamera = kameraYangAkanDizoom.transform.position;
        Vector3 posisiHP = transform.position;
        Vector3 posisiAkhirKamera = new Vector3(posisiHP.x, posisiHP.y, posisiAwalKamera.z);

        float elapsed = 0f;
        while (elapsed < durasiZoom)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / durasiZoom);

            kameraYangAkanDizoom.orthographicSize = Mathf.Lerp(sizeAwal, sizeAkhir, t);
            kameraYangAkanDizoom.transform.position = Vector3.Lerp(posisiAwalKamera, posisiAkhirKamera, t);
            
            yield return null;
        }

        kameraYangAkanDizoom.orthographicSize = sizeAkhir;
        kameraYangAkanDizoom.transform.position = posisiAkhirKamera;
    }
}
