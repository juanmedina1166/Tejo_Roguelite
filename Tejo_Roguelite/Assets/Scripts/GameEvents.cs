using System;
using UnityEngine;

public static class GameEvents
{
    // Define un evento para cuando un enemigo es derrotado
    public static event Action OnTurnStarted;
    public static void TriggerTurnStarted() => OnTurnStarted?.Invoke();

    public static event Action OnRoundEnded;
    public static void TriggerRoundEnded() => OnRoundEnded?.Invoke();

    // Evento de Lanzamiento
    public static event Action<GameObject> OnTejoThrown; // Pasamos el tejo que fue lanzado
    public static void TriggerTejoThrown(GameObject tejoInstance) => OnTejoThrown?.Invoke(tejoInstance);

    // Eventos de Puntuación (pasamos los puntos base)
    public static event Action<int> OnMechaExploded;
    public static void TriggerMechaExploded(int basePoints) => OnMechaExploded?.Invoke(basePoints);

    public static event Action<int> OnEmbocinada;
    public static void TriggerEmbocinada(int basePoints) => OnEmbocinada?.Invoke(basePoints);

    // ... y así sucesivamente para Moñona, Mano, etc.

    // ... ¡Crea todos los eventos que necesites para tus triggers!
}
