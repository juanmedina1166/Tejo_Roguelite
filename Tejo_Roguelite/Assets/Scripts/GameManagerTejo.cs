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

    private int tirosRealizados = 0;
    private int cambiosDeTurno = 0;
    private bool esperandoCambioTurno = false;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    public void SumarPuntos(int jugadorID, int puntos)
    {
        Debug.Log($"Jugador {jugadorID + 1} gana {puntos} puntos");
        puntajes[jugadorID] += puntos;
        if (puntajeTextos != null && jugadorID >= 0 && jugadorID < puntajeTextos.Length)
            puntajeTextos[jugadorID].text = $"J{jugadorID + 1}: {puntajes[jugadorID]}";

        if (puntajes[jugadorID] >= puntajeMaximo)
        {
            // Aquí iría la lógica de victoria
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

    // --- ¡AQUÍ ESTÁ EL MÉTODO QUE FALTABA! ---
    public void AvisarCambioTurno()
    {
        cambiosDeTurno++;
        Debug.Log($"Cambio de turno #{cambiosDeTurno}");

        // Aquí puedes agregar lógica si el juego termina después de un número de rondas
    }

    public void RegistrarTejoLanzado()
    {
        tirosRealizados++;
        if (tirosRealizados >= maxTiros)
        {
            if (blocker != null) blocker.SetActive(true);
            esperandoCambioTurno = true;
        }
    }

    public void TejoTermino(Tejo tejo)
    {
        // NOTA 3D: Asegúrate de que tu script "Tejo.cs" llame a este método cuando el Rigidbody 3D se detenga.
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

        // Esta línea es la que daba el error. Ahora funcionará.
        AvisarCambioTurno();

        MultiJoystickControl multi = FindObjectOfType<MultiJoystickControl>();
        if (multi != null)
            multi.PrepareForNextRound();

        CentroController centro = FindObjectOfType<CentroController>();
        if (centro != null)
        {
            // NOTA 3D: El objeto del Centro ahora debe tener un Collider 3D.
            var col3D = centro.GetComponent<Collider>();
            if (col3D != null) col3D.enabled = true;

            var renderer = centro.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = true;

            centro.MoverCentro();
        }

        tirosRealizados = 0;
        if (blocker != null) blocker.SetActive(false);
    }
}
