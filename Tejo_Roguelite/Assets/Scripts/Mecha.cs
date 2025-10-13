// Archivo: Mecha.cs
using UnityEngine;

public class Mecha : MonoBehaviour
{
    [Header("Configuración de Puntos")]
    [SerializeField] private int puntosBase = 6; // Puntos estándar por una mecha

    private bool haSidoGolpeada = false;

    private void OnCollisionEnter(Collision collision)
    {
        // Si ya fue golpeada en un frame anterior, no hagas nada.
        if (haSidoGolpeada) return;

        // Comprobamos si el objeto que nos golpeó tiene la etiqueta "Tejo".
        // ¡Es importante que tu prefab del tejo tenga esta etiqueta!
        if (collision.gameObject.CompareTag("Tejo"))
        {
            haSidoGolpeada = true;
            Debug.Log("¡Mecha golpeada por un Tejo!");

            // Lanzamos el evento global, avisando que se anotaron 'puntosBase'.
            GameEvents.TriggerMechaExploded(puntosBase);

            // Opcional: Desactivamos la mecha para que no pueda ser golpeada de nuevo.
            // Podrías reemplazar esto con una animación de explosión y luego destruir el objeto.
            gameObject.SetActive(false);
        }
    }
}