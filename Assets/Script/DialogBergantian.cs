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

/// <summary>
/// Mengatur dialog dua arah antar gelembung karakter secara bergantian.
/// </summary>
public class DialogBergantian : MonoBehaviour
{
    [Header("Percakapan")]
    public BarisDialogBergantian[] percakapan;
    public float kecepatanKetik = 0.04f;
    public KeyCode tombolLanjut = KeyCode.F;

    [Header("Tutup Saat Menjauh")]
    public bool tutupSaatMenjauh = true;
    public float jarakMaksimal = 100f;
    public Transform playerTransform;
    public Transform pusatInteraksi;

    [Header("Event Selesai")]
    public UnityEvent saatSemuaSelesai;

    int indeks = 0;
    bool sedangNgetik = false;
    bool aktif = false;
    bool menungguMenjauh = false;
    bool menungguLepas = false; // Tunggu tombol F dilepas dulu sebelum boleh input
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
        indeks = 0;
        aktif = true;
        menungguMenjauh = false;
        menungguLepas = true; // KUNCI: Jangan terima input sampai pemain MELEPAS tombol F
        MatikanSemuaGelembung();
        Tampilkan(0);
    }

    void Update()
    {
        // Cek jarak untuk menutup dialog jika pemain menjauh
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

        // Tunggu tombol F dilepas dulu sebelum menerima input baru
        if (menungguLepas)
        {
            if (!Input.GetKey(tombolLanjut))
                menungguLepas = false;
            return;
        }

        if (Input.GetKeyDown(tombolLanjut) || Input.GetKeyDown(KeyCode.Space))
        {
            menungguLepas = true; // Setelah ditekan, tunggu dilepas lagi

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
            CanvasGroup cg = data.gelembungAktif.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1f;
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
        MatikanSemuaGelembung();
        aktif = false;
        menungguMenjauh = false;
        sedangNgetik = false;
        gameObject.SetActive(false);
    }
}
