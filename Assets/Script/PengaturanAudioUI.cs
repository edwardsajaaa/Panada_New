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
        MuatDataDanUpdateUI(audioBGM);
        MuatDataDanUpdateUI(audioMain);
        MuatDataDanUpdateUI(audioVoice);
        MuatDataDanUpdateUI(audioSFX);
    }

    private void MuatDataDanUpdateUI(KategoriAudio kategori)
    {
        if (string.IsNullOrEmpty(kategori.namaPreferensi)) return;

        kategori.levelSaatIni = PlayerPrefs.GetInt(kategori.namaPreferensi, 5);
        
        PerbaruiVisualKotak(kategori);
        
        TerapkanVolumeKeMixer(kategori);
    }

    private void PerbaruiVisualKotak(KategoriAudio kategori)
    {
        if (kategori.blokVolume == null || kategori.blokVolume.Length == 0) return;

        for (int i = 0; i < kategori.blokVolume.Length; i++)
        {
            if (kategori.blokVolume[i] != null)
            {
                if (i < kategori.levelSaatIni)
                {
                    kategori.blokVolume[i].gameObject.SetActive(true);
                }
                else
                {
                    kategori.blokVolume[i].gameObject.SetActive(false);
                }
            }
        }
    }

    private void TerapkanVolumeKeMixer(KategoriAudio kategori)
    {
        if (masterMixer == null || string.IsNullOrEmpty(kategori.namaParameterMixer)) return;

        float volumeDecibel = -80f;
        
        if (kategori.levelSaatIni > 0)
        {
            float persentase = (float)kategori.levelSaatIni / 5f;
            volumeDecibel = Mathf.Log10(persentase) * 20f;
        }

        masterMixer.SetFloat(kategori.namaParameterMixer, volumeDecibel);
    }

    private void UbahVolume(KategoriAudio kategori, int perubahan)
    {
        kategori.levelSaatIni += perubahan;

        kategori.levelSaatIni = Mathf.Clamp(kategori.levelSaatIni, 0, 5);

        if (!string.IsNullOrEmpty(kategori.namaPreferensi))
        {
            PlayerPrefs.SetInt(kategori.namaPreferensi, kategori.levelSaatIni);
            PlayerPrefs.Save();
        }

        PerbaruiVisualKotak(kategori);
        TerapkanVolumeKeMixer(kategori);
    }

    public void TambahBGM()   { UbahVolume(audioBGM, 1); }
    public void KurangiBGM()  { UbahVolume(audioBGM, -1); }

    public void TambahMain()  { UbahVolume(audioMain, 1); }
    public void KurangiMain() { UbahVolume(audioMain, -1); }

    public void TambahVoice() { UbahVolume(audioVoice, 1); }
    public void KurangiVoice(){ UbahVolume(audioVoice, -1); }

    public void TambahSFX()   { UbahVolume(audioSFX, 1); }
    public void KurangiSFX()  { UbahVolume(audioSFX, -1); }
}
