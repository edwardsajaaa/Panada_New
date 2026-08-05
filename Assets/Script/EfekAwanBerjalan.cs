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
        // Secara otomatis mencari semua awan yang ada di dalam objek ini (Awan, Awan (1), Awan (2), dst)
        int jumlahAwan = transform.childCount;
        awanAnak = new Transform[jumlahAwan];
        
        for (int i = 0; i < jumlahAwan; i++)
        {
            awanAnak[i] = transform.GetChild(i);
        }
    }

    void Update()
    {
        // Gerakkan setiap awan satu per satu setiap frame
        for (int i = 0; i < awanAnak.Length; i++)
        {
            Transform awan = awanAnak[i];
            
            // Geser posisi X ke arah kiri
            awan.localPosition -= new Vector3(kecepatan * Time.deltaTime, 0, 0);

            // Jika awan sudah melewati batas kiri layar
            if (awan.localPosition.x <= batasKiri)
            {
                // Lempar (teleportasi) awan tersebut kembali ke ujung kanan layar
                awan.localPosition = new Vector3(posisiSpawnKanan, awan.localPosition.y, awan.localPosition.z);
            }
        }
    }
}
