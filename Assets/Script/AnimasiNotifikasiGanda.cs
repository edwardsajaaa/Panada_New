using UnityEngine;
using System.Collections;

public class AnimasiNotifikasiGanda : MonoBehaviour
{
    [Header("Referensi Notifikasi")]
    public GameObject notifPertama;
    public GameObject notifKedua;

    [Header("Pengaturan Durasi & Jeda")]
    public float durasiPopupPertama = 0.3f;
    public float jedaAntarNotif = 0.5f;
    public float durasiPopupKedua = 0.35f;

    [Header("Pengaturan Skala")]
    public Vector3 skalaAkhirNotifPertama = Vector3.one;
    [Tooltip("Dibuat lebih besar sedikit sesuai permintaan")]
    public Vector3 skalaAkhirNotifKedua = new Vector3(1.15f, 1.15f, 1f);

    void OnEnable()
    {
        if (notifPertama != null) notifPertama.SetActive(false);
        if (notifKedua != null) notifKedua.SetActive(false);
        
        StartCoroutine(ProsesNotifGanda());
    }

    IEnumerator ProsesNotifGanda()
    {
        if (notifPertama != null)
        {
            notifPertama.SetActive(true);
            notifPertama.transform.localScale = Vector3.zero;
            
            float waktu = 0f;
            while (waktu < durasiPopupPertama)
            {
                waktu += Time.deltaTime;
                float progress = waktu / durasiPopupPertama;
                
                float t = progress - 1f;
                float s = 1.70158f;
                float easeOutBack = (t * t * ((s + 1f) * t + s) + 1f);

                notifPertama.transform.localScale = Vector3.LerpUnclamped(Vector3.zero, skalaAkhirNotifPertama, easeOutBack);
                yield return null;
            }
            notifPertama.transform.localScale = skalaAkhirNotifPertama;
        }

        yield return new WaitForSeconds(jedaAntarNotif);

        if (notifKedua != null)
        {
            notifKedua.SetActive(true);
            notifKedua.transform.localScale = Vector3.zero;
            
            float waktu = 0f;
            while (waktu < durasiPopupKedua)
            {
                waktu += Time.deltaTime;
                float progress = waktu / durasiPopupKedua;
                
                float t = progress - 1f;
                float s = 1.70158f;
                float easeOutBack = (t * t * ((s + 1f) * t + s) + 1f);

                notifKedua.transform.localScale = Vector3.LerpUnclamped(Vector3.zero, skalaAkhirNotifKedua, easeOutBack);
                yield return null;
            }
            notifKedua.transform.localScale = skalaAkhirNotifKedua;
        }

        if (notifPertama != null)
        {
            notifPertama.SetActive(false);
        }
    }
}
