using UnityEngine;

public class EfekKedipBergantian : MonoBehaviour
{
    [Header("Pengaturan Grup Objek")]
    [Tooltip("Masukkan gambar dering grup pertama (misal: yang bagian dalam)")]
    public GameObject[] grupA;
    
    [Tooltip("Masukkan gambar dering grup kedua (misal: yang bagian luar)")]
    public GameObject[] grupB;

    [Header("Pengaturan Waktu")]
    [Tooltip("Seberapa cepat animasi kedip bergantian (dalam detik)")]
    public float kecepatanKedip = 0.15f;

    [Header("Pengaturan Audio (Opsional)")]
    [Tooltip("Efek suara nada dering atau notifikasi yang dimainkan saat kedip")]
    public AudioClip suaraDering;
    [Tooltip("Komponen pemutar suara (jika kosong, akan dibuat otomatis)")]
    public AudioSource sumberSuara;
    [Tooltip("Centang jika suara harus di-loop terus menerus selama kedip berlangsung")]
    public bool loopSuara = true;

    private float timer;
    private bool statusGrupA = true;
    private bool sedangKedip = false;

    void OnEnable()
    {
        MulaiKedip();
    }

    void OnDisable()
    {
        HentikanKedip();
    }

    public void MulaiKedip()
    {
        Debug.Log("[EfekKedipBergantian] MulaiKedip dipanggil di objek: " + gameObject.name);
        sedangKedip = true;
        timer = kecepatanKedip;
        statusGrupA = true;
        UpdateVisual();

        if (suaraDering != null)
        {
            if (sumberSuara == null)
            {
                sumberSuara = gameObject.AddComponent<AudioSource>();
                sumberSuara.playOnAwake = false;
            }
            sumberSuara.clip = suaraDering;
            sumberSuara.loop = loopSuara;
            sumberSuara.volume = PengaturanAudioUI.GlobalSFXVolume;
            sumberSuara.Play();
        }
    }

    public void HentikanKedip()
    {
        Debug.Log("[EfekKedipBergantian] HentikanKedip dipanggil di objek: " + gameObject.name);
        sedangKedip = false;

        if (sumberSuara != null && sumberSuara.isPlaying)
        {
            sumberSuara.Stop();
        }
        
        if (grupA != null)
        {
            foreach (var obj in grupA)
                if (obj != null) obj.SetActive(false);
        }
        
        if (grupB != null)
        {
            foreach (var obj in grupB)
                if (obj != null) obj.SetActive(false);
        }
    }

    void Update()
    {
        if (!sedangKedip) return;

        timer -= Time.unscaledDeltaTime;
        if (timer <= 0f)
        {
            statusGrupA = !statusGrupA;
            UpdateVisual();
            
            timer = kecepatanKedip;
        }
    }

    void UpdateVisual()
    {
        if (grupA != null)
        {
            foreach (var obj in grupA)
                if (obj != null) obj.SetActive(statusGrupA);
        }

        if (grupB != null)
        {
            foreach (var obj in grupB)
                if (obj != null) obj.SetActive(!statusGrupA);
        }
    }
}
