using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ControlJugador : MonoBehaviour
{
    [Header("Referencias Esenciales")]
    public Camera mainCamera;
    public LanzamientoTejo tejoPrefab;
    public Transform puntoDeLanzamiento;
    public Slider barraDeFuerzaSlider;

    [Header("Configuración de Lanzamiento")]
    [Tooltip("La fuerza MÁXIMA (cuando la barra está al 100%)")]
    public float multiplicadorDeFuerza = 60f; // Sincroniza con maxLaunchSpeed
    [Tooltip("Velocidad de la barra oscilante")]
    public float velocidadBarra = 1.5f;
    [Tooltip("El error MÁXIMO en grados si fallas por completo")]
    public float maxDesviacionAngular = 15f;
    public float alturaDelArco = 0.8f;

    [Header("Feedback Visual")]
    [Tooltip("Gradiente (Ej: Rojo en 0, Verde en 0.5, Rojo en 1)")]
    public Gradient powerGradient;
    private Image fillImage;
    private Color colorDefaultFill;

    // --- Variables de Estado ---
    private LanzamientoTejo tejoActual;
    private bool puedeLanzar = true;
    private enum EstadoLanzamiento { Inactivo, CargandoPoder }
    private EstadoLanzamiento estado = EstadoLanzamiento.Inactivo;
    private float valorBarra = 0f;
    private bool barraSubiendo = true; // Para controlar la OSCILACIÓN
    private Vector3 puntoDestino;

    // --- Lógica de TurnManager (Sin Cambios) ---
    #region TurnManager
    void OnEnable()
    {
        if (TurnManager.instance != null)
            TurnManager.instance.OnTurnChanged += OnTurnChanged;
        else
            StartCoroutine(WaitAndSubscribe());
    }

    void OnDisable()
    {
        if (TurnManager.instance != null)
            TurnManager.instance.OnTurnChanged -= OnTurnChanged;
    }

    IEnumerator WaitAndSubscribe()
    {
        while (TurnManager.instance == null)
            yield return null;
        TurnManager.instance.OnTurnChanged += OnTurnChanged;
    }

    void Start()
    {
        if (barraDeFuerzaSlider != null)
        {
            barraDeFuerzaSlider.gameObject.SetActive(false);
            if (barraDeFuerzaSlider.fillRect != null)
            {
                fillImage = barraDeFuerzaSlider.fillRect.GetComponent<Image>();
                if (fillImage != null)
                    colorDefaultFill = fillImage.color;
            }
        }
        if (TurnManager.instance == null || TurnManager.instance.IsHumanTurn())
            PrepararNuevoTejo();
    }

    void OnTurnChanged(int jugador)
    {
        // Cada vez que cambia el turno (sea de quien sea),
        // le quitamos el permiso de lanzar.
        puedeLanzar = false;
        tejoActual = null; // También limpiamos la referencia al tejo viejo

        // El GameManager será quien nos llame "AsignarTejoExistente()"
        // y nos devuelva "puedeLanzar = true" CUANDO sea el momento correcto.
    }
    #endregion

    // --- Lógica de Update (CON BARRA OSCILANTE) (Sin Cambios) ---
    void Update()
    {
        if (GameManagerTejo.instance != null && GameManagerTejo.instance.estadoActual != GameManagerTejo.GameState.Jugando)
        {
            return;
        }
        // ------------------------------------------

        // El resto de tu código de Update
        if (TurnManager.instance == null || !TurnManager.instance.IsHumanTurn()) return;
        if (!puedeLanzar) return;

        switch (estado)
        {
            case EstadoLanzamiento.Inactivo:
                ActualizarPuntoDestino();
                if (Input.GetMouseButtonDown(0))
                {
                    valorBarra = 0f;
                    barraSubiendo = true; // Empezar subiendo
                    barraDeFuerzaSlider.gameObject.SetActive(true);
                    estado = EstadoLanzamiento.CargandoPoder;
                }
                break;

            case EstadoLanzamiento.CargandoPoder:
                // --- La barra OSCILA (sube y baja) ---
                if (barraSubiendo)
                {
                    valorBarra += velocidadBarra * Time.deltaTime;
                    if (valorBarra >= 1f) { valorBarra = 1f; barraSubiendo = false; }
                }
                else
                {
                    valorBarra -= velocidadBarra * Time.deltaTime;
                    if (valorBarra <= 0f) { valorBarra = 0f; barraSubiendo = true; }
                }

                barraDeFuerzaSlider.value = valorBarra;
                if (fillImage != null)
                    fillImage.color = powerGradient.Evaluate(valorBarra);

                // --- Lanzar AL SOLTAR ---
                if (Input.GetMouseButtonUp(0))
                {
                    StartCoroutine(LanzarTejoSincronizado(valorBarra));
                    estado = EstadoLanzamiento.Inactivo;
                }
                break;
        }
    }

    private void ActualizarPuntoDestino()
    {
        Ray rayo = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(rayo, out hitInfo))
            puntoDestino = hitInfo.point;
    }

    // --- Corrutina de Lanzamiento (¡AQUÍ ESTÁ EL CAMBIO!) ---
    private IEnumerator LanzarTejoSincronizado(float valorLanzamiento) // valorLanzamiento (0-1)
    {
        if (tejoActual == null) yield break;

        barraDeFuerzaSlider.gameObject.SetActive(false);
        if (fillImage != null)
            fillImage.color = colorDefaultFill;

        // --- INICIO DE LA LÓGICA HÍBRIDA ---

        // 1. Calcular el PODER (Fuerza) - Es LINEAL
        // La fuerza es directamente el valor de la barra.
        // 0.0 = mínima, 0.5 = "adecuada", 1.0 = máxima.
        float poder = valorLanzamiento;
        float fuerza = poder * multiplicadorDeFuerza;

        // 2. Calcular la PRECISIÓN (Error) - Es PARABÓLICA
        // Se basa en el punto dulce (0.5).
        // 'distanciaDelCentro' (0.0 = perfecto, 0.5 = peor)
        float distanciaDelCentro = Mathf.Abs(valorLanzamiento - 0.5f);

        // 'precision' (error) va de 0.0 (perfecto) a 1.0 (peor).
        // Se normaliza dividiendo por la distancia máxima (0.5).
        float precision = distanciaDelCentro / 0.5f;

        // --- FIN DE LA LÓGICA ---

        // --- El resto de tu código (sin cambios) ---
        Vector3 direccion = puntoDestino - puntoDeLanzamiento.position;
        Vector3 direccionBase = new Vector3(direccion.x, 0, direccion.z).normalized;
        direccionBase.y = alturaDelArco;

        float anguloDesviacion = precision * maxDesviacionAngular;
        if (Random.value < 0.5f) { anguloDesviacion *= -1f; }
        Vector3 direccionFinal = Quaternion.Euler(0, anguloDesviacion, 0) * direccionBase;

        yield return null;
        yield return new WaitForFixedUpdate();

        tejoActual.Iniciar(puntoDeLanzamiento.position, direccionFinal, fuerza);

        Tejo tejoComp = tejoActual.GetComponent<Tejo>();
        if (tejoComp != null)
            tejoComp.ActivarDeteccion();

        if (GameManagerTejo.instance != null)
            GameManagerTejo.instance.RegistrarTejoLanzado();

        Debug.Log($" [Jugador] Lanzado! Valor: {valorLanzamiento:F2}, Poder: {poder:F2}, Precisión(Error): {precision:F2}, Fuerza: {fuerza:F2}");
        tejoActual = null;
        puedeLanzar = false;
    }

    // --- Preparar Tejo (Sin Cambios) ---
    #region PrepararTejo
    public void PrepararNuevoTejo()
    {
        if (tejoActual != null) return;
        if (TurnManager.instance != null && !TurnManager.instance.IsHumanTurn()) return;
        if (tejoPrefab != null)
        {
            tejoActual = Instantiate(tejoPrefab, puntoDeLanzamiento.position, puntoDeLanzamiento.rotation);
            puedeLanzar = true;
        }
    }

    public void AsignarTejoExistente(LanzamientoTejo nuevoTejo)
    {
        tejoActual = nuevoTejo;
        puedeLanzar = true;
    }
    #endregion
}