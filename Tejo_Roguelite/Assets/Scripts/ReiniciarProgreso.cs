using UnityEngine;
using UnityEngine.SceneManagement;

public class ReiniciarProgreso : MonoBehaviour
{
    public void ReiniciarTodo()
    {
        // 1. Borrar PlayerPrefs (opcional)
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // 2. Delegar el reinicio al MetaProgressionManager (si existe)
        if (MetaProgressionManager.instance != null)
        {
            MetaProgressionManager.instance.ReiniciarProgreso();
        }
        else
        {
            Debug.LogWarning("MetaProgressionManager no encontrado en la escena.");
        }

        Debug.Log("Progreso completamente reiniciado.");

        // 3. Recargar la escena actual para actualizar UI
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}