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
}
