using System.Collections.Generic;
using UnityEngine;
using System; // Necesario para 'Action'

public class HabilidadManager : MonoBehaviour
{
    [Header("Configuraci�n de la Baraja")]
    [SerializeField] private int tamanoMaximoBaraja = 8; // L�mite de habilidades

    // Usamos una Lista porque es f�cil a�adir y quitar elementos.
    private List<Habilidad> barajaDeHabilidades = new List<Habilidad>();

    // Evento para notificar a la UI cuando la baraja cambie.
    public static event Action OnBarajaCambio;

    // M�todo para que otros scripts obtengan una copia segura de la baraja
    public List<Habilidad> GetBaraja()
    {
        return new List<Habilidad>(barajaDeHabilidades);
    }

    /// <summary>
    /// Intenta a�adir una habilidad a la baraja.
    /// </summary>
    /// <returns>True si la habilidad fue a�adida, false si la baraja est� llena.</returns>
    public bool AnadirHabilidad(Habilidad nuevaHabilidad)
    {
        // Comprobamos si hay espacio en la baraja
        if (barajaDeHabilidades.Count >= tamanoMaximoBaraja)
        {
            Debug.LogWarning("�La baraja est� llena! No se pudo a�adir: " + nuevaHabilidad.nombre);
            return false; // No se pudo a�adir
        }

        barajaDeHabilidades.Add(nuevaHabilidad);
        Debug.Log(nuevaHabilidad.nombre + " fue a�adida a la baraja.");

        // Avisamos a quien est� escuchando (la UI) que la baraja ha cambiado.
        OnBarajaCambio?.Invoke();

        return true; // �xito
    }

    /// <summary>
    /// Quita una habilidad espec�fica de la baraja.
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
