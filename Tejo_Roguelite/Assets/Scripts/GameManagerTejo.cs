using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManagerTejo : MonoBehaviour
{
    public static GameManagerTejo instance;

    [Header("Puntajes")]
    public int[] puntajes = new int[4];
    public int puntajeMaximo = 21;

    [Header("UI Puntajes")]
    public Text[] puntajeTextos;

    [Header("Ronda / Turnos")]
    [SerializeField] private int maxTiros = 3;
    [SerializeField] private float delayCambioTurno = 2f;
    [SerializeField] private float delayMoverCentro = 2f;

    [Header("Control de Input")]
    [SerializeField] private GameObject blocker;

    [Header("Prefabs y posiciones")]
    [SerializeField] private GameObject tejoJugadorPrefab;
    [SerializeField] private Transform spawnJugador;

    private int tirosRealizados = 0;
    private int cambiosDeTurno = 0;
    private bool esperandoCambioTurno = false;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        StartCoroutine(CrearTejoConDelay(0.2f));
    }

    private IEnumerator CrearTejoConDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CrearTejoJugador();
    }

    public void SumarPuntos(int jugadorID, int puntos)
    {
        Debug.Log($"Jugador {jugadorID + 1} gana {puntos} puntos");
        puntajes[jugadorID] += puntos;
        if (puntajeTextos != null && jugadorID >= 0 && jugadorID < puntajeTextos.Length)
            puntajeTextos[jugadorID].text = $"J{jugadorID + 1}: {puntajes[jugadorID]}";

        if (puntajes[jugadorID] >= puntajeMaximo)
        {
            Debug.Log($"¡El jugador {jugadorID + 1} ha ganado el juego!");
        }
    }

    public void RestarPuntos(int jugadorID, int puntos)
    {
        puntajes[jugadorID] -= puntos;
        if (puntajeTextos != null && jugadorID >= 0 && jugadorID < puntajeTextos.Length)
            puntajeTextos[jugadorID].text = $"J{jugadorID + 1}: {puntajes[jugadorID]}";
    }

    public void DarPuntoAlMasCercano(Vector3[] posicionesTejos, Vector3 centro)
    {
        float distanciaMin = float.MaxValue;
        int jugadorCercano = -1;

        for (int i = 0; i < posicionesTejos.Length; i++)
        {
            float dist = Vector3.Distance(posicionesTejos[i], centro);
            if (dist < distanciaMin)
            {
                distanciaMin = dist;
                jugadorCercano = i;
            }
        }

        if (jugadorCercano >= 0)
        {
            SumarPuntos(jugadorCercano, 1);
        }
    }

    public void AvisarCambioTurno()
    {
        cambiosDeTurno++;
        Debug.Log($"Cambio de turno #{cambiosDeTurno}");
    }

    public void RegistrarTejoLanzado()
    {
        tirosRealizados++;
        esperandoCambioTurno = true;
        if (blocker != null) blocker.SetActive(true);
    }

    public void TejoTermino(Tejo tejo)
    {
        StartCoroutine(MoverCentroConDelay(delayMoverCentro));

        if (!esperandoCambioTurno) return;

        StartCoroutine(CambiarTurnoDespuesDeRetraso(delayCambioTurno));
        esperandoCambioTurno = false;
    }

    private IEnumerator MoverCentroConDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CentroController centro = FindObjectOfType<CentroController>();
        if (centro != null)
            centro.MoverCentro();
    }

    private IEnumerator CambiarTurnoDespuesDeRetraso(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (TurnManager.instance != null)
            TurnManager.instance.NextTurn();

        AvisarCambioTurno();

        MultiJoystickControl multi = FindObjectOfType<MultiJoystickControl>();
        if (multi != null)
            multi.PrepareForNextRound();

        CentroController centro = FindObjectOfType<CentroController>();
        if (centro != null)
        {
            var col3D = centro.GetComponent<Collider>();
            if (col3D != null) col3D.enabled = true;

            var renderer = centro.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = true;

            centro.MoverCentro();
        }

        tirosRealizados = 0;
        if (blocker != null) blocker.SetActive(false);

        //  Crear nuevo tejo para el jugador solo cuando vuelva su turno
        if (TurnManager.instance != null && TurnManager.instance.IsHumanTurn())
        {
            CrearTejoJugador();
        }
    }

    // ======================================================
    // === NUEVO MÉTODO: creación e inicialización completa del tejo ===
    // ======================================================
    private void CrearTejoJugador()
    {
        // Elimina cualquier tejo viejo que haya quedado inactivo
        foreach (Tejo viejo in FindObjectsOfType<Tejo>())
        {
            if (!viejo.gameObject.activeInHierarchy)
                Destroy(viejo.gameObject);
        }

        // Evita duplicar si ya existe un tejo activo
        Tejo tejoExistente = FindObjectOfType<Tejo>();
        if (tejoExistente != null) return;

        if (tejoJugadorPrefab != null && spawnJugador != null)
        {
            GameObject nuevoTejo = Instantiate(tejoJugadorPrefab, spawnJugador.position, spawnJugador.rotation);
            Debug.Log(" Nuevo tejo del jugador creado.");

            //  Asignar el nuevo tejo al ControlJugador (para que pueda lanzarlo)
            var controlJugador = FindObjectOfType<ControlJugador>();
            if (controlJugador != null)
            {
                var lanzador = nuevoTejo.GetComponent<LanzamientoTejo>();
                controlJugador.AsignarTejoExistente(lanzador);
            }

            //  Marcarlo como "listo para disparar"
            var scriptTejo = nuevoTejo.GetComponent<Tejo>();
            if (scriptTejo != null)
            {
                scriptTejo.ResetTejo(); // este método debería dejarlo listo (posición, física desactivada, etc.)
            }
        }
        else
        {
            Debug.LogWarning(" No se pudo crear el tejo: prefab o spawn no asignado.");
        }
    }
}
