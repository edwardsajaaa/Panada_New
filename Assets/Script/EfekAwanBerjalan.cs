using UnityEngine;

public class EfekAwanBerjalan : MonoBehaviour
{
    [Header("Pengaturan Gerakan")]
    [Tooltip("Seberapa cepat awan bergerak ke kiri. Semakin besar angka, semakin cepat.")]
    public float kecepatan = 50f;

    [Header("Pengaturan Mengulang (Loop)")]
    [Tooltip("Posisi X (kiri) di mana awan dianggap sudah hilang dari layar dan harus diulang.")]
    public float batasKiri = -1500f;
    
    [Tooltip("Posisi X (kanan) tempat awan akan dimunculkan kembali.")]
    public float posisiSpawnKanan = 1500f;

    private Transform[] awanAnak;

    void Start()
    {
        int jumlahAwan = transform.childCount;
        awanAnak = new Transform[jumlahAwan];
        
        for (int i = 0; i < jumlahAwan; i++)
        {
            awanAnak[i] = transform.GetChild(i);
        }
    }

    void Update()
    {
        for (int i = 0; i < awanAnak.Length; i++)
        {
            Transform awan = awanAnak[i];
            
            awan.localPosition -= new Vector3(kecepatan * Time.deltaTime, 0, 0);

            if (awan.localPosition.x <= batasKiri)
            {
                awan.localPosition = new Vector3(posisiSpawnKanan, awan.localPosition.y, awan.localPosition.z);
            }
        }
    }
}
