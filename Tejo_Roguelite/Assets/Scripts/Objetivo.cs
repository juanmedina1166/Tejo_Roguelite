using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Objetivo : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int puntosBase = 3; // ¡Puedes configurar cuántos puntos vale este objetivo!

    private bool haSidoGolpeado = false;

    private void OnCollisionEnter(Collision collision)
    {
        // Si ya fue golpeado o el objeto que choca no es un Tejo, no hagas nada.
        if (haSidoGolpeado || collision.gameObject.GetComponent<Tejo>() == null)
        {
            return;
        }

        haSidoGolpeado = true;
        Debug.Log($"¡Objetivo '{gameObject.name}' golpeado!");
        Debug.Log("==> PASO 1: Objetivo está a punto de lanzar el evento OnMechaExploded.");

        // El objetivo ANUNCIA que fue explotado y cuántos puntos base vale.
        // NO decide quién gana los puntos.
        GameEvents.TriggerMechaExploded(puntosBase);

        // Aquí puedes añadir un efecto de explosión o sonido.

        // Finalmente, el objeto se destruye.
        Destroy(gameObject);
    }
}
