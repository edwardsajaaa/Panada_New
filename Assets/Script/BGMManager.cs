using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Sistem BGM Global yang tidak hancur saat pindah Scene.
/// Menangani Crossfade antar lagu dan terhubung dengan AudioMixer.
/// </summary>
public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance { get; private set; }

    [Header("Pengaturan Audio")]
    [Tooltip("Masukkan AudioMixerGroup untuk BGM (agar terhubung dengan menu pengaturan volume)")]
    public AudioMixerGroup bgmMixerGroup;
    
    [Tooltip("Batas maksimal volume internal (0 sampai 1)")]
    public float maxVolume = 1f;

    private AudioSource audioSource1;
    private AudioSource audioSource2;
    private bool pakaiSource1 = true; // Menandakan source mana yang sedang aktif

    private Coroutine transisiBerjalan;

    void Awake()
    {
        // Pastikan hanya ada SATU BGMManager di seluruh game
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Hancurkan duplikat jika pindah ke scene yang juga punya BGMManager
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Bikin abadi antar scene

        // Siapkan 2 AudioSource untuk keperluan crossfade
        audioSource1 = gameObject.AddComponent<AudioSource>();
        audioSource2 = gameObject.AddComponent<AudioSource>();

        audioSource1.loop = true;
        audioSource2.loop = true;
        
        audioSource1.playOnAwake = false;
        audioSource2.playOnAwake = false;

        audioSource1.volume = 0f;
        audioSource2.volume = 0f;

        // Hubungkan ke Mixer BGM jika ada
        if (bgmMixerGroup != null)
        {
            audioSource1.outputAudioMixerGroup = bgmMixerGroup;
            audioSource2.outputAudioMixerGroup = bgmMixerGroup;
        }
    }

    /// <summary>
    /// Memutar lagu baru. Jika lagu yang diminta sama dengan yang sedang main, ia tidak akan mengulang.
    /// </summary>
    public void PutarLagu(AudioClip laguBaru, float durasiFade = 2f, float targetVolume = 1f)
    {
        if (laguBaru == null) return;

        AudioSource sourceAktif = pakaiSource1 ? audioSource1 : audioSource2;
        AudioSource sourceBerikutnya = pakaiSource1 ? audioSource2 : audioSource1;

        // Cegah lagu mengulang (restart) jika ternyata lagunya sama
        if (sourceAktif.clip == laguBaru && sourceAktif.isPlaying)
        {
            // Jika lagunya sama tapi volumenya diubah, sesuaikan volumenya
            if (sourceAktif.volume != targetVolume)
            {
                sourceAktif.volume = targetVolume;
            }
            return;
        }

        // Siapkan lagu di source berikutnya
        sourceBerikutnya.clip = laguBaru;
        sourceBerikutnya.volume = 0f;
        sourceBerikutnya.Play();

        // Mulai transisi Crossfade
        if (transisiBerjalan != null) StopCoroutine(transisiBerjalan);
        transisiBerjalan = StartCoroutine(ProsesCrossfade(sourceAktif, sourceBerikutnya, durasiFade, targetVolume));

        // Tukar status source
        pakaiSource1 = !pakaiSource1;
    }

    /// <summary>
    /// Menghentikan BGM yang sedang bermain secara perlahan
    /// </summary>
    public void HentikanBGM(float durasiFade = 2f)
    {
        AudioSource sourceAktif = pakaiSource1 ? audioSource1 : audioSource2;
        if (transisiBerjalan != null) StopCoroutine(transisiBerjalan);
        transisiBerjalan = StartCoroutine(ProsesFadeOut(sourceAktif, durasiFade));
    }

    private IEnumerator ProsesCrossfade(AudioSource sourceLama, AudioSource sourceBaru, float durasi, float targetVol)
    {
        float timer = 0f;
        float volLama = sourceLama.volume;
        
        // Batasi targetVol maksimal tidak lebih besar dari maxVolume manager
        float volumeAkhir = Mathf.Min(targetVol, maxVolume);

        while (timer < durasi)
        {
            timer += Time.deltaTime;
            float persentase = timer / durasi;

            sourceLama.volume = Mathf.Lerp(volLama, 0f, persentase);
            sourceBaru.volume = Mathf.Lerp(0f, volumeAkhir, persentase);
            
            yield return null;
        }

        sourceLama.volume = 0f;
        sourceBaru.volume = volumeAkhir;
        sourceLama.Stop();
    }

    private IEnumerator ProsesFadeOut(AudioSource source, float durasi)
    {
        float timer = 0f;
        float volAwal = source.volume;

        while (timer < durasi)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(volAwal, 0f, timer / durasi);
            yield return null;
        }

        source.volume = 0f;
        source.Stop();
    }
}
