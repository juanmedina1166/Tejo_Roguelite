using UnityEngine;

// Se asegura de que el objeto siempre tenga un Rigidbody
[RequireComponent(typeof(Rigidbody))]
public class Tejo : MonoBehaviour
{
    private Rigidbody rb;
    private bool fueLanzado = false;
    public bool haTerminado = false;
    public int jugadorID;
    public bool puedeLanzar = false;
    public bool haTocadoMecha = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (GameManagerTejo.instance != null && GameManagerTejo.instance.estadoActual != GameManagerTejo.GameState.Jugando)
        {
            return;
        }
        if (!fueLanzado || haTerminado)
            return;

        // Si el tejo pasa de X = 100, se destruye y cambia de turno
        if (transform.position.x > 100f)
        {
            haTerminado = true;
            Debug.Log("El tejo pasó de x=100 en la posición: " + transform.position);

            if (GameManagerTejo.instance != null)
                GameManagerTejo.instance.TejoTermino(this);

            return;
        }

        // Si el tejo se detuvo (está dormido)
        if (rb.IsSleeping())
        {
            Debug.Log("El tejo se ha detenido en la posición: " + transform.position);

            if (GameManagerTejo.instance != null)
                GameManagerTejo.instance.TejoTermino(this);

        }
    }

    public void ResetTejo()
    {
        // Restablece su velocidad, rotación y cualquier estado previo
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 🔹 Marca el tejo como listo para lanzar nuevamente
        fueLanzado = false;
        haTerminado = false;
        puedeLanzar = true;
        haTocadoMecha = false; // Reiniciamos el flag de mecha

        Debug.Log("🔄 Tejo reiniciado y listo para lanzar.");
    }

    // Este método se llama cuando el tejo realmente se lanza
    public void ActivarDeteccion()
    {
        fueLanzado = true;
        puedeLanzar = false;
    }
}
