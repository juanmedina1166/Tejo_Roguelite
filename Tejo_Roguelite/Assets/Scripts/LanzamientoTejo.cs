using UnityEngine;

// Obliga a que este GameObject tenga siempre un componente Rigidbody
[RequireComponent(typeof(Rigidbody))]
public class LanzamientoTejo : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 escalaInicial;

    // Awake se llama una vez al crear el objeto, antes que Start
    void Awake()
    {
        // Obtenemos la referencia al componente Rigidbody para poder usarlo
        rb = GetComponent<Rigidbody>();
        escalaInicial = transform.localScale;
    }

    /// <summary>
    /// Inicia el lanzamiento del tejo aplicando una fuerza f�sica.
    /// </summary>
    /// <param name="origen">Punto desde donde se lanza el tejo.</param>
    /// <param name="direccionLanzamiento">Vector que indica la direcci�n y �ngulo del tiro.</param>
    /// <param name="fuerza">La potencia del tiro, que viene de la barra de fuerza.</param>
    public void Iniciar(Vector3 origen, Vector3 direccionLanzamiento, float fuerza)
    {
        // Colocamos el tejo en la posici�n inicial
        transform.position = origen;
        transform.localScale = escalaInicial; // Reseteamos la escala por si acaso

        // Reseteamos cualquier f�sica anterior para un lanzamiento limpio
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // �La magia del 3D! Aplicamos una fuerza en la direcci�n calculada.
        // ForceMode.Impulse aplica la fuerza instant�neamente, ideal para un lanzamiento.
        rb.AddForce(direccionLanzamiento.normalized * fuerza, ForceMode.Impulse);
    }

    // El m�todo Update() ya no es necesario para el movimiento,
    // porque el motor de f�sica de Unity se encarga de ello autom�ticamente.
    // Podr�as usarlo para efectos visuales, como hacer que el tejo rote.
}
