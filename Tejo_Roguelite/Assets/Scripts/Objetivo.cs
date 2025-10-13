using UnityEngine;
using System;

/// <summary>
/// Colocar este script en cada objetivo del tablero.
/// Al colisionar con un Tejo, el objetivo se destruye.
/// Opcionalmente otorga 1 punto al jugador actual si existe GameManagerTejo y TurnManager.
/// Publica un evento estático para que otros sistemas (p. ej. IA) reaccionen al ser destruido.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Objetivo : MonoBehaviour
{
    // Evento: (objetivo, playerIndexQueLoDestruyo)
    public static event Action<Objetivo, int> OnObjetivoDestruido;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Tejo>() != null)
        {
            int playerIndex = -1;
            if (TurnManager.instance != null)
            {
                playerIndex = TurnManager.instance.CurrentPlayerIndex();
            }

            // Opcional: sumar punto al jugador actual
            if (GameManagerTejo.instance != null && playerIndex >= 0)
            {
                GameManagerTejo.instance.SumarPuntos(playerIndex, 1);
            }

            // Notificar a suscriptores antes de destruir
            OnObjetivoDestruido?.Invoke(this, playerIndex);

            Destroy(gameObject);
        }
    }
}
