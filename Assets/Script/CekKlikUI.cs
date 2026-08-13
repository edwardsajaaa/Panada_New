using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CekKlikUI : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("🟢 TOMBOL BERHASIL DITEKAN: " + gameObject.name, gameObject);
        
        Image img = GetComponent<Image>();
        if (img != null)
        {
            img.color = new Color(Random.value, Random.value, Random.value);
        }
    }
}
