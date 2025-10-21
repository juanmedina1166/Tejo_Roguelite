using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.Collections.Generic;

public class GameManagerTejo : MonoBehaviour
{
    // Arriba del todo de tu clase GameManagerTejo, pero dentro de ella.
    public enum GameState { Jugando, FinDeRonda, PartidaTerminada }
    private GameState estadoActual;
    public static GameManagerTejo instance;

    [Header("Puntajes")]
    public int[] puntajes = new int[4];
    public int puntajeMaximo = 21;

    [Header("UI Puntajes")]
    public TextMeshProUGUI[] puntajeTextos;

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
    private int turnosJugadosEnRonda = 0;
    private int cambiosDeTurno = 0;
    private bool esperandoCambioTurno = false;

    [Header("Objetivos")]
    public Transform bocin; // Arrastra el objeto del centro del objetivo aquí

    private List<Tejo> tejosDeLaRonda = new List<Tejo>();

    private bool mechaExplotadaEnRonda = false;

    // Añade esta línea en la sección de Headers, junto a las otras referencias de UI.
    [Header("Pantallas")]
    public RewardScreen rewardScreen; // ¡Arrastra tu UIManager aquí en el Inspector!

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        StartCoroutine(CrearTejoConDelay(0.2f));
        estadoActual = GameState.Jugando; // Empezamos la partida en el estado "Jugando"
    }

    private void OnEnable()
    {
        // Nos suscribimos al evento del TurnManager
        if (TurnManager.instance != null)
        {
            TurnManager.instance.OnTurnChanged += ManejarCambioDeTurno;
        }
    }

    private void OnDisable()
    {
        // Nos damos de baja para evitar errores
        if (TurnManager.instance != null)
        {
            TurnManager.instance.OnTurnChanged -= ManejarCambioDeTurno;
        }
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

    public void DarPuntoAlMasCercano()
    {
        if (mechaExplotadaEnRonda) return;
        // Primero, comprobamos si en esta ronda se sumaron puntos por mechas.
        // Si alguien hizo mecha, la regla de "mano" no aplica.
        // (Necesitarás una variable bool para controlar esto).
        // if (alguienHizoMechaEnLaRonda) return;

        if (bocin == null || tejosDeLaRonda.Count == 0) return;

        float distanciaMinima = float.MaxValue;
        Tejo tejoMasCercano = null;

        // Recorremos los tejos lanzados en la ronda
        foreach (var tejo in tejosDeLaRonda)
        {
            if (tejo == null) continue;

            float distancia = Vector3.Distance(tejo.transform.position, bocin.position);
            if (distancia < distanciaMinima)
            {
                distanciaMinima = distancia;
                tejoMasCercano = tejo;
            }
        }

        // Si encontramos un tejo más cercano, le damos el punto a su dueño
        if (tejoMasCercano != null)
        {
            int idDelGanador = tejoMasCercano.jugadorID;
            Debug.Log($"Punto por mano para el Jugador {idDelGanador + 1}! Distancia: {distanciaMinima}");
            SumarPuntos(idDelGanador, 1);
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
        tejosDeLaRonda.Add(tejo);
        Debug.Log($"El tejo de {TurnManager.instance.CurrentTurn()} se ha detenido.");
        StartCoroutine(RutinaCambioDeTurno());

    }
    private IEnumerator RutinaCambioDeTurno()
    {
        // Usamos la variable que ya tenías para el delay.
        Debug.Log($"Esperando {delayCambioTurno} segundos antes de cambiar de turno...");
        yield return new WaitForSeconds(delayCambioTurno);

        // Después de la pausa, le decimos al TurnManager que es el turno del siguiente.
        if (TurnManager.instance != null)
        {
            TurnManager.instance.NextTurn();
        }
    }

    private void LimpiarCancha()
    {
        Debug.Log("Limpiando los tejos de la cancha...");
        Tejo[] tejosEnCancha = FindObjectsOfType<Tejo>();
        foreach (var tejo in tejosEnCancha)
        {
            Destroy(tejo.gameObject);
        }
    }

    private IEnumerator MoverCentroConDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CentroController centro = FindObjectOfType<CentroController>();
        if (centro != null)
            centro.MoverCentro();
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
            nuevoTejo.GetComponent<Tejo>().jugadorID = 0;
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

    // Añade estos NUEVOS métodos a GameManagerTejo.cs

    /// <summary>
    /// Este método se activa cada vez que el TurnManager anuncia un nuevo turno.
    /// </summary>
    private void ManejarCambioDeTurno(int nuevoJugadorID)
    {
        // Cada vez que hay un cambio de turno, incrementamos nuestro contador.
        turnosJugadosEnRonda++;

        // Ahora, la condición para terminar la ronda es:
        // 1. Que el turno vuelva a ser del jugador 1.
        // 2. Y que se haya jugado al menos un turno en esta ronda.
        if (nuevoJugadorID == 1 && turnosJugadosEnRonda > 1 && estadoActual == GameState.Jugando)
        {
            StartCoroutine(RutinaFinDeRonda());
        }
    }

    /// <summary>
    /// Corrutina que maneja la secuencia de fin de ronda.
    /// </summary>
    private IEnumerator RutinaFinDeRonda()
    {
        estadoActual = GameState.FinDeRonda;
        Debug.Log("FIN DE LA RONDA. Calculando puntos...");

        // Esperamos un par de segundos para que el jugador vea el resultado
        yield return new WaitForSeconds(2f);

        DarPuntoAlMasCercano();

        // --- LÓGICA DE PUNTUACIÓN DE FIN DE RONDA ---
        // Aquí es donde calculas puntos como el de "mano" (el más cercano)
        // Nota: Necesitarás una forma de obtener las posiciones de los tejos lanzados.
        // DarPuntoAlMasCercano(...); 

        // --- COMPROBAR SI LA PARTIDA TERMINÓ ---
        bool partidaGanada = false;
        for (int i = 0; i < puntajes.Length; i++)
        {
            if (puntajes[i] >= puntajeMaximo)
            {
                partidaGanada = true;
                break;
            }
        }

        if (partidaGanada)
        {
            TerminarPartida();
        }
        else
        {
            // Si nadie ha ganado, empezamos la siguiente ronda
            Debug.Log("Nadie ha ganado todavía. Iniciando siguiente ronda.");
            LimpiarCancha();
            tejosDeLaRonda.Clear();
            mechaExplotadaEnRonda = false;
            estadoActual = GameState.Jugando;
            turnosJugadosEnRonda = 0;
            yield return null;
            CrearTejoJugador();
            // Aquí puedes añadir lógica para limpiar los tejos viejos de la cancha.
        }
    }

    /// <summary>
    /// Se llama cuando un jugador alcanza el puntaje máximo.
    /// </summary>
    private void TerminarPartida()
    {
        estadoActual = GameState.PartidaTerminada;
        Debug.Log("¡PARTIDA TERMINADA! Mostrando recompensas.");

        // ¡La conexión clave! Llamamos a la pantalla de recompensas.
        if (rewardScreen != null)
        {
            rewardScreen.MostrarRecompensas();
        }
        else
        {
            Debug.LogError("RewardScreen no está asignado en el GameManager!");
        }
    }

    public void ReiniciarParaNuevaPartida()
    {
        Debug.Log("Reiniciando el juego para una nueva partida...");

        // 1. Reiniciar puntajes
        for (int i = 0; i < puntajes.Length; i++)
        {
            puntajes[i] = 0;
        }
        // Actualizar la UI a 0
        // Necesitarás un método que redibuje los puntajes, vamos a asumir que lo tienes.
        // ActualizarPuntajeUI(); 

        LimpiarCancha();

        // 3. Reiniciar el turno al jugador 1
        TurnManager.instance.SetTurn(1);

        // 4. Volver al estado de "Jugando"
        estadoActual = GameState.Jugando;

    }
    public void MarcarMechaExplotada()
    {
        mechaExplotadaEnRonda = true;
    }
}
