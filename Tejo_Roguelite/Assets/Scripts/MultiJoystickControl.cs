using UnityEngine;

public class MultiJoystickControl : MonoBehaviour
{
    /// <summary>
    /// Prepara la escena para la siguiente ronda de lanzamientos.
    /// </summary>
    public void PrepareForNextRound()
    {
        // Esta función es llamada por el GameManager al cambiar de turno.
        Debug.Log("Preparando la siguiente ronda...");

        // Aquí iría la lógica para:
        // 1. Limpiar los tejos de la ronda anterior.
        LimpiarTejosAnteriores();

        // 2. Reactivar los controles para el siguiente jugador.
        // (Por ejemplo, si tienes un objeto 'ControlJugadorMouse', lo reactivarías aquí)
        var controlJugador = FindObjectOfType<ControlJugador>();
        if (controlJugador != null)
        {
            controlJugador.PrepararNuevoTejo();
        }
    }

    private void LimpiarTejosAnteriores()
    {
        // Busca todos los objetos con el script 'Tejo' en la escena
        Tejo[] tejosEnEscena = FindObjectsOfType<Tejo>();

        // Y los destruye
        foreach (Tejo tejo in tejosEnEscena)
        {
            Destroy(tejo.gameObject);
        }
        Debug.Log($"Se limpiaron {tejosEnEscena.Length} tejos de la escena.");
    }
}
