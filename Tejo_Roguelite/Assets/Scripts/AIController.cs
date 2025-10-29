using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    [Header("Referencias")]
    public LanzamientoTejo tejoPrefab;
    public Transform puntoDeLanzamiento;

    [Header("Objetivos")]
    [Tooltip("Lista de objetivos en el tablero. La IA elegirá uno al azar.")]
    public Transform objetivoPrincipal;
    public List<Transform> objetivos = new List<Transform>();

    private List<Transform> objetivosActivos = new List<Transform>();

    [Header("Parámetros de tablero")]
    public Vector2 tamañoTablero = new Vector2(6f, 6f);

    [Header("Parámetros de lanzamiento")]
    public float alturaExtra = 1.5f;
    public Vector2 rangoFuerzaPercent = new Vector2(0.9f, 1.1f);

    [Header("Comportamiento")]
    public float decisionDelay = 1.0f;
    [Range(0f, 1f)]
    public float chanceFallar = 0.15f;
    public float missFactorMin = 1.2f, missFactorMax = 2.0f;

    bool lanzando = false;

    void OnEnable()
    {
        RebuildActiveList();
        TrySubscribe();
    }

    void OnDisable()
    {
        if (TurnManager.instance != null)
            TurnManager.instance.OnTurnChanged -= OnTurnChanged;
    }

    void Start()
    {
        PersonajeIAData datos = GameLevelManager.instance.ObtenerDatosIA();

        chanceFallar = datos.chanceFallar;
        decisionDelay = datos.decisionDelay;
        rangoFuerzaPercent = datos.rangoFuerza;
        missFactorMin = datos.missFactor.x;
        missFactorMax = datos.missFactor.y;

        Debug.Log($"[IA] Iniciado personaje: {datos.nombre} ({datos.dificultad})");
    }

    void RebuildActiveList()
    {
        objetivosActivos.Clear();
        if (objetivos == null) return;

        foreach (var t in objetivos)
        {
            if (t != null)
                objetivosActivos.Add(t);
        }
    }

    void TrySubscribe()
    {
        if (TurnManager.instance != null)
        {
            TurnManager.instance.OnTurnChanged += OnTurnChanged;
            OnTurnChanged(TurnManager.instance.CurrentTurn());
        }
        else
        {
            StartCoroutine(WaitAndSubscribe());
        }
    }

    IEnumerator WaitAndSubscribe()
    {
        while (TurnManager.instance == null)
            yield return null;

        TurnManager.instance.OnTurnChanged += OnTurnChanged;
        OnTurnChanged(TurnManager.instance.CurrentTurn());
    }

    void OnTurnChanged(int jugador)
    {
        if (GameManagerTejo.instance != null && GameManagerTejo.instance.estadoActual != GameManagerTejo.GameState.Jugando)
        {
            return;
        }
        if (jugador == 2 && !lanzando)
        {
            StartCoroutine(RealizarLanzamientoIA());
        }
    }

    IEnumerator RealizarLanzamientoIA()
    {
        lanzando = true;
        yield return new WaitForSeconds(decisionDelay);

        if (tejoPrefab == null || puntoDeLanzamiento == null)
        {
            lanzando = false;
            yield break;
        }

        RebuildActiveList();

        // --- Elegir objetivo ---
        Transform objetivoElegido = null;

        if (objetivosActivos.Count > 0)
        {
            // 1. Prioridad: Una mecha activa
            objetivoElegido = objetivosActivos[Random.Range(0, objetivosActivos.Count)];
            Debug.Log($"AIController: Objetivo elegido (Mecha) '{objetivoElegido.name}' en {objetivoElegido.position}");
        }
        else if (objetivoPrincipal != null)
        {
            // 2. Plan B: El Bocín (objetivo principal)
            objetivoElegido = objetivoPrincipal;
            Debug.Log("AIController: No hay mechas activas, apuntando al objetivo principal (Bocín).");
        }

        // --- Definir punto de lanzamiento ---
        Vector3 objetivo; // 'objetivo' ahora se llama 'puntoDestino' en el original, lo cambiamos a 'objetivo'

        if (objetivoElegido != null)
        {
            objetivo = objetivoElegido.position;
        }
        else
        {
            // 3. Plan C: (Fallo de seguridad) Apuntar área general
            objetivo = puntoDeLanzamiento.position + puntoDeLanzamiento.forward * 5f;
            Debug.Log("AIController: No hay objetivos, apuntando área general.");
        }

        // --- Fallo intencional ---
        if (Random.value < chanceFallar)
        {
            float missFactor = Random.Range(missFactorMin, missFactorMax);
            Vector2 dir = Random.insideUnitCircle.normalized;
            objetivo += new Vector3(dir.x, 0, dir.y) * missFactor;
            Debug.Log($"AIController: Decidió fallar. Nuevo objetivo: {objetivo}");
        }

        // --- Calcular lanzamiento ---
        Vector3 velocidadInicial = CalcularLanzamientoParabolico(
            puntoDeLanzamiento.position, objetivo, alturaExtra, Physics.gravity.y
        );

        float variacion = Random.Range(rangoFuerzaPercent.x, rangoFuerzaPercent.y);
        velocidadInicial *= variacion;

        // --- Instanciar y lanzar ---
        LanzamientoTejo instancia = Instantiate(tejoPrefab, puntoDeLanzamiento.position, puntoDeLanzamiento.rotation);
        instancia.GetComponent<Tejo>().jugadorID = 1;
        instancia.IniciarConVelocidad(puntoDeLanzamiento.position, velocidadInicial);

        // === DEBUGS CLAVE ===
        Debug.Log($"[IA] Lanzamiento -> objetivo: {objetivo}, velocidadInicial: {velocidadInicial}, variacion: {variacion}");

        Rigidbody rb = instancia.GetComponent<Rigidbody>();
        if (rb != null)
            Debug.Log($"[IA] Velocidad real Rigidbody tras iniciar: {rb.linearVelocity}");

        Tejo tejoComp = instancia.GetComponent<Tejo>();
        if (tejoComp != null)
            tejoComp.ActivarDeteccion();

        //  Paso adicional: activar cámara de seguimiento luego de 0.5s
        StartCoroutine(EsperarYSeguirCamara(instancia.transform));

        lanzando = false;
    }

    Vector3 CalcularLanzamientoParabolico(Vector3 origen, Vector3 destino, float alturaMax, float gravedad)
    {
        Vector3 desplazamiento = destino - origen;
        Vector3 desplazamientoXZ = new Vector3(desplazamiento.x, 0, desplazamiento.z);
        float distancia = desplazamientoXZ.magnitude;

        float altura = destino.y - origen.y;
        float h = Mathf.Max(alturaMax, altura + 0.1f);

        float tiempoSubida = Mathf.Sqrt(2 * h / -gravedad);
        float tiempoBajada = Mathf.Sqrt(2 * Mathf.Abs(h - altura) / -gravedad);
        float tiempoTotal = tiempoSubida + tiempoBajada;

        Vector3 velocidadXZ = desplazamientoXZ / tiempoTotal;
        float velocidadY = Mathf.Sqrt(-2 * gravedad * h);

        Vector3 resultado = velocidadXZ + Vector3.up * velocidadY;
        return resultado;
    }

    private IEnumerator EsperarYSeguirCamara(Transform tejo)
    {
        yield return new WaitForSeconds(0.5f);

        CamaraSeguirTejo cam = FindObjectOfType<CamaraSeguirTejo>();
        if (cam != null)
        {
            cam.SeguirTejo(tejo);
            Debug.Log("[IA] Cámara ahora sigue el tejo de la IA.");
        }
        else
        {
            Debug.LogWarning("[IA] No se encontró una cámara con el script CamaraSeguirTejo.");
        }
    }

    public void ConfigurarDificultad(string nivel)
    {
        // Aquí puedes ajustar variables internas según la dificultad
        Debug.Log($"{name} configurada a dificultad {nivel}");
    }
}