using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CekKlikUI : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("🟢 TOMBOL BERHASIL DITEKAN: " + gameObject.name, gameObject);
        
        // Ubah warna acak untuk memberikan respon visual yang sangat jelas
        Image img = GetComponent<Image>();
        if (img != null)
        {
            img.color = new Color(Random.value, Random.value, Random.value);
        }
    }
}
