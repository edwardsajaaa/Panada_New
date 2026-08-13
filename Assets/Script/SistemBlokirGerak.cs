using UnityEngine;

public class SistemBlokirGerak : MonoBehaviour
{
    public static int jumlahPanelTerbuka = 0;

    void OnEnable()
    {
        jumlahPanelTerbuka++;
    }

    void OnDisable()
    {
        jumlahPanelTerbuka--;
        if (jumlahPanelTerbuka < 0) jumlahPanelTerbuka = 0;
    }

    public static bool SedangBukaUI()
    {
        return jumlahPanelTerbuka > 0;
    }
}
