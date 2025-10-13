using System.Collections.Generic;
using UnityEngine;

// La siguiente línea es opcional para el método actual, pero es buena
// práctica tenerla por si quieres hacer selecciones más complejas en el futuro.
using System.Linq;

[CreateAssetMenu(fileName = "HabilidadDatabase", menuName = "Tejo Rogulite/Habilidad Database")]
public class HabilidadDatabase : ScriptableObject
{
    // Esta es la lista principal donde debes arrastrar TODAS las
    // habilidades que existen en tu juego.
    public List<Habilidad> todasLasHabilidades;

    /// <summary>
    /// Devuelve una lista de habilidades aleatorias y únicas de la base de datos.
    /// Es ideal para generar las opciones de recompensa.
    /// </summary>
    /// <param name="cantidad">El número de habilidades aleatorias que deseas obtener.</param>
    /// <returns>Una nueva lista con las habilidades seleccionadas.</returns>
    public List<Habilidad> GetHabilidadesAleatorias(int cantidad)
    {
        // Creamos una copia de la lista original para poder manipularla
        // sin afectar la base de datos principal.
        List<Habilidad> listaTemporal = new List<Habilidad>(todasLasHabilidades);

        // Esta será la lista que devolveremos con nuestra selección.
        List<Habilidad> seleccionFinal = new List<Habilidad>();

        // Nos aseguramos de no intentar sacar más habilidades de las que existen.
        if (cantidad > listaTemporal.Count)
        {
            Debug.LogWarning("Se pidieron más habilidades de las que existen en la base de datos. Se devolverán todas las disponibles.");
            cantidad = listaTemporal.Count;
        }

        // Hacemos un bucle para escoger la cantidad de habilidades pedida.
        for (int i = 0; i < cantidad; i++)
        {
            // Escogemos un índice al azar de la lista temporal.
            int randomIndex = Random.Range(0, listaTemporal.Count);

            // Añadimos la habilidad de ese índice a nuestra lista final.
            seleccionFinal.Add(listaTemporal[randomIndex]);

            // ¡Muy importante! La quitamos de la lista temporal para que
            // no pueda ser seleccionada de nuevo en la siguiente vuelta del bucle.
            listaTemporal.RemoveAt(randomIndex);
        }

        return seleccionFinal;
    }
}
