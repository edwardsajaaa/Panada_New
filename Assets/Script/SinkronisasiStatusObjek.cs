using UnityEngine;

public class SinkronisasiStatusObjek : MonoBehaviour
{
    [Tooltip("Objek yang akan dibuat berlawanan statusnya (misal: PixelOverlay)")]
    public GameObject objekKebalikan;

    void OnEnable()
    {
        if (objekKebalikan != null)
        {
            // Saat panel ini (Story) NYALA, matikan PixelOverlay
            objekKebalikan.SetActive(false);
        }
    }

    void OnDisable()
    {
        if (objekKebalikan != null)
        {
            // Saat panel ini (Story) MATI, nyalakan kembali PixelOverlay
            objekKebalikan.SetActive(true);
        }
    }
}
