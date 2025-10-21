using UnityEngine;
using System.Collections;

public class CamaraSeguirTejo : MonoBehaviour
{
    public static CamaraSeguirTejo instance; // Singleton

    [Header("Referencias")]
    public Transform puntoInicial;
    private Transform objetivo;

    [Header("Ajustes de movimiento")]
    public float suavizado = 5f;
    public Vector3 offsetDiagonal = new Vector3(-5f, 3f, -5f);

    [Header("Detección de movimiento del tejo")]
    public float velocidadMinima = 0.2f;
    public float tiempoParaDetener = 1f;

    private Rigidbody rbTejo;
    private bool siguiendo = false;
    private float tiempoQuieto = 0f;

    // --- ✅ NUEVAS VARIABLES DE ESTADO ---
    private bool estaVolviendo = false; // true si la cámara está en la corutina VolverAPosicionInicial
    private Coroutine corutinaDeSeguimiento = null; // referencia a EsperarYSeguir

    private void Awake()
    {
        if (instance == null) instance = this;
    }
    // --- ✅ MÉTODO 'LateUpdate' MODIFICADO ---
    void LateUpdate()
    {
        // Si no estamos siguiendo O si YA estamos volviendo, no hacer nada.
        if (!siguiendo || estaVolviendo)
            return;

        // CASO 1: El objetivo (tejo) AÚN EXISTE.
        if (objetivo != null)
        {
            // Mover la cámara
            Vector3 posicionDeseada = objetivo.position + offsetDiagonal;
            transform.position = Vector3.Lerp(transform.position, posicionDeseada, suavizado * Time.deltaTime);
            transform.LookAt(objetivo.position);

            // Verificar si el tejo ya se detuvo
            if (rbTejo != null)
            {
                if (rbTejo.linearVelocity.magnitude < velocidadMinima)
                {
                    tiempoQuieto += Time.deltaTime;
                    if (tiempoQuieto >= tiempoParaDetener)
                    {
                        Debug.Log("Cámara: Tejo detenido. Volviendo a pos inicial.");
                        IniciarRetorno(); // <-- Usamos la nueva función
                    }
                }
                else
                {
                    tiempoQuieto = 0f;
                }
            }
            else
            {
                Debug.LogWarning("Cámara: Objetivo existe pero rbTejo es null. Volviendo.");
                IniciarRetorno(); // <-- Usamos la nueva función
            }
        }
        // CASO 2: El objetivo HA SIDO DESTRUIDO.
        else
        {
            Debug.Log("Cámara: ¡Objetivo nulo! (destruido). Volviendo a pos inicial.");
            IniciarRetorno(); // <-- Usamos la nueva función
        }
    }

    /// <summary>
    /// Se llama cuando un nuevo tejo es lanzado.
    /// </summary>
    // --- ✅ MÉTODO 'SeguirTejo' MODIFICADO ---
    public void SeguirTejo(Transform nuevoTejo)
    {
        // 1. Detener cualquier corutina de "espera para seguir" que estuviera pendiente
        if (corutinaDeSeguimiento != null)
        {
            StopCoroutine(corutinaDeSeguimiento);
        }

        // 2. Iniciar la nueva corutina de "espera para seguir" y guardar su referencia
        corutinaDeSeguimiento = StartCoroutine(EsperarYSeguir(nuevoTejo));
    }

    /// <summary>
    /// Espera a que la cámara vuelva Y LUEGO sigue al tejo.
    /// </summary>
    // --- ✅ MÉTODO 'EsperarYSeguir' MODIFICADO ---
    private IEnumerator EsperarYSeguir(Transform nuevoTejo)
    {
        // --- INICIO LÓGICA DE ESPERA ---
        // 1. Si la cámara está volviendo (estaVolviendo == true), espera.
        while (estaVolviendo)
        {
            Debug.Log("Cámara: Solicitud de seguimiento en espera, la cámara está volviendo...");
            yield return null; // Espera al siguiente frame
        }
        // --- FIN LÓGICA DE ESPERA ---

        // 2. Ahora que la cámara no está ocupada, espera el delay de 0.5s
        yield return new WaitForSeconds(0.5f); // retardo antes de seguir

        // 3. Asigna el nuevo objetivo y activa el seguimiento
        objetivo = nuevoTejo;
        rbTejo = nuevoTejo.GetComponent<Rigidbody>();
        siguiendo = true;
        tiempoQuieto = 0f;

        corutinaDeSeguimiento = null; // Esta corutina ha terminado
    }

    // --- ✅ NUEVO MÉTODO DE CONTROL ---
    /// <summary>
    /// Inicia la corutina de retorno, asegurándose de que solo haya una activa.
    /// </summary>
    private void IniciarRetorno()
    {
        // Si ya estamos volviendo, no hacer nada (evita múltiples llamadas)
        if (estaVolviendo)
            return;

        // Detener cualquier corutina de "espera para seguir" que estuviera pendiente
        if (corutinaDeSeguimiento != null)
        {
            StopCoroutine(corutinaDeSeguimiento);
            corutinaDeSeguimiento = null;
        }

        // Iniciar la corutina de retorno
        StartCoroutine(VolverAPosicionInicial());
    }


    // --- ✅ MÉTODO 'VolverAPosicionInicial' MODIFICADO ---
    private IEnumerator VolverAPosicionInicial()
    {
        // 1. Activar los flags de estado
        estaVolviendo = true; // <-- ¡LA CÁMARA ESTÁ OCUPADA!
        siguiendo = false;
        objetivo = null;
        rbTejo = null;

        yield return new WaitForSeconds(0.5f); // Pausa

        // Lógica de Lerp (sin cambios)
        Vector3 posFinal = puntoInicial.position;
        Quaternion rotFinal = puntoInicial.rotation;
        float t = 0f;
        Vector3 inicio = transform.position;
        Quaternion inicioRot = transform.rotation;
        while (t < 1f)
        {
            t += Time.deltaTime * 1.5f;
            transform.position = Vector3.Lerp(inicio, posFinal, t);
            transform.rotation = Quaternion.Lerp(inicioRot, rotFinal, t);
            yield return null;
        }

        // 2. Limpiar el flag al terminar
        estaVolviendo = false; // <-- ¡LA CÁMARA ESTÁ LIBRE!
    }
    /// <summary>
    /// Fuerza a la cámara a detener el seguimiento y volver a la base.
    /// Se llama desde el GameManager cuando el turno termina (por parada O POR FUERA DE LÍMITES).
    /// </summary>
    public void ForzarRetorno()
    {
        // No necesitamos comprobar si ya está volviendo,
        // porque 'IniciarRetorno' (que escribimos antes) ya tiene esa protección.
        Debug.Log("Cámara: Forzando retorno (llamado por GameManager).");
        IniciarRetorno(); // Usamos la misma función de limpieza que ya tenemos
    }
}