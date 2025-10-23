// Archivo: GameEvents.cs
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

    // --- NUEVO ---
    // Evento de inicio de apuntado
    public static event Action OnAimStarted;
    public static void TriggerAimStarted() => OnAimStarted?.Invoke();
    // --- FIN NUEVO ---

    // Eventos de Puntuaci�n (pasamos los puntos base)
    public static event Action<int> OnMechaExploded;
    public static void TriggerMechaExploded(int basePoints) => OnMechaExploded?.Invoke(basePoints);

    public static event Action<int> OnEmbocinada;
    public static void TriggerEmbocinada(int basePoints) => OnEmbocinada?.Invoke(basePoints);

    // --- NUEVO ---
    // Eventos de Puntuaci�n (pasamos el ID del jugador que anot�)
    public static event Action<int> OnMo�onaScored;
    public static void TriggerMo�onaScored(int playerID) => OnMo�onaScored?.Invoke(playerID);

    public static event Action<int> OnManoScored;
    public static void TriggerManoScored(int playerID) => OnManoScored?.Invoke(playerID);
    // --- FIN NUEVO ---
}