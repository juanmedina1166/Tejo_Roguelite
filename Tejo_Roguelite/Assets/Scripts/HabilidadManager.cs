using System.Collections.Generic;
using UnityEngine;
using System; // Necesario para 'Action'

public class HabilidadManager : MonoBehaviour
{
    [Header("Configuración de la Baraja")]
    [SerializeField] private int tamanoMaximoBaraja = 8; // Límite de habilidades

    // Usamos una Lista porque es fácil añadir y quitar elementos.
    private List<Habilidad> barajaDeHabilidades = new List<Habilidad>();

    // Evento para notificar a la UI cuando la baraja cambie.
    public static event Action OnBarajaCambio;

    // Método para que otros scripts obtengan una copia segura de la baraja
    public List<Habilidad> GetBaraja()
    {
        return new List<Habilidad>(barajaDeHabilidades);
    }
    private void OnEnable()
    {
        // Le decimos al sistema de eventos: "Cuando ocurra OnMechaExploded, llama a mi método OnMechaExploded_Handler"
        GameEvents.OnMechaExploded += OnMechaExploded_Handler;
    }

    private void OnDisable()
    {
        // Le decimos al sistema de eventos que ya no necesitamos que nos avise.
        // Esto es muy importante para evitar errores.
        GameEvents.OnMechaExploded -= OnMechaExploded_Handler;
    }

    /// <summary>
    /// Intenta añadir una habilidad a la baraja.
    /// </summary>
    /// <returns>True si la habilidad fue añadida, false si la baraja está llena.</returns>
    public bool AnadirHabilidad(Habilidad nuevaHabilidad)
    {
        // Comprobamos si hay espacio en la baraja
        if (barajaDeHabilidades.Count >= tamanoMaximoBaraja)
        {
            Debug.LogWarning("¡La baraja está llena! No se pudo añadir: " + nuevaHabilidad.nombre);
            return false; // No se pudo añadir
        }

        barajaDeHabilidades.Add(nuevaHabilidad);
        Debug.Log(nuevaHabilidad.nombre + " fue añadida a la baraja.");

        // Avisamos a quien esté escuchando (la UI) que la baraja ha cambiado.
        OnBarajaCambio?.Invoke();

        return true; // Éxito
    }

    /// <summary>
    /// Quita una habilidad específica de la baraja.
    /// </summary>
    public void QuitarHabilidad(Habilidad habilidadParaQuitar)
    {
        if (barajaDeHabilidades.Contains(habilidadParaQuitar))
        {
            barajaDeHabilidades.Remove(habilidadParaQuitar);
            Debug.Log(habilidadParaQuitar.nombre + " fue quitada de la baraja.");

            // Avisamos a la UI que la baraja ha cambiado.
            OnBarajaCambio?.Invoke();
        }
    }
   
    private void OnMechaExploded_Handler(int puntosBase)
    {
        Debug.Log("==> PASO 2: HabilidadManager ha escuchado el evento.");
        GameManagerTejo.instance.MarcarMechaExplotada(); // Esto avisa a la ronda que no hay "mano"

        // 1. Le preguntamos al TurnManager de quién es el turno (0 para humano, 1 para IA).
        int idDelGanador = TurnManager.instance.CurrentPlayerIndex();

        // 2. ? ¡CAMBIO CLAVE! En lugar de sumar puntos, REGISTRAMOS la mecha en el GameManager.
        // El GameManager decidirá si fue Mecha (3) o Moñona (9) cuando el tejo se detenga.
        GameManagerTejo.instance.RegistrarMecha(idDelGanador, puntosBase);

        // 3. Revisamos si el jugador actual tiene habilidades de bonus.
        // ESTO SÍ SE QUEDA, ya que son puntos EXTRA de la habilidad.
        if (idDelGanador == 0) // Asumiendo que solo el jugador humano tiene habilidades
        {
            foreach (var habilidad in barajaDeHabilidades)
            {
                if (habilidad.nombre == "Mecha Explosiva")
                {
                    int puntosExtra = (int)habilidad.valorNumerico1;
                    // Si la tiene, le decimos al GameManager que sume los puntos EXTRA.
                    GameManagerTejo.instance.SumarPuntos(idDelGanador, puntosExtra);
                }
            }
        }
    }
}
