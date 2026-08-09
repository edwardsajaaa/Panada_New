using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Events;

[System.Serializable]
public class BarisDialogBergantian
{
    public GameObject gelembungAktif;
    public TextMeshProUGUI tempatTeksDialog;

    [TextArea(2, 4)]
    public string kalimat;
}

public class DialogBergantian : MonoBehaviour
{
    [Header("Percakapan")]
    public BarisDialogBergantian[] percakapan;
    public float kecepatanKetik = 0.04f;
    public KeyCode tombolLanjut = KeyCode.F;

    [Header("Interact (Opsional)")]
    [Tooltip("Tarik objek NPC 1 (yang punya script PopupInteraksi) ke sini agar balon ? otomatis disembunyikan saat dialog")]
    public PopupInteraksi popupInteraksiNPC;

    [Header("Tutup Saat Menjauh")]
    public bool tutupSaatMenjauh = true;
    public float jarakMaksimal = 100f;
    public Transform playerTransform;
    public Transform pusatInteraksi;

    [Header("Posisi Pemain Saat Dialog (Opsional)")]
    [Tooltip("Buat GameObject kosong untuk titik berdiri Nathan. Jika diisi, Nathan akan otomatis pindah ke titik ini dan menghadap NPC.")]
    public Transform titikBerdiriPemain;

    [Header("Event Selesai")]
    public UnityEvent saatSemuaSelesai;

    int indeks = 0;
    bool sedangNgetik = false;
    bool aktif = false;
    bool menungguMenjauh = false;
    Coroutine proses;

    void Awake()
    {
        if (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }
    }

    void OnEnable()
    {
        aktif = false;
        sedangNgetik = false;
        menungguMenjauh = false;
        StartCoroutine(MulaiSetelahJeda());
    }

    IEnumerator MulaiSetelahJeda()
    {
        yield return null;
        yield return null;

        indeks = 0;
        MatikanSemuaGelembung();

        // Sembunyikan balon Interact (?) saat dialog mulai
        if (popupInteraksiNPC != null) popupInteraksiNPC.sembunyikanSementara = true;

        // Pindahkan dan hadapkan pemain ke NPC
        if (titikBerdiriPemain != null && playerTransform != null)
        {
            playerTransform.position = titikBerdiriPemain.position;

            if (pusatInteraksi != null)
            {
                PlayerMovementUI gerakUI = playerTransform.GetComponent<PlayerMovementUI>();
                if (gerakUI != null)
                {
                    bool npcDiKanan = pusatInteraksi.position.x > playerTransform.position.x;
                    gerakUI.Hadap(npcDiKanan);
                }
            }
        }

        Tampilkan(0);
        aktif = true;
    }

    void Update()
    {
        if (tutupSaatMenjauh && (aktif || menungguMenjauh))
        {
            Vector3 pusat = pusatInteraksi != null ? pusatInteraksi.position : transform.position;
            if (playerTransform != null && Vector2.Distance(pusat, playerTransform.position) > jarakMaksimal)
            {
                Tutup();
                return;
            }
        }

        if (!aktif) return;

        if (Input.GetKeyDown(tombolLanjut) || Input.GetKeyDown(KeyCode.Space))
        {
            if (sedangNgetik)
            {
                if (proses != null) StopCoroutine(proses);
                percakapan[indeks].tempatTeksDialog.text = percakapan[indeks].kalimat;
                sedangNgetik = false;
            }
            else
            {
                if (indeks < percakapan.Length - 1)
                {
                    indeks++;
                    MatikanSemuaGelembung();
                    Tampilkan(indeks);
                }
                else
                {
                    aktif = false;
                    if (tutupSaatMenjauh)
                    {
                        menungguMenjauh = true;
                        saatSemuaSelesai?.Invoke();
                    }
                    else
                    {
                        Tutup();
                    }
                }
            }
        }
    }

    void Tampilkan(int i)
    {
        var data = percakapan[i];
        if (data.gelembungAktif != null)
        {
            data.gelembungAktif.SetActive(true);

            // Perbaiki alpha saja, JANGAN ubah scale (biarkan scale asli dari Inspector)
            CanvasGroup cg = data.gelembungAktif.GetComponent<CanvasGroup>();
            if (cg != null) { cg.alpha = 1f; cg.blocksRaycasts = true; }
            CanvasGroup cgP = data.gelembungAktif.GetComponentInParent<CanvasGroup>();
            if (cgP != null) cgP.alpha = 1f;
        }
        if (data.tempatTeksDialog != null) data.tempatTeksDialog.text = "";
        proses = StartCoroutine(Ketik(data));
    }

    IEnumerator Ketik(BarisDialogBergantian data)
    {
        sedangNgetik = true;
        foreach (char c in data.kalimat.ToCharArray())
        {
            if (data.tempatTeksDialog != null) data.tempatTeksDialog.text += c;
            yield return new WaitForSeconds(kecepatanKetik);
        }
        sedangNgetik = false;
    }

    void MatikanSemuaGelembung()
    {
        foreach (var b in percakapan)
        {
            if (b.gelembungAktif != null && b.gelembungAktif.activeSelf)
                b.gelembungAktif.SetActive(false);
        }
    }

    void Tutup()
    {
        if (proses != null) StopCoroutine(proses);
        StopAllCoroutines();
        MatikanSemuaGelembung();
        aktif = false;
        menungguMenjauh = false;
        sedangNgetik = false;

        // Kembalikan balon Interact (?) saat dialog selesai
        if (popupInteraksiNPC != null) popupInteraksiNPC.sembunyikanSementara = false;

        gameObject.SetActive(false);
    }
}
