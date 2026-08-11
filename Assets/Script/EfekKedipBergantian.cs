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

    private float timer;
    private bool statusGrupA = true;

    void OnEnable()
    {
        // Reset timer dan status setiap kali HP / objek dinyalakan
        timer = kecepatanKedip;
        statusGrupA = true;
        UpdateVisual();
    }

    void Update()
    {
        timer -= Time.unscaledDeltaTime;
        if (timer <= 0f)
        {
            // Tukar status nyala/mati
            statusGrupA = !statusGrupA;
            UpdateVisual();
            
            // Reset timer
            timer = kecepatanKedip;
        }
    }

    void UpdateVisual()
    {
        // Nyalakan/matikan Grup A
        foreach (var obj in grupA)
        {
            if (obj != null) obj.SetActive(statusGrupA);
        }

        // Nyalakan/matikan Grup B berlawanan dengan Grup A
        foreach (var obj in grupB)
        {
            if (obj != null) obj.SetActive(!statusGrupA);
        }
    }
}
