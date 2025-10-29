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

        //  Selección basada en nombre (nivel actual define el personaje)
        if (datosIA.nombre == "Estefa")
        {
            iaSeleccionada = estefa;
        }
        else
        {
            iaSeleccionada = todasLasIAs.Find(ia => ia.name.Contains(datosIA.nombre));

            if (iaSeleccionada == null)
                iaSeleccionada = ObtenerIAAleatoria(new List<AIController> { estefa });
        }

        //  Activar y configurar
        if (iaSeleccionada != null)
        {
            iaSeleccionada.gameObject.SetActive(true);
            if (!iasUsadas.Contains(iaSeleccionada))
                iasUsadas.Add(iaSeleccionada);

            // Aplicar los datos del GameLevelManager
            iaSeleccionada.AplicarDatosIA(datosIA);

            //  DEBUG INFORMATIVO COMPLETO
            Debug.Log($" IA configurada correctamente  " +
                      $"Personaje: {datosIA.nombre} | Nivel: {nivel} | Dificultad: {datosIA.dificultad} | " +
                      $"Fallo: {datosIA.chanceFallar} | Delay: {datosIA.decisionDelay}s");
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