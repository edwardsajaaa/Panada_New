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
        // 1. Bersihkan Console secara otomatis
        var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
        if (logEntries != null)
        {
            var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            if (clearMethod != null) clearMethod.Invoke(null, null);
        }

        // 2. Mencari semua jendela editor yang terbuka
        EditorWindow[] semuaJendela = Resources.FindObjectsOfTypeAll<EditorWindow>();
        
        foreach (EditorWindow jendela in semuaJendela)
        {
            string namaJendela = jendela.GetType().Name;
            // Tutup jendela Animator dan Animation yang sering memicu error Edge.WakeUp
            if (namaJendela.Contains("AnimatorControllerTool") || 
                namaJendela.Contains("Graph") || 
                namaJendela.Contains("Animator") ||
                namaJendela.Contains("AnimationWindow"))
            {
                jendela.Close();
                Debug.Log("<color=green><b>[Auto-Fix]</b></color> Jendela penyebab error telah ditutup dan Console dibersihkan.");
            }
        }
    }
}
#endif
