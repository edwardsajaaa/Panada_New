using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

// Sistem BGM Global yang tidak hancur saat pindah Scene.
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
    private bool pakaiSource1 = true;

    private Coroutine transisiBerjalan;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource1 = gameObject.AddComponent<AudioSource>();
        audioSource2 = gameObject.AddComponent<AudioSource>();

        audioSource1.loop = true;
        audioSource2.loop = true;
        
        audioSource1.playOnAwake = false;
        audioSource2.playOnAwake = false;

        audioSource1.volume = 0f;
        audioSource2.volume = 0f;

        if (bgmMixerGroup != null)
        {
            audioSource1.outputAudioMixerGroup = bgmMixerGroup;
            audioSource2.outputAudioMixerGroup = bgmMixerGroup;
        }
    }

    // Memutar lagu baru. Jika lagu yang diminta sama dengan yang sedang main, ia tidak akan mengulang.
    public void PutarLagu(AudioClip laguBaru, float durasiFade = 2f, float targetVolume = 1f)
    {
        if (laguBaru == null) return;

        AudioSource sourceAktif = pakaiSource1 ? audioSource1 : audioSource2;
        AudioSource sourceBerikutnya = pakaiSource1 ? audioSource2 : audioSource1;

        if (sourceAktif.clip == laguBaru && sourceAktif.isPlaying)
        {
            if (sourceAktif.volume != targetVolume)
            {
                sourceAktif.volume = targetVolume;
            }
            return;
        }

        sourceBerikutnya.clip = laguBaru;
        sourceBerikutnya.volume = 0f;
        sourceBerikutnya.Play();

        if (transisiBerjalan != null) StopCoroutine(transisiBerjalan);
        transisiBerjalan = StartCoroutine(ProsesCrossfade(sourceAktif, sourceBerikutnya, durasiFade, targetVolume));

        pakaiSource1 = !pakaiSource1;
    }

    // Menghentikan BGM yang sedang bermain secara perlahan
    public void HentikanBGM(float durasiFade = 2f)
    {
        AudioSource sourceAktif = pakaiSource1 ? audioSource1 : audioSource2;
        if (transisiBerjalan != null) StopCoroutine(transisiBerjalan);
        transisiBerjalan = StartCoroutine(ProsesFadeOut(sourceAktif, durasiFade));
    }

    // Mengubah volume secara instan (Dipanggil dari menu Pengaturan Audio)
    public void SetGlobalVolume(float vol)
    {
        maxVolume = Mathf.Clamp01(vol);
        if (audioSource1 != null && audioSource1.isPlaying && transisiBerjalan == null) audioSource1.volume = maxVolume;
        if (audioSource2 != null && audioSource2.isPlaying && transisiBerjalan == null) audioSource2.volume = maxVolume;
    }

    private IEnumerator ProsesCrossfade(AudioSource sourceLama, AudioSource sourceBaru, float durasi, float targetVol)
    {
        float timer = 0f;
        float volLama = sourceLama.volume;
        
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
