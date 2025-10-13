using System.Collections;
using UnityEngine;

public class AIController : MonoBehaviour
{
    [Header("Referencias")]
    public LanzamientoTejo tejoPrefab;
    public Transform puntoDeLanzamiento;
    public Transform objetivoCentral;
    public CentroController centroController;

    [Header("Par�metros de tablero")]
    public Vector2 tama�oTablero = new Vector2(6f, 6f);

    [Header("Par�metros de lanzamiento")]
    public float alturaExtra = 1.5f; // Altura m�xima de la par�bola
    public float multiplicadorDeFuerza = 60f; // ya no es cr�tico para la IA bal�stica, se puede ajustar
    public Vector2 rangoFuerzaPercent = new Vector2(0.9f, 1.1f); // aplicable como variaci�n sobre la velocidad

    [Header("Comportamiento")]
    public float decisionDelay = 1.0f;
    [Range(0f, 1f)]
    public float chanceFallar = 0.15f;
    public float missFactorMin = 1.2f, missFactorMax = 2.0f;

    bool lanzando = false;

    void OnEnable()
    {
        TrySubscribe();
    }

    void OnDisable()
    {
        if (TurnManager.instance != null)
            TurnManager.instance.OnTurnChanged -= OnTurnChanged;
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

        // Determinar la posici�n base del centro del tablero
        Vector3 centroPos;
        if (objetivoCentral != null)
        {
            centroPos = objetivoCentral.position;
        }
        else if (centroController != null)
        {
            centroPos = centroController.transform.position;
        }
        else
        {
            centroPos = puntoDeLanzamiento.position + puntoDeLanzamiento.forward * 5f;
        }

        // Elegir objetivo dentro del tablero
        Vector2 half = tama�oTablero * 0.5f;
        Vector2 offset2D = Random.insideUnitCircle;
        Vector3 objetivo = new Vector3(
            centroPos.x + offset2D.x * half.x,
            puntoDeLanzamiento.position.y,
            centroPos.z + offset2D.y * half.y
        );

        bool fallo = Random.value < chanceFallar;
        if (fallo)
        {
            float missFactor = Random.Range(missFactorMin, missFactorMax);
            Vector2 dirExterior = Random.insideUnitCircle.normalized;
            objetivo = new Vector3(
                centroPos.x + dirExterior.x * Mathf.Max(half.x, half.y) * missFactor,
                puntoDeLanzamiento.position.y,
                centroPos.z + dirExterior.y * Mathf.Max(half.x, half.y) * missFactor
            );
        }

        // --- Lanzamiento parab�lico: calculamos la velocidad inicial exacta ---
        Vector3 velocidadInicial = CalcularLanzamientoParabolico(puntoDeLanzamiento.position, objetivo, alturaExtra, Physics.gravity.y);

        // Aplicamos una ligera variaci�n aleatoria sobre la magnitud para simular imprecisi�n
        float variacion = Random.Range(rangoFuerzaPercent.x, rangoFuerzaPercent.y);
        velocidadInicial *= variacion;

        // Instanciar y fijar la velocidad directamente (evita problemas con la masa y ForceMode)
        LanzamientoTejo instancia = Instantiate(tejoPrefab, puntoDeLanzamiento.position, puntoDeLanzamiento.rotation);
        instancia.IniciarConVelocidad(puntoDeLanzamiento.position, velocidadInicial);

        if (GameManagerTejo.instance != null)
            GameManagerTejo.instance.RegistrarTejoLanzado();

        Tejo tejoComp = instancia.GetComponent<Tejo>();
        if (tejoComp != null)
            tejoComp.ActivarDeteccion();

        lanzando = false;
    }

    // Calcula la velocidad inicial necesaria para que un objeto lanzado desde 'origen' llegue a 'destino'
    // alcanzando aproximadamente 'alturaMax' en la curva. gravedad debe ser negativo (Physics.gravity.y).
    Vector3 CalcularLanzamientoParabolico(Vector3 origen, Vector3 destino, float alturaMax, float gravedad)
    {
        Vector3 desplazamiento = destino - origen;
        Vector3 desplazamientoXZ = new Vector3(desplazamiento.x, 0, desplazamiento.z);
        float distancia = desplazamientoXZ.magnitude;

        float altura = destino.y - origen.y;
        float h = Mathf.Max(alturaMax, altura + 0.1f);

        // Tiempos de subida y bajada
        float tiempoSubida = Mathf.Sqrt(2 * h / -gravedad);
        float tiempoBajada = Mathf.Sqrt(2 * Mathf.Abs(h - altura) / -gravedad);
        float tiempoTotal = tiempoSubida + tiempoBajada;

        // Velocidad horizontal necesaria
        Vector3 velocidadXZ = desplazamientoXZ / tiempoTotal;

        // Velocidad vertical inicial
        float velocidadY = Mathf.Sqrt(-2 * gravedad * h);

        Vector3 resultado = velocidadXZ + Vector3.up * velocidadY;
        return resultado;
    }
}