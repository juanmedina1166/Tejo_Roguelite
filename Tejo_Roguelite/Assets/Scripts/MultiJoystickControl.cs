using UnityEngine;

public class MultiJoystickControl : MonoBehaviour
{
    //  Variable añadida para guardar el tejo actual del jugador
    private Tejo tejoActual;

    /// <summary>
    /// Prepara la escena para la siguiente ronda de lanzamientos.
    /// </summary>
    public void PrepareForNextRound()
    {
        Debug.Log("Preparando la siguiente ronda...");

        // 1. Limpiar los tejos de la ronda anterior.
        LimpiarTejosAnteriores();

        // 2. Reactivar los controles para el siguiente jugador.
        var controlJugador = FindObjectOfType<ControlJugador>();
        if (controlJugador != null)
        {
            controlJugador.PrepararNuevoTejo();
        }
    }

    //  Asigna el tejo actual al control del jugador
    public void AsignarTejoActual(Tejo nuevoTejo)
    {
        tejoActual = nuevoTejo;
        Debug.Log(" Tejo actual asignado al control del jugador.");
    }

    // (Opcional) Método para acceder al tejo desde otros scripts
    public Tejo GetTejoActual()
    {
        return tejoActual;
    }

    private void LimpiarTejosAnteriores()
    {
        // Busca todos los objetos con el script 'Tejo' en la escena
        Tejo[] tejosEnEscena = FindObjectsOfType<Tejo>();

        // Los destruye
        foreach (Tejo tejo in tejosEnEscena)
        {
            Destroy(tejo.gameObject);
        }
        Debug.Log($"Se limpiaron {tejosEnEscena.Length} tejos de la escena.");
    }
}
