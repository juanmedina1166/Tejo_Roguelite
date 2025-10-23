using System.Collections.Generic;
using UnityEngine;

public class GestorDeNivelesIA : MonoBehaviour
{
    [Header("Lista de IAs disponibles")]
    public List<AIController> todasLasIAs;

    [Header("Referencia especial de Estefa (tutorial)")]
    public AIController estefa;

    private List<AIController> iasUsadas = new List<AIController>();

    private void Start()
    {
        if (GameLevelManager.instance == null)
        {
            Debug.LogError(" No se encontró GameLevelManager en la escena.");
            return;
        }

        // Usamos el nivel actual definido por GameLevelManager
        int nivel = GameLevelManager.instance.nivelActual;
        IniciarNivel(nivel);
    }

    public void IniciarNivel(int nivel)
    {
        // Desactivar todas las IA antes de iniciar el nuevo nivel
        foreach (var ia in todasLasIAs)
            ia.gameObject.SetActive(false);

        AIController iaSeleccionada = null;
        PersonajeIAData datosIA = GameLevelManager.instance.ObtenerDatosIA();

        // Seleccionar la IA correspondiente al personaje actual
        if (datosIA.nombre == "Estefa")
        {
            iaSeleccionada = estefa;
        }
        else
        {
            iaSeleccionada = todasLasIAs.Find(ia => ia.name.Contains(datosIA.nombre));

            // Si no se encuentra, usar una aleatoria como respaldo
            if (iaSeleccionada == null)
                iaSeleccionada = ObtenerIAAleatoria(new List<AIController> { estefa });
        }

        // Activar IA y registrar
        if (iaSeleccionada != null)
        {
            iaSeleccionada.gameObject.SetActive(true);
            if (!iasUsadas.Contains(iaSeleccionada))
                iasUsadas.Add(iaSeleccionada);

            // Aplicar la dificultad
            iaSeleccionada.ConfigurarDificultad(datosIA.dificultad.ToString());

            Debug.Log($" Personaje IA: {datosIA.nombre} | Nivel: {nivel} | Dificultad: {datosIA.dificultad}");
        }
        else
        {
            Debug.LogWarning($" No se pudo seleccionar una IA para el nivel {nivel}");
        }
    }

    private AIController ObtenerIAAleatoria(List<AIController> excluir)
    {
        List<AIController> posibles = new List<AIController>(todasLasIAs);
        posibles.RemoveAll(ia => excluir.Contains(ia));

        if (posibles.Count == 0)
        {
            Debug.LogWarning(" No hay IAs disponibles para seleccionar.");
            return null;
        }

        return posibles[Random.Range(0, posibles.Count)];
    }
}