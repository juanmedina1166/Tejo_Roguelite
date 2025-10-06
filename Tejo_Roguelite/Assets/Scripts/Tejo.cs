using UnityEngine;

// Se asegura de que el objeto siempre tenga un Rigidbody
[RequireComponent(typeof(Rigidbody))]
public class Tejo : MonoBehaviour
{
    private Rigidbody rb;
    private bool fueLanzado = false;
    private bool haTerminado = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Usamos FixedUpdate para trabajar con físicas de forma consistente
    void FixedUpdate()
    {
        // Si no ha sido lanzado o ya se detuvo, no hacemos nada
        if (!fueLanzado || haTerminado)
        {
            return;
        }

        // La forma más fiable de saber si un objeto físico se detuvo
        // es preguntar si está "durmiendo" (IsSleeping).
        if (rb.IsSleeping())
        {
            // Marcamos que ya terminó para no llamar al GameManager múltiples veces
            haTerminado = true;

            Debug.Log("El tejo se ha detenido en la posición: " + transform.position);

            // Le avisamos al GameManager que este tejo específico terminó su movimiento
            if (GameManagerTejo.instance != null)
            {
                GameManagerTejo.instance.TejoTermino(this);
            }
        }
    }

    // Este método debe ser llamado justo después de que el tejo es lanzado
    // para que empiece a comprobar si se ha detenido.
    public void ActivarDeteccion()
    {
        fueLanzado = true;
    }
}