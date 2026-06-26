using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class AnimasiHoverUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Pengaturan Efek Hover")]
    [Tooltip("Target skala saat kursor berada di atas objek (misal 1.08 untuk membesar 8%)")]
    public float targetSkalaHover = 1.08f;
    [Tooltip("Pergeseran posisi ke atas dalam pixel saat di-hover")]
    public float geserAtasHover = 12f;
    [Tooltip("Rotasi miring tambahan dalam derajat saat di-hover")]
    public float rotasiMiringHover = 2.5f;
    [Tooltip("Kecepatan transisi animasi (semakin besar semakin cepat)")]
    public float kecepatanAnimasi = 14f;
    [Tooltip("Apakah objek dibawa ke lapisan paling depan saat di-hover agar tidak tertutup objek lain")]
    public bool bawaKeDepanSaatHover = true;

    private Vector3 skalaAwal;
    private Vector2 posisiAwal;
    private Quaternion rotasiAwal;
    private int indeksSiblingAwal;
    private bool sedangHover = false;
    private bool sudahInisialisasi = false;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // Matikan transisi warna abu-abu bawaan tombol Unity agar tidak kusam
        Button btn = GetComponent<Button>();
        if (btn != null) btn.transition = Selectable.Transition.None;
    }

    void Start()
    {
        // Sengaja dibiarkan kosong agar posisi aktual direkam saat kursor pertama kali masuk (setelah transisi intro selesai)
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Jangan jalankan efek jika UI sedang menyeret barang (DraggableUI)
        if (DraggableUI.isInteractingWithUI) return;

        // Rekam posisi aktual objek tepat sebelum di-hover agar posisi desain Editor 100% akurat
        if (!sedangHover)
        {
            skalaAwal = transform.localScale;
            if (rectTransform != null) posisiAwal = rectTransform.anchoredPosition;
            rotasiAwal = transform.localRotation;
            indeksSiblingAwal = transform.GetSiblingIndex();
            sudahInisialisasi = true;
        }

        sedangHover = true;
        if (bawaKeDepanSaatHover)
        {
            indeksSiblingAwal = transform.GetSiblingIndex();
            transform.SetAsLastSibling();

            // Garansi 100% tombol Next tetap berada di lapisan paling atas agar tidak tertutup koran yang di-hover!
            if (Pengenalan.instansiTombolNext != null && Pengenalan.instansiTombolNext.transform.parent == transform.parent)
            {
                Pengenalan.instansiTombolNext.transform.SetAsLastSibling();
            }
        }
        StopAllCoroutines();
        StartCoroutine(TransisiHover(true));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!sudahInisialisasi) return;
        sedangHover = false;
        if (bawaKeDepanSaatHover)
        {
            transform.SetSiblingIndex(indeksSiblingAwal);
        }
        StopAllCoroutines();
        StartCoroutine(TransisiHover(false));
    }

    void OnDisable()
    {
        if (!sudahInisialisasi) return;
        sedangHover = false;
        transform.localScale = skalaAwal != Vector3.zero ? skalaAwal : Vector3.one;
        if (rectTransform != null) rectTransform.anchoredPosition = posisiAwal;
        transform.localRotation = rotasiAwal;
    }

    IEnumerator TransisiHover(bool hover)
    {
        Vector3 targetSkala = hover ? skalaAwal * targetSkalaHover : skalaAwal;
        Vector2 targetPosisi = hover ? posisiAwal + new Vector2(0f, geserAtasHover) : posisiAwal;
        Quaternion targetRotasi = hover ? rotasiAwal * Quaternion.Euler(0f, 0f, rotasiMiringHover) : rotasiAwal;

        while (true)
        {
            float step = Time.deltaTime * kecepatanAnimasi;
            transform.localScale = Vector3.Lerp(transform.localScale, targetSkala, step);
            if (rectTransform != null) rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPosisi, step);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotasi, step);

            if (Vector3.Distance(transform.localScale, targetSkala) < 0.001f &&
                (rectTransform == null || Vector2.Distance(rectTransform.anchoredPosition, targetPosisi) < 0.1f))
            {
                transform.localScale = targetSkala;
                if (rectTransform != null) rectTransform.anchoredPosition = targetPosisi;
                transform.localRotation = targetRotasi;
                break;
            }
            yield return null;
        }
    }
}
