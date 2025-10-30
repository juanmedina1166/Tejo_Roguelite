using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class LanzamientoTejo : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 escalaInicial;

    [Header("Seguridad de lanzamiento")]
    [Tooltip("Velocidad máxima permitida al lanzar (m/s). Si la velocidad calculada supera este valor, se clampeará.")]
    [SerializeField] private float maxLaunchSpeed = 40f;
    private Transform bocinTransform;
    private Tejo tejoComponent;
    // --- LÍNEA ELIMINADA ---
    // private bool haSplitteado = false; (Ya no se usa)

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        escalaInicial = transform.localScale;

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = false;
            Debug.Log($" [Awake] Tejo inicializado. Kinematic={rb.isKinematic}, Gravity={rb.useGravity}");
        }
        tejoComponent = GetComponent<Tejo>();
    }
    void Start()
    {
        // Obtenemos el transform del bocín desde el GameManager
        if (GameManagerTejo.instance != null)
        {
            bocinTransform = GameManagerTejo.instance.bocin;
        }
    }

    // Ahora Iniciar lanza mediante una corrutina que establece la velocidad directamente
    public void Iniciar(Vector3 origen, Vector3 direccionLanzamiento, float fuerza)
    {
        StartCoroutine(IniciarConRetraso(origen, direccionLanzamiento, fuerza));
    }

    private System.Collections.IEnumerator IniciarConRetraso(Vector3 origen, Vector3 direccionLanzamiento, float fuerza)
    {
        // Reinicio sincrónico
        ResetearFisica();

        // Asegurarnos de que el GameObject completó Awake/Start en este frame
        yield return null;

        // Esperar al siguiente FixedUpdate para sincronizar con la física (protección extra)
        yield return new WaitForFixedUpdate();

        transform.position = origen;
        transform.localScale = escalaInicial;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.WakeUp();

        // Normalizar dirección y calcular velocidad objetivo.
        Vector3 dirNorm = direccionLanzamiento.normalized;

        // Interpretamos 'fuerza' como un impulso (Newton·seg). Vel = impulso / masa.
        float masa = (rb != null && rb.mass > 0f) ? rb.mass : 1f;
        Vector3 velocidadCalculada = dirNorm * (fuerza / masa);

        // Aplicamos tope de seguridad para evitar valores absurdos
        if (velocidadCalculada.magnitude > maxLaunchSpeed)
        {
            Debug.LogWarning($" Vel calculada ({velocidadCalculada.magnitude:F2}) mayor que maxLaunchSpeed ({maxLaunchSpeed}), clampearé.");
            velocidadCalculada = velocidadCalculada.normalized * maxLaunchSpeed;
        }

        Debug.Log($" [Antes de SET velocity] fuerza={fuerza:F2}, masa={masa:F2}, velCalc={velocidadCalculada.magnitude:F2}, pos={transform.position}");

        // Asignamos la velocidad directamente (comportamiento determinista, igual que IA)
        rb.linearVelocity = velocidadCalculada;
        rb.WakeUp();

        Debug.Log($" [Después de SET velocity] Velocidad={rb.linearVelocity.magnitude:F2}, Vector={rb.linearVelocity}, UseGravity={rb.useGravity}, Kinematic={rb.isKinematic}");
    }

    public void IniciarConVelocidad(Vector3 origen, Vector3 velocidadInicial)
    {
        ResetearFisica();

        transform.position = origen;
        transform.localScale = escalaInicial;

        rb.useGravity = true;
        rb.isKinematic = false;

        // Aplicar tope también para lanzamientos de IA
        Vector3 velocidadAjustada = velocidadInicial;
        if (velocidadInicial.magnitude > maxLaunchSpeed)
            velocidadAjustada = velocidadInicial.normalized * maxLaunchSpeed;

        rb.linearVelocity = velocidadAjustada;
        rb.WakeUp();

        Debug.Log($" [IA Lanzamiento] Velocidad inicial={rb.linearVelocity.magnitude:F2}, Vector={rb.linearVelocity}");
    }

    private void ResetearFisica()
    {
        if (rb == null) return;

        Debug.Log($" [ResetearFisica] Antes del reset: Vel={rb.linearVelocity.magnitude:F2}, Kinematic={rb.isKinematic}, Gravity={rb.useGravity}");

        rb.isKinematic = false;
        rb.useGravity = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Sleep + WakeUp inmediato para intentar dejar el cuerpo limpio
        // rb.Sleep(); // <-- ¡LÍNEA ELIMINADA! Esta era la causa del error.
        rb.WakeUp();

        Debug.Log($" [ResetearFisica] Después del reset: Vel={rb.linearVelocity.magnitude:F2}, Kinematic={rb.isKinematic}, Gravity={rb.useGravity}");
    }
    void FixedUpdate()
    {
        // Solo aplicar si el tejo está en el aire (con gravedad activa)
        if (rb == null || !rb.useGravity || rb.IsSleeping()) return;

        // --- Lógica de "Imán de Bocín" ---
        Habilidad iman = HabilidadManager.instance.GetHabilidad("Iman del Bocin");
        
        if (HabilidadManager.instance.imanBocinActivo && iman != null)
        {
            if (bocinTransform != null)
            {
                if (tejoComponent != null && tejoComponent.jugadorID == 0)
                {
                    Vector3 direccionAlBocin = (bocinTransform.position - transform.position);
                    float distancia = direccionAlBocin.magnitude;

                    // Leemos el rango (valor2) y la fuerza (valor1) del asset
                    if (distancia < iman.valorNumerico2 && distancia > 0.1f)
                    {
                        Debug.Log("¡HABILIDAD: Imán de Bocín! Atrayendo tejo.");
                        float fuerzaIman = iman.valorNumerico1 / (distancia + 1f);
                        rb.AddForce(direccionAlBocin.normalized * fuerzaIman, ForceMode.Acceleration);
                    }
                }
            }
        }

        // --- LÓGICA ELIMINADA ---
        // (Toda la lógica de "Partido a machete" fue eliminada de aquí)
    }

    // --- MÉTODO ELIMINADO ---
    // private void SplitTejo(Habilidad habilidadMachete)
    // (El método completo fue eliminado)

    private void OnDestroy()
    {
        if (rb != null)
            Debug.Log($" [Destroy] Tejo destruido. Última Vel={rb.linearVelocity.magnitude:F2}");
    }
}
