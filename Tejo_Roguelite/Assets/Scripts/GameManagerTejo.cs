using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using static GameManagerTejo;

public class GameManagerTejo : MonoBehaviour
{
    // Arriba del todo de tu clase GameManagerTejo, pero dentro de ella.
    public enum GameState { Jugando, FinDeRonda, PartidaTerminada }
    public GameState estadoActual;
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

    [SerializeField] private GameObject tejoIAPrefab;

    private int tirosRealizados = 0;
    private int turnosJugadosEnRonda = 0;
    private int cambiosDeTurno = 0;
    private bool esperandoCambioTurno = false;

    [Header("Objetivos")]
    public Transform bocin; // Arrastra el objeto del centro del objetivo aquí

    [SerializeField] private BocinTrigger bocinTrigger;

    private List<Tejo> tejosDeLaRonda = new List<Tejo>();

    private bool mechaExplotadaEnRonda = false;

    private bool mechaExplotadaEnTurno = false;
    private int idJugadorMecha = -1;
    private int puntosBaseMecha = 0;

    [Header("Habilidades")]
    [Tooltip("El prefab de la mecha señuelo para 'Falsa Alarma'")]
    [SerializeField] private GameObject prefabMechaFalsa;
    [Tooltip("Un array de Transforms donde pueden aparecer las mechas falsas")]
    [SerializeField] private Transform[] posicionesMechasFalsas;

    private bool bumeranExitoso = false;
    private int tirosExtraEsteTurno = 0;
    private int rondasJugadas = 0; // Para "Chancleteè el motor"

    // Añade esta línea en la sección de Headers, junto a las otras referencias de UI.
    [Header("Pantallas")]
    public RewardScreen rewardScreen; // ¡Arrastra tu UIManager aquí en el Inspector!
    [Header("Gestión de Mechas (Objetivos)")]
    [Tooltip("Arrastra aquí tu Prefab de Mecha (el que tiene Objetivo.cs)")]
    [SerializeField] private GameObject prefabMecha;

    [Tooltip("Cuántas mechas quieres que aparezcan en cada ronda")]
    [SerializeField] private int cantidadDeMechas = 3;

    [Tooltip("El tamaño del área donde pueden aparecer (X, Z) alrededor del bocín")]
    [SerializeField] private Vector2 areaDeSpawn = new Vector2(5f, 5f);

    [Tooltip("La altura (Y) a la que deben aparecer las mechas")]
    [SerializeField] private float alturaDeSpawn = 0.5f;

    // Lista para guardar las mechas que creamos
    private List<GameObject> mechasActivas = new List<GameObject>();

    [Tooltip("La capa física (Layer) donde está el suelo de la cancha. El raycast buscará aquí.")]
    [SerializeField] private LayerMask canchaLayerMask; // <-- AÑADE ESTA LÍNEA

    // Renombra esta variable para que sea más claro su propósito
    [Tooltip("Cuánto 'flotará' la mecha por encima del suelo inclinado")]
    [SerializeField] private float alturaSobreElSuelo = 0.1f;
    [Tooltip("El radio alrededor del bocín donde NO aparecerán mechas")]
    [SerializeField] private float radioZonaMuerta = 1.5f;

    [Header("Gestión de IA")]
    [SerializeField] private GestorDeNivelesIA gestorIA;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        if (SaveManager.DoesSaveExist())
        {
            // MODO CONTINUAR
            Debug.Log("Cargando partida guardada...");
            GameData data = SaveManager.LoadGame();
            if (data != null)
            {
                CargarPartidaGuardada(data);
            }
            else
            {
                // Error al cargar, empezar de cero
                Debug.LogError("¡Error al cargar GameData! Empezando nueva partida.");
                EmpezarSeleccionInicial();
            }
        }
        else
        {
            // MODO NUEVA PARTIDA (como lo tenías)
            EmpezarSeleccionInicial();
        }
    }
    private void EmpezarSeleccionInicial()
    {
        estadoActual = GameState.PartidaTerminada;
        if (rewardScreen != null)
        {
            // El RewardScreen llamará a 'ReiniciarParaNuevaPartida'
            rewardScreen.MostrarRecompensas();
        }
        else
        {
            Debug.LogError("¡RewardScreen no está asignado!");
            // Si no hay reward screen, forzamos inicio
            ReiniciarParaNuevaPartida();
        }
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
        if (estadoActual == GameState.PartidaTerminada)
        {
            return;
        }
        float multiplicador = 1; // Cambiamos a float para multiplicadores

        if (jugadorID == 0)
        {
            // Habilidad: "Ultima Chance"
            Habilidad ultima = HabilidadManager.instance.GetHabilidad("Ultima Chance");
            if (ultima != null)
            {
                int puntajeIA = puntajes[1];
                // Leemos los puntos de ventaja (valor1) y el multiplicador (valor2) del asset
                if (puntajes[0] < puntajeIA && (puntajeMaximo - puntajeIA) <= ultima.valorNumerico1)
                {
                    Debug.Log("¡HABILIDAD: Ultima Chance!");
                    multiplicador *= ultima.valorNumerico2;
                }
            }

            // Habilidad: "Fiebre del Oro"
            Habilidad fiebre = HabilidadManager.instance.GetHabilidad("Fiebre del Oro");
            if (fiebre != null && fiebre.valorNumerico1 > 0)
            {
                Debug.Log("¡HABILIDAD: Fiebre del Oro!");
                // Leemos el multiplicador (valor2) del asset
                multiplicador *= fiebre.valorNumerico2;
            }
        }

        int puntosFinales = Mathf.RoundToInt(puntos * multiplicador);
        Debug.Log($"Jugador {jugadorID + 1} gana {puntosFinales} puntos (base: {puntos}, mult: {multiplicador}x)");
        puntajes[jugadorID] += puntosFinales;

        if (puntajeTextos != null && jugadorID >= 0 && jugadorID < puntajeTextos.Length)
            puntajeTextos[jugadorID].text = $"J{jugadorID + 1}: {puntajes[jugadorID]}";

        if (puntajes[jugadorID] >= puntajeMaximo)
        {
            // ¡Se acaba el juego INMEDIATAMENTE!
            // No esperamos a que la IA lance.
            Debug.Log($"¡El jugador {jugadorID + 1} ha ganado el juego!");
            TerminarPartida(jugadorID);
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
            GameEvents.TriggerManoScored(idDelGanador);
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
        Debug.Log($"[DEBUG] ¡TejoTermino llamado por {tejo.gameObject.name}!");
        if (HabilidadManager.instance.imanBocinActivo)
        {
            Debug.Log("Desactivando Imán de Bocín después del tiro.");
            HabilidadManager.instance.imanBocinActivo = false;
            // Lo quitamos de la baraja
            Habilidad iman = HabilidadManager.instance.GetHabilidad("Imán de Bocín");
            if (iman != null) HabilidadManager.instance.QuitarHabilidad(iman);
        }
        bumeranExitoso = false; // Reseteamos la bandera de bumerán
        tejosDeLaRonda.Add(tejo);
        Debug.Log($"El tejo de {TurnManager.instance.CurrentTurn()} se ha detenido.");
        // 1. Comprobar dónde aterrizó el tejo
        bool estaEmbocinado = false;
        if (bocinTrigger != null)
        {
            estaEmbocinado = bocinTrigger.EstaTejoDentro(tejo);
        }
        else
        {
            Debug.LogError("¡BocinTrigger no está asignado en el GameManagerTejo!");
        }

        // 2. Comprobar si este tejo tocó una mecha (usando la variable que añadimos)
        bool tejoTocoMecha = tejo.haTocadoMecha;

        // 3. Obtener el ID del jugador actual
        int idJugadorActual = TurnManager.instance.CurrentPlayerIndex();
        bool scoreOcurrido = false;


        // 4. Evaluar los puntos en orden de prioridad (Moñona > Embocinado > Mecha)

        // CASO 1: MOÑONA (9 Puntos)
        // El tejo tocó una mecha (tejoTocoMecha) Y aterrizó dentro del bocín (estaEmbocinado)
        if (tejoTocoMecha && estaEmbocinado)
        {
            Debug.Log($"¡¡MOÑONA para Jugador {idJugadorActual + 1}!!");
            SumarPuntos(idJugadorActual, 9);
            scoreOcurrido = true;
            // Los puntos extra de habilidades (como Mecha Explosiva) ya se sumaron
            // gracias al HabilidadManager cuando la mecha explotó.
        }
        // CASO 2: EMBOCINADO (6 Puntos)
        // El tejo NO tocó mecha, PERO aterrizó en el bocín.
        else if (!tejoTocoMecha && estaEmbocinado)
        {
            Debug.Log($"¡EMBOCINADO para Jugador {idJugadorActual + 1}!");
            SumarPuntos(idJugadorActual, 6);
            scoreOcurrido = true;
        }
        // CASO 3: MECHA (3 Puntos, o los que sean)
        // El tejo SÍ tocó mecha, pero NO aterrizó en el bocín.
        // Usamos 'mechaExplotadaEnTurno' para asegurarnos de que el HabilidadManager
        // realmente registró la mecha en este turno.
        else if (mechaExplotadaEnTurno && idJugadorActual == idJugadorMecha && !estaEmbocinado)
        {
            Debug.Log($"¡MECHA para Jugador {idJugadorActual + 1}!");
            SumarPuntos(idJugadorActual, puntosBaseMecha); // Suma los puntos base (ej: 3)
            scoreOcurrido = true;
            // Puntos extra de habilidad ya sumados por HabilidadManager.
        }
        // CASO 4: Tiro normal (sin puntos)
        else
        {
            Debug.Log("Tiro sin puntos. Se calculará 'mano' al final de ronda si aplica.");
        }

        // Si ocurrió cualquier tipo de puntuación (Mecha, Embocinado o Moñona),
        // usamos tu función 'MarcarMechaExplotada' para anular el punto de "mano".
        if (scoreOcurrido)
        {
            MarcarMechaExplotada(); // Esto previene que se dé el punto de 'mano'
        }

        if (idJugadorActual == 0) // Solo para el jugador humano
        {
            // Lógica "Fiebre del Oro":
            Habilidad fiebre = HabilidadManager.instance.GetHabilidad("Fiebre del Oro");
            if (fiebre != null && fiebre.valorNumerico1 > 0)
            {
                // Si el tejo no puntuó (no está en bocín Y no hizo score)
                // Y ASUMIMOS que `tejo.EstaEnLaCancha()` es un bool que tienes en tu script Tejo.cs
                // Si no lo tienes, una aproximación es `!scoreOcurrido && !estaEmbocinado`
                // bool fallo = !scoreOcurrido && !estaEmbocinado; 
                // Por ahora, usaremos tu descripción "si el tejo no se queda en la cancha"
                // Necesitarás un trigger que detecte si el tejo se fue "fuera de límites".

                // --- Simplificación: "Fallo" = no sumar puntos ---
                bool falloElTiro = !scoreOcurrido && !estaEmbocinado;
                if (falloElTiro)
                {
                    Debug.Log("¡Fiebre del Oro perdida por fallo!");
                    HabilidadManager.instance.QuitarHabilidad(fiebre);
                }
                else
                {
                    // Si acertó, descontamos un uso
                    fiebre.valorNumerico1--;
                    Debug.Log($"Fiebre del Oro: {fiebre.valorNumerico1} lanzamientos restantes.");
                    if (fiebre.valorNumerico1 <= 0)
                    {
                        HabilidadManager.instance.QuitarHabilidad(fiebre);
                    }
                }
            }

            // Lógica "El Tejo Bumerán":
            // Si falló el tiro (misma condición de antes)
            if (!scoreOcurrido && !estaEmbocinado)
            {
                Habilidad bumeran = HabilidadManager.instance.GetHabilidad("El Tejo Bumerán");
                if (bumeran != null)
                {
                    // Leemos la probabilidad (valor1) del asset
                    if (UnityEngine.Random.Range(0f, 100f) <= bumeran.valorNumerico1)
                    {
                        Debug.Log("¡HABILIDAD: Tejo Bumerán! Tienes otro intento.");
                        bumeranExitoso = true;
                        HabilidadManager.instance.QuitarHabilidad(bumeran);
                        Destroy(tejo.gameObject);
                    }
                }
            }
        }

        // 5. Resetear las variables del TURNO
        mechaExplotadaEnTurno = false;
        idJugadorMecha = -1;
        puntosBaseMecha = 0;

        // --- FIN DE LA NUEVA LÓGICA DE PUNTUACIÓN ---
        if (CamaraSeguirTejo.instance != null)
        {
            CamaraSeguirTejo.instance.ForzarRetorno();
        }
        // 6. Iniciar la rutina para el siguiente jugador
        StartCoroutine(RutinaCambioDeTurno());

    }

    private IEnumerator RutinaCambioDeTurno()
    {
        // Usamos la variable que ya tenías para el delay.
        Debug.Log($"Esperando {delayCambioTurno} segundos antes de cambiar de turno...");
        yield return new WaitForSeconds(delayCambioTurno);
        if (estadoActual == GameState.PartidaTerminada)
        {
            yield break;
        }
        if (bumeranExitoso)
        {
            Debug.Log("Bumerán: Devolviendo tejo al jugador.");
            bumeranExitoso = false;
            CrearTejoJugador();
            if (blocker != null) blocker.SetActive(false); // Desbloquear input
        }
        // Si tenemos un tiro extra (Amaneciste con suerte) y es el jugador 0
        else if (tirosExtraEsteTurno > 0 && TurnManager.instance.CurrentPlayerIndex() == 0)
        {
            Debug.Log("Usando tiro extra (Amaneciste con suerte).");
            tirosExtraEsteTurno--;
            CrearTejoJugador();
            if (blocker != null) blocker.SetActive(false);
        }
        else
        {
            // Después de la pausa, le decimos al TurnManager que es el turno del siguiente.
            if (TurnManager.instance != null)
            {
                TurnManager.instance.NextTurn();
            }
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
        if (tejoJugadorPrefab == null || spawnJugador == null)
        {
            Debug.LogWarning(" No se pudo crear el tejo: prefab o spawn no asignado.");
            return;
        }

        GameObject nuevoTejo = Instantiate(tejoJugadorPrefab, spawnJugador.position, spawnJugador.rotation);
        nuevoTejo.GetComponent<Tejo>().jugadorID = 0;
        Debug.Log(" Nuevo tejo del jugador creado.");

        // Asignar el nuevo tejo al ControlJugador (para que pueda lanzarlo)
        var controlJugador = FindObjectOfType<ControlJugador>();
        if (controlJugador != null)
        {
            var lanzador = nuevoTejo.GetComponent<LanzamientoTejo>();
            controlJugador.AsignarTejoExistente(lanzador);
        }
        else
        {
            Debug.LogError("¡No se encontró 'ControlJugador' en la escena para asignarle el tejo!");
        }

        // Marcarlo como "listo para disparar"
        var scriptTejo = nuevoTejo.GetComponent<Tejo>();
        if (scriptTejo != null)
        {
            scriptTejo.ResetTejo();
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

        if (nuevoJugadorID == 0) // Es el turno del jugador humano
        {
            // 'rondasJugadas' se incrementa en RutinaFinDeRonda
            if (rondasJugadas >= 3)
            {
                Habilidad chanclete = HabilidadManager.instance.GetHabilidad("Chancleteè el motor");
                if (chanclete != null)
                {
                    // Leemos las rondas a esperar (valor1) del asset
                    if (rondasJugadas >= (int)chanclete.valorNumerico1)
                    {
                        if (puntajes[0] < puntajes[1])
                        {
                            Debug.Log("¡HABILIDAD: Chancleteè el motor!");
                            // Leemos los puntos a sumar (valor2) del asset
                            SumarPuntos(0, (int)chanclete.valorNumerico2);
                            HabilidadManager.instance.QuitarHabilidad(chanclete);
                        }
                    }
                }
            }
        }

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

        rondasJugadas++; // Incrementamos el contador de rondas jugadas
        Debug.Log($"Ronda {rondasJugadas} terminada.");

        // Esperamos un par de segundos para que el jugador vea el resultado
        yield return new WaitForSeconds(2f);

        // --- ¡NUEVO! CLÁUSULA DE GUARDA ---
        // Comprobamos si el último tiro (Mecha/Embocinado) ya hizo ganar a alguien.
        // Si es así, no calculamos "mano" y simplemente terminamos.
        if (estadoActual == GameState.PartidaTerminada)
        {
            yield break; // Detiene la corrutina aquí
        }

        // Si nadie ha ganado, calculamos el punto de "mano"
        DarPuntoAlMasCercano();

        // --- ¡NUEVO! SEGUNDA CLÁUSULA DE GUARDA ---
        // Comprobamos si el punto de "mano" hizo ganar a alguien.
        if (estadoActual == GameState.PartidaTerminada)
        {
            yield break; // Detiene la corrutina aquí
        }

        // --- LÓGICA DE VICTORIA ANTIGUA ELIMINADA ---
        // (El 'for' loop que buscaba al ganador se ha borrado)


        // Si llegamos aquí, es porque NADIE ha ganado todavía.
        Debug.Log("Nadie ha ganado todavía. Iniciando siguiente ronda.");
        LimpiarCancha();
        RespawnMechas(); // Asegúrate de que esto esté aquí
        tejosDeLaRonda.Clear();

        if (bocinTrigger != null)
            bocinTrigger.LimpiarLista();

        mechaExplotadaEnRonda = false;
        estadoActual = GameState.Jugando;
        turnosJugadosEnRonda = 0;
        yield return null;
        CrearTejoJugador();
        // Aquí puedes añadir lógica para limpiar los tejos viejos de la cancha.
    }

    /// <summary>
    /// Se llama cuando un jugador alcanza el puntaje máximo.
    /// </summary>
    /// <param name="ganadorID">0 = Humano, 1 = IA</param>
    private void TerminarPartida(int ganadorID)
    {
        estadoActual = GameState.PartidaTerminada;
        bool runHaTerminado = false;

        bool jugadorGano = (ganadorID == 0);
        Debug.Log(jugadorGano ? "¡PARTIDA TERMINADA! ¡Ganaste!" : "¡PARTIDA TERMINADA! ¡Perdiste!");

        if (GameLevelManager.instance != null)
        {
            if (MetaProgressionManager.instance != null)
            {
                int puntajeJugador = puntajes[0];
                MetaProgressionManager.instance.ConvertirPuntajeEnDinero(puntajeJugador, jugadorGano);
            }
            else
            {
                Debug.LogWarning("No se encontró MetaProgressionManager en la escena.");
            }

            if (jugadorGano)
                runHaTerminado = GameLevelManager.instance.RegistrarVictoria();
            else
                GameLevelManager.instance.RegistrarDerrota();
        }

        if (runHaTerminado)
        {
            Debug.Log("¡EL RUN HA TERMINADO! No se muestra el reward screen normal.");
        }
        else if (rewardScreen != null)
        {
            rewardScreen.MostrarRecompensas();
        }
        else
        {
            Debug.LogError("RewardScreen no está asignado en el GameManager!");
        }
    } // <-- La llave que cierra la función

    public void ReiniciarParaNuevaPartida()
    {
        Debug.Log("Reiniciando el juego para una nueva partida...");

        if (gestorIA != null)
        {
            // Obtenemos el nivel actual. Si el jugador acaba de ganar,
            // TerminarPartida() ya debería haberlo incrementado.
            int nivelActual = GameLevelManager.instance.nivelActual;
            gestorIA.IniciarNivel(nivelActual);
        }
        else
        {
            Debug.LogError("¡'GestorDeNivelesIA' no está asignado en el GameManagerTejo!");
        }
        // 1. Reiniciar puntajes y UI
        for (int i = 0; i < puntajes.Length; i++)
        {
            puntajes[i] = 0;
            if (puntajeTextos[i] != null) // Actualizamos la UI a 0
                puntajeTextos[i].text = $"J{i + 1}: 0";
        }

        // 2. Limpiar la cancha y estados de ronda
        LimpiarCancha();
        RespawnMechas();
        if (bocinTrigger != null)
            bocinTrigger.LimpiarLista(); // Limpia el trigger del bocin

        tejosDeLaRonda.Clear();
        mechaExplotadaEnRonda = false;
        turnosJugadosEnRonda = 0;
        rondasJugadas = 0; // ¡Importante reiniciar el contador de rondas!

        // 3. Reiniciar el turno al JUGADOR 0 (Humano)
        // Tu log muestra "Turno forzado al Jugador 1", aquí lo corregimos.
        TurnManager.instance.SetTurn(1);

        // 4. Volver al estado de "Jugando"
        estadoActual = GameState.Jugando;

        // 5. ¡LA PARTE QUE FALTABA! Crear el tejo para el jugador
        // Esto reemplaza el 'CrearTejoConDelay' que tenías en Start()
        CrearTejoJugador();

        // 6. Mover la lógica de "Amaneciste con suerte" aquí
        tirosExtraEsteTurno = 0; // Reseteamos por si acaso
        Habilidad hab = HabilidadManager.instance.GetHabilidad("Amaneciste con suerte");
        if (hab != null)
        {
            // Leemos la probabilidad (valor1) del asset
            if (UnityEngine.Random.Range(0f, 100f) <= hab.valorNumerico1)
            {
                Debug.Log("¡HABILIDAD: Amaneciste con suerte!");
                // Leemos los tiros extra (valor2) del asset
                tirosExtraEsteTurno = (int)hab.valorNumerico2;
            }
            HabilidadManager.instance.QuitarHabilidad(hab);
        }
    } // <-- ¡¡LA LLAVE DE CIERRE DE LA FUNCIÓN FALTABA AQUÍ!!

    public void RegistrarMecha(int jugadorID, int puntosBase)
    {
        Debug.Log($"Registrando mecha para Jugador {jugadorID + 1} con {puntosBase} puntos base.");
        mechaExplotadaEnTurno = true;
        idJugadorMecha = jugadorID;
        puntosBaseMecha = puntosBase;

        // Nota: HabilidadManager ya llama a MarcarMechaExplotada(),
        // así que la regla de "mano" ya está anulada para esta ronda.
    }
    public void MarcarMechaExplotada()
    {
        mechaExplotadaEnRonda = true;
    }
    public void ColocarMechasFalsas()
    {
        if (prefabMechaFalsa == null || posicionesMechasFalsas.Length == 0)
        {
            Debug.LogWarning("Prefab de 'Mecha Falsa' o sus posiciones no están asignados en el GameManager!");
            return;
        }

        // Coloca una mecha señuelo en una de las posiciones definidas
        int index = UnityEngine.Random.Range(0, posicionesMechasFalsas.Length);
        Instantiate(prefabMechaFalsa, posicionesMechasFalsas[index].position, Quaternion.identity);
        Debug.Log("Mecha Falsa colocada.");
    }
    /// <summary>
    /// Recolecta TODOS los datos del estado actual y los guarda en el JSON.
    /// Llama a esto desde tu botón "Guardar y Salir" del menú de pausa.
    /// </summary>
    public void GuardarPartidaActual()
    {
        Debug.Log("Guardando partida...");
        GameData data = new GameData();

        // 1. Recolectar datos
        data.nivelActual = GameLevelManager.instance.nivelActual;
        data.puntajes = this.puntajes;
        data.rondasJugadas = this.rondasJugadas;
        data.mechaExplotadaEnRonda = this.mechaExplotadaEnRonda;
        data.jugadorActual = TurnManager.instance.CurrentTurn();

        // 2. Recolectar datos de habilidades (usando la nueva función)
        data.barajaHabilidades = HabilidadManager.instance.GetDatosDeBaraja();

        // 3. Guardar los tejos en la cancha
        data.tejosEnCancha = new List<TejoData>();
        foreach (Tejo tejo in tejosDeLaRonda) // 'tejosDeLaRonda' es tu variable
        {
            if (tejo != null) // Asegurarse de que el tejo no fue destruido
            {
                data.tejosEnCancha.Add(new TejoData(
                    tejo.jugadorID,
                    tejo.transform.position,
                    tejo.transform.rotation
                ));
            }
        }

        // 4. Usar el SaveManager
        SaveManager.SaveGame(data);
    }

    /// <summary>
    /// Restaura todo el estado del juego desde un objeto GameData.
    /// Es llamado por Start() si existe un guardado.
    /// </summary>
    private void CargarPartidaGuardada(GameData data)
    {
        // 1. Cargar Nivel
        GameLevelManager.instance.nivelActual = data.nivelActual;
        // (Tu AIController y GameLevelManager parecen leer el nivel
        // en sus propios Start/Awake, así que esto debería funcionar)

        // 2. Cargar Puntajes y UI
        puntajes = data.puntajes;
        for (int i = 0; i < puntajes.Length; i++)
        {
            if (i < puntajeTextos.Length && puntajeTextos[i] != null)
                puntajeTextos[i].text = $"J{i + 1}: {puntajes[i]}";
        }

        // 3. Cargar Estado de Ronda
        rondasJugadas = data.rondasJugadas;
        mechaExplotadaEnRonda = data.mechaExplotadaEnRonda;

        // 4. Cargar Baraja (usando la nueva función)
        HabilidadManager.instance.CargarBarajaDesdeDatos(data.barajaHabilidades);

        // 5. Cargar Tejos en Cancha (re-instanciarlos)
        LimpiarCancha();
        tejosDeLaRonda.Clear();
        foreach (TejoData tejoData in data.tejosEnCancha)
        {
            // Elige el prefab correcto según el ID
            GameObject prefab = (tejoData.jugadorID == 0) ? tejoJugadorPrefab : tejoIAPrefab;

            if (prefab != null)
            {
                GameObject tejoGO = Instantiate(prefab, tejoData.position, tejoData.rotation);
                Tejo tejoScript = tejoGO.GetComponent<Tejo>();
                tejoScript.jugadorID = tejoData.jugadorID;

                // (Necesitarás una función en 'Tejo.cs' para que se quede quieto
                // y no active físicas al spawnear)
                // ej: tejoScript.SetAsStationary(); 

                tejosDeLaRonda.Add(tejoScript);
            }
        }

        // 6. Forzar el turno
        estadoActual = GameState.Jugando;
        TurnManager.instance.SetTurn(data.jugadorActual);

        // 7. IMPORTANTE: Crear el tejo para el jugador si es su turno
        if (data.jugadorActual == 1) // 1 = Humano
        {
            CrearTejoJugador();
            if (blocker != null) blocker.SetActive(false); // Activar input
        }
        // Si es 2 (IA), el AIController.OnTurnChanged se activará solo.
    }

    /// <summary>
    /// Limpia las mechas viejas, crea nuevas en posiciones aleatorias
    /// y actualiza la lista de objetivos de la IA.
    /// </summary>
    private void RespawnMechas()
    {
        // 1. Limpiar las mechas de la ronda anterior
        foreach (GameObject mecha in mechasActivas)
        {
            Destroy(mecha);
        }
        mechasActivas.Clear();

        // 2. Encontrar la IA y limpiar sus objetivos
        AIController ai = FindObjectOfType<AIController>();
        if (ai != null)
        {
            ai.objetivos.Clear();
        }

        if (prefabMecha == null || bocin == null)
        {
            Debug.LogError("¡PrefabMecha o Bocin no están asignados en el GameManagerTejo!");
            return;
        }

        Vector3 centroBocin = bocin.position;
        Vector2 bocinXZ = new Vector2(centroBocin.x, centroBocin.z);
        Debug.Log($"[GameManager] Spawneando {cantidadDeMechas} mechas...");

        // 3. Crear nuevas mechas
        for (int i = 0; i < cantidadDeMechas; i++)
        {
            int intentos = 0;
            bool puntoValidoEncontrado = false;

            // Intentaremos hasta 20 veces encontrar un punto válido
            while (intentos < 20 && !puntoValidoEncontrado)
            {
                intentos++;

                // Calcular una posición X, Z aleatoria
                float posX = Random.Range(-areaDeSpawn.x / 2, areaDeSpawn.x / 2);
                float posZ = Random.Range(-areaDeSpawn.y / 2, areaDeSpawn.y / 2);

                // --- ¡NUEVA COMPROBACIÓN DE ZONA MUERTA! ---
                Vector2 puntoSpawnXZ = new Vector2(centroBocin.x + posX, centroBocin.z + posZ);

                // Calculamos la distancia horizontal al bocín
                float distancia = Vector2.Distance(puntoSpawnXZ, bocinXZ);

                // Si está muy cerca, ignoramos este punto y probamos de nuevo
                if (distancia < radioZonaMuerta)
                {
                    continue; // Vuelve al inicio del "while"
                }
                // --- FIN DE LA COMPROBACIÓN ---


                // Si llegamos aquí, el punto está fuera de la zona muerta.
                // Ahora lanzamos el rayo para encontrar el suelo.
                Vector3 rayStartPos = new Vector3(centroBocin.x + posX, centroBocin.y + 20f, centroBocin.z + posZ);

                RaycastHit hit;
                if (Physics.Raycast(rayStartPos, Vector3.down, out hit, 50f, canchaLayerMask))
                {
                    Vector3 spawnPos = hit.point + new Vector3(0, alturaSobreElSuelo, 0);
                    Quaternion spawnRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

                    GameObject nuevaMecha = Instantiate(prefabMecha, spawnPos, spawnRotation);
                    mechasActivas.Add(nuevaMecha);

                    if (ai != null)
                    {
                        ai.objetivos.Add(nuevaMecha.transform);
                    }

                    puntoValidoEncontrado = true; // ¡Éxito! Salimos del "while"
                }
                else
                {
                    // El raycast falló (cayó fuera de la cancha), probamos de nuevo
                    continue;
                }
            } // Fin del while

            if (!puntoValidoEncontrado)
            {
                Debug.LogWarning($"No se pudo encontrar un punto válido para la mecha {i + 1} tras 20 intentos.");
            }

        } // Fin del for
    }
}
