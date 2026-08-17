using UnityEngine;
using System.Collections.Generic;

public class KameraParallaxUI : MonoBehaviour
{
    [Header("Referensi Utama")]
    [Tooltip("Karakter player yang akan diikuti oleh kamera")]
    public RectTransform player;
    
    [Tooltip("Grup yang berisi tanah, bangunan, dan player. (Grup ini yang akan digeser berlawanan arah biar seolah-olah kamera bergerak)")]
    public RectTransform grupMap;

    [Header("Pengaturan Kamera (Batas Layar)")]
    [Tooltip("Kecepatan kamera mengikuti player")]
    public float kecepatanKamera = 5f;
    [Tooltip("Batas mentok kiri kamera (agar tidak melihat luar map)")]
    public float batasKiriKamera = 0f;
    [Tooltip("Batas mentok kanan kamera (sesuaikan dengan panjang map)")]
    public float batasKananKamera = 3000f;

    [System.Serializable]
    public class LayerParallax
    {
        public RectTransform objekLatar;
        [Tooltip("0 = Diam (ikut layar terus), 1 = Bergerak sama cepat dengan tanah, 0.5 = Setengah kecepatan")]
        [Range(0f, 1f)]
        public float kecepatanParallax = 0.5f;
        
        [HideInInspector] public float posisiAwalX;
    }

    [Header("Efek Parallax")]
    [Tooltip("Masukkan latar belakang seperti langit atau awan yang letaknya di LUAR Grup Map")]
    public List<LayerParallax> layerParallax;

    private float targetPosisiX;
    private float kameraPosisiX;
    private float posisiAwalGrupY;

    void Start()
    {
        if (player == null || grupMap == null) return;

        posisiAwalGrupY = grupMap.anchoredPosition.y;

        // Simpan posisi awal layer parallax sebelum digeser
        foreach (var layer in layerParallax)
        {
            if (layer.objekLatar != null)
            {
                layer.posisiAwalX = layer.objekLatar.anchoredPosition.x;
            }
        }
        
        // Langsung paskan (snap) kamera ke posisi target yang dibatasi saat awal game,
        // supaya tidak terjadi efek 'kamera terseret dari luar map' di detik pertama.
        kameraPosisiX = Mathf.Clamp(player.anchoredPosition.x, batasKiriKamera, batasKananKamera);
        
        // Langsung terapkan posisi agar frame pertama langsung benar
        grupMap.anchoredPosition = new Vector2(-kameraPosisiX, posisiAwalGrupY);
        foreach (var layer in layerParallax)
        {
            if (layer.objekLatar != null)
            {
                float geserX = -kameraPosisiX * layer.kecepatanParallax;
                layer.objekLatar.anchoredPosition = new Vector2(layer.posisiAwalX + geserX, layer.objekLatar.anchoredPosition.y);
            }
        }
    }

    void LateUpdate()
    {
        if (player == null || grupMap == null) return;

        // Kamera mentargetkan posisi player, tapi dibatasi batas map
        targetPosisiX = Mathf.Clamp(player.anchoredPosition.x, batasKiriKamera, batasKananKamera);

        // Gerakkan "kamera" (sebenarnya map yang digeser) secara halus
        kameraPosisiX = Mathf.Lerp(kameraPosisiX, targetPosisiX, Time.deltaTime * kecepatanKamera);

        // 1. Geser Grup Map ke arah berlawanan
        grupMap.anchoredPosition = new Vector2(-kameraPosisiX, posisiAwalGrupY);

        // 2. Geser layer parallax (Langit / Awan)
        foreach (var layer in layerParallax)
        {
            if (layer.objekLatar != null)
            {
                // Rumus Parallax UI
                float geserX = -kameraPosisiX * layer.kecepatanParallax;
                
                layer.objekLatar.anchoredPosition = new Vector2(
                    layer.posisiAwalX + geserX, 
                    layer.objekLatar.anchoredPosition.y
                );
            }
        }
    }
}
