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
    private bool haSplitteado = false;

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
        rb.Sleep();
        rb.WakeUp();

        Debug.Log($" [ResetearFisica] Después del reset: Vel={rb.linearVelocity.magnitude:F2}, Kinematic={rb.isKinematic}, Gravity={rb.useGravity}");
    }
    void FixedUpdate()
    {
        // Solo aplicar si el tejo está en el aire (con gravedad activa)
        if (rb == null || !rb.useGravity || rb.IsSleeping()) return;

        // --- Lógica de "Imán de Bocín" ---
        Habilidad iman = HabilidadManager.instance.GetHabilidad("Imán de Bocín");
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
        // --- Lógica de "Partido a machete" ---
        Habilidad machete = HabilidadManager.instance.GetHabilidad("Partido a machete");
        if (!haSplitteado && tejoComponent != null && tejoComponent.jugadorID == 0 && machete != null)
        {
            if (rb.linearVelocity.y < 0.1f && rb.linearVelocity.y > -0.1f && transform.position.y > 1f)
            {
                haSplitteado = true;
                SplitTejo(machete); // Pasamos la habilidad al método
            }
        }
    }
    private void SplitTejo(Habilidad habilidadMachete)
    {
        Debug.Log("¡HABILIDAD: Partido a machete! Dividiendo el tejo.");

        // Creamos el segundo tejo (una copia de este)
        GameObject tejo2_GO = Instantiate(gameObject, transform.position, transform.rotation);
        Tejo tejo2_Comp = tejo2_GO.GetComponent<Tejo>();
        LanzamientoTejo tejo2_Lanz = tejo2_GO.GetComponent<LanzamientoTejo>();
        Rigidbody rb2 = tejo2_GO.GetComponent<Rigidbody>();

        // Marcamos el segundo tejo para que no se divida de nuevo
        tejo2_Lanz.haSplitteado = true;

        // Hacemos ambos más pequeños y ligeros
        float escala = habilidadMachete.valorNumerico2;
        float fuerzaSplit = habilidadMachete.valorNumerico1;

        transform.localScale *= escala;
        rb.mass *= escala;
        tejo2_GO.transform.localScale *= escala;
        rb2.mass *= escala;

        Vector3 vel1 = rb.linearVelocity + (transform.right * fuerzaSplit);
        Vector3 vel2 = rb.linearVelocity + (-transform.right * fuerzaSplit);

        rb.linearVelocity = vel1;
        rb2.linearVelocity = vel2;

        // El GameManager ya se encarga de registrar ambos tejos
        // y calculará "mano" con el que quede más cerca.
        // Y si CUALQUIERA golpea una mecha, `MarcarMechaExplotada` se llamará,
        // anulando el punto de mano, tal como dice la habilidad.
    }

    private void OnDestroy()
    {
        if (rb != null)
            Debug.Log($" [Destroy] Tejo destruido. Última Vel={rb.linearVelocity.magnitude:F2}");
    }
}


