using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[InitializeOnLoad]
public class FixAnimatorError
{
    // Script ini akan berjalan otomatis setiap kali Unity selesai meng-compile code
    static FixAnimatorError()
    {
        EditorApplication.delayCall += TutupJendelaAnimator;
    }

    [MenuItem("Tools/Fix Error Animator")]
    public static void TutupJendelaAnimator()
    {
        // Mencari semua jendela editor yang terbuka
        EditorWindow[] semuaJendela = Resources.FindObjectsOfTypeAll<EditorWindow>();
        
        foreach (EditorWindow jendela in semuaJendela)
        {
            // Jika ada jendela Animator atau Graph yang terbuka di background (penyebab utama error Edge.WakeUp)
            if (jendela.GetType().Name.Contains("AnimatorControllerTool") || 
                jendela.GetType().Name.Contains("Graph") || 
                jendela.GetType().Name.Contains("Animator"))
            {
                jendela.Close();
                Debug.Log("<color=green><b>[Auto-Fix]</b></color> Jendela Animator yang menyebabkan error Edge.WakeUp telah ditutup secara otomatis oleh sistem.");
            }
        }
    }
}
#endif
