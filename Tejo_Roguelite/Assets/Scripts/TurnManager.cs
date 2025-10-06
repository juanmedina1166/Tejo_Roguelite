using UnityEngine;

public class TurnManager : MonoBehaviour
{
    // Singleton: Un patrón para asegurar que solo haya una instancia de este manager.
    public static TurnManager instance;

    [SerializeField]
    private int numJugadores = 4;
    private int jugadorActual = 1; // Empezamos con el Jugador 1

    void Awake()
    {
        // Configuración del Singleton
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Devuelve el número del jugador actual (1, 2, 3, o 4).
    /// </summary>
    public int CurrentTurn()
    {
        return jugadorActual;
    }

    /// <summary>
    /// Pasa al siguiente turno.
    /// </summary>
    public void NextTurn()
    {
        jugadorActual++;
        if (jugadorActual > numJugadores)
        {
            jugadorActual = 1; // Vuelve al primer jugador
        }
        Debug.Log($"Turno del Jugador {jugadorActual}");
    }
}
