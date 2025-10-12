using System.Collections;
using UnityEngine;

public class AIController : MonoBehaviour
{
    [Header("Referencias")]
    public LanzamientoTejo tejoPrefab;
    public Transform puntoDeLanzamiento;
    public CentroController centroController;

    [Header("Par�metros de lanzamiento")]
    [Tooltip("Componente vertical objetivo (igual que ControlJugador)")]
    public float alturaDelArco = 0.8f;
    [Tooltip("Multiplicador de fuerza (igual que ControlJugador)")]
    public float multiplicadorDeFuerza = 60f;
    [Tooltip("Fuerza aleatoria como porcentaje (0..1) del m�ximo")]
    public Vector2 rangoFuerzaPercent = new Vector2(0.6f, 0.95f);

    [Header("Comportamiento")]
    [Tooltip("Retraso antes de que la IA lance (simula pensar)")]
    public float decisionDelay = 1.0f;
    [Tooltip("Radio de dispersi�n alrededor del centro para apuntar")]
    public float radioAproximacion = 1.5f;

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
            Debug.Log("AIController: suscrito a TurnManager.OnTurnChanged");

            // Reaccionar inmediatamente al turno actual por si TurnManager ya lo anunci�
            OnTurnChanged(TurnManager.instance.CurrentTurn());
        }
        else
        {
            // Si TurnManager no est� listo a�n, lo esperamos y lo intentamos de nuevo.
            StartCoroutine(WaitAndSubscribe());
        }
    }

    IEnumerator WaitAndSubscribe()
    {
        while (TurnManager.instance == null)
            yield return null;

        TurnManager.instance.OnTurnChanged += OnTurnChanged;
        Debug.Log("AIController: suscrito a TurnManager.OnTurnChanged (despu�s de esperar)");

        // Reaccionar inmediatamente al turno actual en cuanto estemos suscritos
        OnTurnChanged(TurnManager.instance.CurrentTurn());
    }

    void OnTurnChanged(int jugador)
    {
        Debug.Log($"AIController: OnTurnChanged recibido para jugador {jugador}");
        // Si es el turno de la IA y no estamos ya lanzando, iniciamos la rutina.
        if (jugador == 2 && !lanzando)
        {
            StartCoroutine(RealizarLanzamientoIA());
        }
    }

    IEnumerator RealizarLanzamientoIA()
    {
        lanzando = true;

        // Peque�a espera para simular "pensar"
        yield return new WaitForSeconds(decisionDelay);

        if (tejoPrefab == null || puntoDeLanzamiento == null)
        {
            Debug.LogWarning("AIController: faltan referencias (tejoPrefab o puntoDeLanzamiento).");
            lanzando = false;
            yield break;
        }

        // Determinar objetivo: si hay CentroController usar su posici�n y a�adir ruido
        Vector3 objetivo;
        if (centroController != null)
        {
            Vector3 basePos = centroController.transform.position;
            Vector2 ruido = Random.insideUnitCircle * radioAproximacion;
            objetivo = new Vector3(basePos.x + ruido.x, puntoDeLanzamiento.position.y, basePos.z + ruido.y);
        }
        else
        {
            // Si no hay centro, lanzamos hacia delante con algo de variaci�n
            objetivo = puntoDeLanzamiento.position + puntoDeLanzamiento.forward * 5f;
            objetivo += new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        }

        // Calcular direcci�n y fuerza (misma l�gica que ControlJugador)
        Vector3 direccion = objetivo - puntoDeLanzamiento.position;
        Vector3 direccionDeLanzamiento = new Vector3(direccion.x, 0, direccion.z).normalized;
        direccionDeLanzamiento.y = alturaDelArco;

        float percent = Random.Range(rangoFuerzaPercent.x, rangoFuerzaPercent.y);
        float fuerza = percent * multiplicadorDeFuerza;

        // Instanciar y lanzar
        LanzamientoTejo instancia = Instantiate(tejoPrefab, puntoDeLanzamiento.position, puntoDeLanzamiento.rotation);
        instancia.Iniciar(puntoDeLanzamiento.position, direccionDeLanzamiento, fuerza);

        // Registrar el lanzamiento en el GameManager (si existe)
        if (GameManagerTejo.instance != null)
            GameManagerTejo.instance.RegistrarTejoLanzado();

        // Si el prefab tiene un componente Tejo, activamos la detecci�n de parada
        Tejo tejoComp = instancia.GetComponent<Tejo>();
        if (tejoComp != null)
            tejoComp.ActivarDeteccion();

        // Fin del lanzamiento; permitimos que el sistema contin�e cuando corresponda.
        lanzando = false;
    }
}
