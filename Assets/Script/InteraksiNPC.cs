using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

// Script untuk interaksi NPC di area 2D (Canvas UI).
[RequireComponent(typeof(Image))] // Agar bisa diklik, butuh komponen Image (bisa transparan)
public class InteraksiNPC : MonoBehaviour, IPointerClickHandler
{
    [Header("Referensi Dialog")]
    [Tooltip("Drag objek DialogPopup milik NPC ini ke sini")]
    public GameObject dialogPopup;

    [Header("Pengaturan Animasi Popup")]
    [Tooltip("Durasi animasi popup muncul/hilang (dalam detik)")]
    public float durasiAnimasi = 0.3f;

    [Tooltip("Tipe animasi popup")]
    public TipeAnimasi tipeAnimasi = TipeAnimasi.MembesarDariKecil;

    public enum TipeAnimasi
    {
        MembesarDariKecil,
        MelayanDariBawah,
        MelayanDariAtas
    }

    [Header("Pengaturan Jarak (Opsional)")]
    [Tooltip("Centang jika ingin NPC hanya bisa diinteraksi saat pemain berada di dekatnya")]
    public bool perlucCekJarak = false;
    [Tooltip("Jarak maksimal pemain dari NPC agar bisa diinteraksi")]
    public float jarakInteraksi = 3f;
    [Tooltip("Transform pemain (otomatis dicari jika kosong)")]
    public Transform playerTransform;

    private bool dialogSedangAktif = false;
    private Vector3 skalaAsli;
    private CanvasGroup dialogGroup;
    private RectTransform dialogRect;
    private Vector2 posisiAsliDialog;
    private PopupInteraksi popup;
    private Coroutine animasiAktif;

    void Start()
    {
        popup = GetComponent<PopupInteraksi>();

        // Pastikan Image di Interact ini bisa di-raycast (diklik) walau transparan
        Image img = GetComponent<Image>();
        if (img != null)
        {
            img.raycastTarget = true;
            // Jika belum ada sprite/gambar, buat transparan agar tidak mengganggu visual
            if (img.sprite == null)
            {
                img.color = new Color(1, 1, 1, 0);
            }
        }

        if (playerTransform == null)
        {
            PlayerMovementUI playerUI = FindAnyObjectByType<PlayerMovementUI>();
            if (playerUI != null) playerTransform = playerUI.transform;
        }

        if (dialogPopup != null)
        {
            skalaAsli = dialogPopup.transform.localScale;
            dialogRect = dialogPopup.GetComponent<RectTransform>();
            if (dialogRect != null) posisiAsliDialog = dialogRect.anchoredPosition;

            dialogGroup = dialogPopup.GetComponent<CanvasGroup>();
            if (dialogGroup == null) dialogGroup = dialogPopup.AddComponent<CanvasGroup>();

            dialogPopup.SetActive(false);
        }
    }

    void Update()
    {
        // Jika dialog sedang aktif, dan fitur cek jarak dinyalakan, tutup dialog otomatis jika pemain menjauh
        if (dialogSedangAktif && perlucCekJarak && playerTransform != null)
        {
            float jarak = Vector3.Distance(transform.position, playerTransform.position);
            if (jarak > jarakInteraksi)
            {
                TutupDialog();
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (dialogPopup == null) return;

        if (perlucCekJarak && playerTransform != null)
        {
            float jarak = Vector3.Distance(transform.position, playerTransform.position);
            if (jarak > jarakInteraksi) return;
        }

        ToggleDialog();
    }

    // Buka/Tutup dialog secara otomatis (Sangat berguna untuk dipanggil lewat Event Tombol F)
    public void ToggleDialog()
    {
        if (dialogSedangAktif) TutupDialog();
        else BukaDialog();
    }

    public void BukaDialog()
    {
        if (animasiAktif != null) StopCoroutine(animasiAktif);

        dialogPopup.SetActive(true);
        dialogSedangAktif = true;
        
        if (popup != null) popup.sembunyikanSementara = true;

        animasiAktif = StartCoroutine(AnimasiBuka());
    }

    public void TutupDialog()
    {
        if (animasiAktif != null) StopCoroutine(animasiAktif);

        if (popup != null) popup.sembunyikanSementara = false;

        animasiAktif = StartCoroutine(AnimasiTutup());
    }

    IEnumerator AnimasiBuka()
    {
        float timer = 0f;

        switch (tipeAnimasi)
        {
            case TipeAnimasi.MembesarDariKecil:
                dialogPopup.transform.localScale = Vector3.zero;
                dialogGroup.alpha = 0f;
                while (timer < durasiAnimasi)
                {
                    timer += Time.deltaTime;
                    float t = timer / durasiAnimasi;
                    float kurva = 1f - Mathf.Pow(1f - t, 3f);
                    float skalaKurva = kurva * 1.05f;
                    if (t > 0.8f) skalaKurva = Mathf.Lerp(1.05f, 1f, (t - 0.8f) / 0.2f);
                    
                    dialogPopup.transform.localScale = skalaAsli * skalaKurva;
                    dialogGroup.alpha = Mathf.Clamp01(t * 2f);
                    yield return null;
                }
                break;

            case TipeAnimasi.MelayanDariBawah:
                dialogGroup.alpha = 0f;
                if (dialogRect != null)
                    dialogRect.anchoredPosition = posisiAsliDialog + Vector2.down * 30f;
                while (timer < durasiAnimasi)
                {
                    timer += Time.deltaTime;
                    float t = timer / durasiAnimasi;
                    float kurva = 1f - Mathf.Pow(1f - t, 3f);
                    dialogGroup.alpha = kurva;
                    if (dialogRect != null)
                        dialogRect.anchoredPosition = Vector2.Lerp(
                            posisiAsliDialog + Vector2.down * 30f, 
                            posisiAsliDialog, kurva);
                    yield return null;
                }
                break;

            case TipeAnimasi.MelayanDariAtas:
                dialogGroup.alpha = 0f;
                if (dialogRect != null)
                    dialogRect.anchoredPosition = posisiAsliDialog + Vector2.up * 30f;
                while (timer < durasiAnimasi)
                {
                    timer += Time.deltaTime;
                    float t = timer / durasiAnimasi;
                    float kurva = 1f - Mathf.Pow(1f - t, 3f);
                    dialogGroup.alpha = kurva;
                    if (dialogRect != null)
                        dialogRect.anchoredPosition = Vector2.Lerp(
                            posisiAsliDialog + Vector2.up * 30f, 
                            posisiAsliDialog, kurva);
                    yield return null;
                }
                break;
        }

        dialogPopup.transform.localScale = skalaAsli;
        dialogGroup.alpha = 1f;
        if (dialogRect != null) dialogRect.anchoredPosition = posisiAsliDialog;
    }

    IEnumerator AnimasiTutup()
    {
        float timer = 0f;

        switch (tipeAnimasi)
        {
            case TipeAnimasi.MembesarDariKecil:
                while (timer < durasiAnimasi)
                {
                    timer += Time.deltaTime;
                    float t = timer / durasiAnimasi;
                    float kurva = Mathf.Pow(t, 2f);
                    dialogPopup.transform.localScale = Vector3.Lerp(skalaAsli, Vector3.zero, kurva);
                    dialogGroup.alpha = 1f - kurva;
                    yield return null;
                }
                break;

            case TipeAnimasi.MelayanDariBawah:
            case TipeAnimasi.MelayanDariAtas:
                Vector2 arahKeluar = tipeAnimasi == TipeAnimasi.MelayanDariBawah 
                    ? Vector2.down * 30f : Vector2.up * 30f;
                while (timer < durasiAnimasi)
                {
                    timer += Time.deltaTime;
                    float t = timer / durasiAnimasi;
                    float kurva = Mathf.Pow(t, 2f);
                    dialogGroup.alpha = 1f - kurva;
                    if (dialogRect != null)
                        dialogRect.anchoredPosition = Vector2.Lerp(
                            posisiAsliDialog, posisiAsliDialog + arahKeluar, kurva);
                    yield return null;
                }
                break;
        }

        dialogPopup.SetActive(false);
        dialogSedangAktif = false;
        if (dialogRect != null) dialogRect.anchoredPosition = posisiAsliDialog;
    }
}
