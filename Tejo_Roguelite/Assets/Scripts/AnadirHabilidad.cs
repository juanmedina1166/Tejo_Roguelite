using UnityEngine;

public class PruebaAnadirHabilidad : MonoBehaviour
{
    // Arrastra tu Player aqu�
    public HabilidadManager habilidadManager;

    // Arrastra una de tus Habilidades de prueba (desde la carpeta Project) aqu�
    public Habilidad habilidadDePrueba;

    public void Anadir()
    {
        if (habilidadManager != null && habilidadDePrueba != null)
        {
            habilidadManager.AnadirHabilidad(habilidadDePrueba);
        }
        else
        {
            Debug.LogError("Asigna el HabilidadManager y una Habilidad de Prueba en el Inspector.");
        }
    }
}