using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Objetivo : MonoBehaviour
{
    [Header("Configuraci�n")]
    [SerializeField] private int puntosBase = 3; // �Puedes configurar cu�ntos puntos vale este objetivo!

    private bool haSidoGolpeado = false;

    private void OnCollisionEnter(Collision collision)
    {
        // Si ya fue golpeado o el objeto que choca no es un Tejo, no hagas nada.
        if (haSidoGolpeado || collision.gameObject.GetComponent<Tejo>() == null)
        {
            return;
        }

        haSidoGolpeado = true;
        Debug.Log($"�Objetivo '{gameObject.name}' golpeado!");
        Debug.Log("==> PASO 1: Objetivo est� a punto de lanzar el evento OnMechaExploded.");

        // El objetivo ANUNCIA que fue explotado y cu�ntos puntos base vale.
        // NO decide qui�n gana los puntos.
        GameEvents.TriggerMechaExploded(puntosBase);

        // Aqu� puedes a�adir un efecto de explosi�n o sonido.

        // Finalmente, el objeto se destruye.
        Destroy(gameObject);
    }
}
