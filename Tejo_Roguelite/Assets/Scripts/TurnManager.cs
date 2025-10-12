using System;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    // Singleton: Un patrón para asegurar que solo haya una instancia de este manager.
    public static TurnManager instance;

    [SerializeField]
    private int numJugadores = 2; // Ahora solo Humano y IA
    private int jugadorActual = 1; // 1 = Humano, 2 = IA

    /// <summary>
    /// Evento que se dispara cuando cambia el turno.
    /// Recibe el número de jugador actual (1-based).
    /// </summary>
    public event Action<int> OnTurnChanged;

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

    // Anunciamos el turno inicial para que quien esté suscrito (IA, UI...) reciba el estado.
    void Start()
    {
        OnTurnChanged?.Invoke(jugadorActual);
    }

    /// <summary>
    /// Devuelve el número del jugador actual (1 o 2).
    /// </summary>
    public int CurrentTurn()
    {
        return jugadorActual;
    }

    /// <summary>
    /// Devuelve el índice del jugador actual en base cero (0 o 1).
    /// Útil para acceder a arrays como puntajes[].
    /// </summary>
    public int CurrentPlayerIndex()
    {
        return Mathf.Clamp(jugadorActual - 1, 0, numJugadores - 1);
    }

    /// <summary>
    /// Indica si es el turno de la IA.
    /// </summary>
    public bool IsAITurn()
    {
        return jugadorActual == 2;
    }

    /// <summary>
    /// Indica si es el turno del jugador humano.
    /// </summary>
    public bool IsHumanTurn()
    {
        return jugadorActual == 1;
    }

    /// <summary>
    /// Forzar un turno específico (1-based). Dispara el evento OnTurnChanged.
    /// </summary>
    public void SetTurn(int jugador)
    {
        if (jugador < 1 || jugador > numJugadores)
        {
            Debug.LogWarning($"TurnManager: intento de SetTurn inválido: {jugador}");
            return;
        }

        jugadorActual = jugador;
        Debug.Log($"Turno forzado al Jugador {jugadorActual}");
        OnTurnChanged?.Invoke(jugadorActual);
    }

    /// <summary>
    /// Pasa al siguiente turno. Alterna entre 1 y 2.
    /// </summary>
    public void NextTurn()
    {
        jugadorActual++;
        if (jugadorActual > numJugadores)
        {
            jugadorActual = 1; // Vuelve al primer jugador
        }

        Debug.Log($"Turno del Jugador {jugadorActual}");
        OnTurnChanged?.Invoke(jugadorActual);
    }
}
