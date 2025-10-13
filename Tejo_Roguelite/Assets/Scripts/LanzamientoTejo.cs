using UnityEngine;

// Obliga a que este GameObject tenga siempre un componente Rigidbody
[RequireComponent(typeof(Rigidbody))]
public class LanzamientoTejo : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 escalaInicial;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        escalaInicial = transform.localScale;

        // Desactivar la gravedad para que el tejo no caiga antes del lanzamiento.
        if (rb != null)
        {
            rb.useGravity = false;
        }
    }

    /// <summary>
    /// Inicia el lanzamiento del tejo aplicando una fuerza física (compatibilidad con uso actual).
    /// </summary>
    public void Iniciar(Vector3 origen, Vector3 direccionLanzamiento, float fuerza)
    {
        transform.position = origen;
        transform.localScale = escalaInicial;

        if (rb == null)
        {
            Debug.LogWarning("LanzamientoTejo: Rigidbody no encontrado.");
            return;
        }

        // Activar gravedad justo antes del lanzamiento y resetear velocidades
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(direccionLanzamiento.normalized * fuerza, ForceMode.Impulse);
    }

    /// <summary>
    /// Inicia el lanzamiento estableciendo directamente la velocidad inicial (útil para la IA).
    /// </summary>
    public void IniciarConVelocidad(Vector3 origen, Vector3 velocidadInicial)
    {
        transform.position = origen;
        transform.localScale = escalaInicial;

        if (rb == null)
        {
            Debug.LogWarning("LanzamientoTejo: Rigidbody no encontrado.");
            return;
        }

        // Activar gravedad justo antes de fijar la velocidad y resetear velocidades previas
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.linearVelocity = velocidadInicial;
    }
}