// Archivo: Mecha.cs
using UnityEngine;

public class Mecha : MonoBehaviour
{
    [Header("Configuraci�n de Puntos")]
    [SerializeField] private int puntosBase = 6; // Puntos est�ndar por una mecha

    private bool haSidoGolpeada = false;

    private void OnCollisionEnter(Collision collision)
    {
        // Si ya fue golpeada en un frame anterior, no hagas nada.
        if (haSidoGolpeada) return;

        // Comprobamos si el objeto que nos golpe� tiene la etiqueta "Tejo".
        // �Es importante que tu prefab del tejo tenga esta etiqueta!
        if (collision.gameObject.CompareTag("Tejo"))
        {
            haSidoGolpeada = true;
            Debug.Log("�Mecha golpeada por un Tejo!");

            // Lanzamos el evento global, avisando que se anotaron 'puntosBase'.
            GameEvents.TriggerMechaExploded(puntosBase);

            // Opcional: Desactivamos la mecha para que no pueda ser golpeada de nuevo.
            // Podr�as reemplazar esto con una animaci�n de explosi�n y luego destruir el objeto.
            gameObject.SetActive(false);
        }
    }
}