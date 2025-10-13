using System.Collections.Generic;
using UnityEngine;

// La siguiente l�nea es opcional para el m�todo actual, pero es buena
// pr�ctica tenerla por si quieres hacer selecciones m�s complejas en el futuro.
using System.Linq;

[CreateAssetMenu(fileName = "HabilidadDatabase", menuName = "Tejo Rogulite/Habilidad Database")]
public class HabilidadDatabase : ScriptableObject
{
    // Esta es la lista principal donde debes arrastrar TODAS las
    // habilidades que existen en tu juego.
    public List<Habilidad> todasLasHabilidades;

    /// <summary>
    /// Devuelve una lista de habilidades aleatorias y �nicas de la base de datos.
    /// Es ideal para generar las opciones de recompensa.
    /// </summary>
    /// <param name="cantidad">El n�mero de habilidades aleatorias que deseas obtener.</param>
    /// <returns>Una nueva lista con las habilidades seleccionadas.</returns>
    public List<Habilidad> GetHabilidadesAleatorias(int cantidad)
    {
        // Creamos una copia de la lista original para poder manipularla
        // sin afectar la base de datos principal.
        List<Habilidad> listaTemporal = new List<Habilidad>(todasLasHabilidades);

        // Esta ser� la lista que devolveremos con nuestra selecci�n.
        List<Habilidad> seleccionFinal = new List<Habilidad>();

        // Nos aseguramos de no intentar sacar m�s habilidades de las que existen.
        if (cantidad > listaTemporal.Count)
        {
            Debug.LogWarning("Se pidieron m�s habilidades de las que existen en la base de datos. Se devolver�n todas las disponibles.");
            cantidad = listaTemporal.Count;
        }

        // Hacemos un bucle para escoger la cantidad de habilidades pedida.
        for (int i = 0; i < cantidad; i++)
        {
            // Escogemos un �ndice al azar de la lista temporal.
            int randomIndex = Random.Range(0, listaTemporal.Count);

            // A�adimos la habilidad de ese �ndice a nuestra lista final.
            seleccionFinal.Add(listaTemporal[randomIndex]);

            // �Muy importante! La quitamos de la lista temporal para que
            // no pueda ser seleccionada de nuevo en la siguiente vuelta del bucle.
            listaTemporal.RemoveAt(randomIndex);
        }

        return seleccionFinal;
    }
}
