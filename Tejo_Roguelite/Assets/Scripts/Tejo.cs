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

    // Usamos FixedUpdate para trabajar con f�sicas de forma consistente
    void FixedUpdate()
    {
        // Si no ha sido lanzado o ya se detuvo, no hacemos nada
        if (!fueLanzado || haTerminado)
        {
            return;
        }

        // La forma m�s fiable de saber si un objeto f�sico se detuvo
        // es preguntar si est� "durmiendo" (IsSleeping).
        if (rb.IsSleeping())
        {
            // Marcamos que ya termin� para no llamar al GameManager m�ltiples veces
            haTerminado = true;

            Debug.Log("El tejo se ha detenido en la posici�n: " + transform.position);

            // Le avisamos al GameManager que este tejo espec�fico termin� su movimiento
            if (GameManagerTejo.instance != null)
            {
                GameManagerTejo.instance.TejoTermino(this);
            }
        }
    }

    // Este m�todo debe ser llamado justo despu�s de que el tejo es lanzado
    // para que empiece a comprobar si se ha detenido.
    public void ActivarDeteccion()
    {
        fueLanzado = true;
    }
}