using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class PengaturanAudioUI : MonoBehaviour
{
    [System.Serializable]
    public class KategoriAudio
    {
        [Header("Pengaturan Audio")]
        [Tooltip("Nama untuk menyimpan data, contoh: Vol_Main, Vol_BGM")]
        public string namaPreferensi;
        [Tooltip("Nama parameter yang di-expose di AudioMixer, contoh: MasterVol")]
        public string namaParameterMixer; 
        
        [Header("Pengaturan UI (Dari bawah ke atas)")]
        [Tooltip("Masukkan ke-5 image kotak dari bawah ke atas")]
        public Image[] blokVolume; 
        
        [Tooltip("Gambar kotak saat menyala (misal: kotak warna Pink)")]
        public Sprite spriteAktif;
        [Tooltip("Gambar kotak saat mati (misal: kotak warna Hitam)")]
        public Sprite spriteMati;
        
        [HideInInspector]
        public int levelSaatIni = 5;
    }

    [Header("Referensi Audio Mixer (Nanti dimasukkan)")]
    public AudioMixer masterMixer;

    [Header("Kategori Volume")]
    public KategoriAudio audioBGM;
    public KategoriAudio audioMain;
    public KategoriAudio audioVoice;
    public KategoriAudio audioSFX;

    void Start()
    {
        // Inisialisasi dan muat data saat menu pengaturan dibuka
        MuatDataDanUpdateUI(audioBGM);
        MuatDataDanUpdateUI(audioMain);
        MuatDataDanUpdateUI(audioVoice);
        MuatDataDanUpdateUI(audioSFX);
    }

    private void MuatDataDanUpdateUI(KategoriAudio kategori)
    {
        if (string.IsNullOrEmpty(kategori.namaPreferensi)) return;

        // Ambil data dari PlayerPrefs (default 5 jika pemain baru pertama kali main)
        kategori.levelSaatIni = PlayerPrefs.GetInt(kategori.namaPreferensi, 5);
        
        // Update UI Warna Kotak
        PerbaruiVisualKotak(kategori);
        
        // Terapkan ke Mixer
        TerapkanVolumeKeMixer(kategori);
    }

    private void PerbaruiVisualKotak(KategoriAudio kategori)
    {
        if (kategori.blokVolume == null || kategori.blokVolume.Length == 0) return;

        // Cek satu per satu ke-5 kotak dari bawah ke atas
        for (int i = 0; i < kategori.blokVolume.Length; i++)
        {
            if (kategori.blokVolume[i] != null)
            {
                // Kembalikan warna ke putih murni agar gambar asli tidak terlihat kusam
                kategori.blokVolume[i].color = Color.white;

                // Jika urutan kotak (mulai dari 0) di bawah level saat ini, pakai gambar aktif!
                if (i < kategori.levelSaatIni)
                {
                    if (kategori.spriteAktif != null) 
                        kategori.blokVolume[i].sprite = kategori.spriteAktif;
                }
                else
                {
                    // Jika di atas level, pakai gambar mati (hitam)
                    if (kategori.spriteMati != null) 
                        kategori.blokVolume[i].sprite = kategori.spriteMati;
                }
            }
        }
    }

    private void TerapkanVolumeKeMixer(KategoriAudio kategori)
    {
        if (masterMixer == null || string.IsNullOrEmpty(kategori.namaParameterMixer)) return;

        // Standar Audio Game Profesional:
        // Konversi level 0-5 menjadi Decibel (dB) logaritmik untuk Unity AudioMixer
        float volumeDecibel = -80f; // -80dB artinya MUTE total
        
        if (kategori.levelSaatIni > 0)
        {
            // Level 1 = 20%, Level 5 = 100%
            float persentase = (float)kategori.levelSaatIni / 5f;
            // Rumus Logaritmik Audio: log10(value) * 20
            volumeDecibel = Mathf.Log10(persentase) * 20f;
        }

        // Kirim angka desibel ke AudioMixer
        masterMixer.SetFloat(kategori.namaParameterMixer, volumeDecibel);
    }

    private void UbahVolume(KategoriAudio kategori, int perubahan)
    {
        kategori.levelSaatIni += perubahan;

        // Batasi level agar tidak pernah kurang dari 0 atau lebih dari 5
        kategori.levelSaatIni = Mathf.Clamp(kategori.levelSaatIni, 0, 5);

        // Simpan ke sistem agar tidak reset saat keluar game
        if (!string.IsNullOrEmpty(kategori.namaPreferensi))
        {
            PlayerPrefs.SetInt(kategori.namaPreferensi, kategori.levelSaatIni);
            PlayerPrefs.Save();
        }

        // Langsung Update Visual & Suara saat itu juga
        PerbaruiVisualKotak(kategori);
        TerapkanVolumeKeMixer(kategori);
    }

    // =======================================================
    // FUNGSI-FUNGSI DI BAWAH INI YANG DIMASUKKAN KE TOMBOL UI
    // =======================================================

    public void TambahBGM()   { UbahVolume(audioBGM, 1); }
    public void KurangiBGM()  { UbahVolume(audioBGM, -1); }

    public void TambahMain()  { UbahVolume(audioMain, 1); }
    public void KurangiMain() { UbahVolume(audioMain, -1); }

    public void TambahVoice() { UbahVolume(audioVoice, 1); }
    public void KurangiVoice(){ UbahVolume(audioVoice, -1); }

    public void TambahSFX()   { UbahVolume(audioSFX, 1); }
    public void KurangiSFX()  { UbahVolume(audioSFX, -1); }
}
