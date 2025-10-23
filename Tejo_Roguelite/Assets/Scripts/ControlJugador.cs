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

    // --- Lógica de TurnManager ---
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
        puedeLanzar = false;
        tejoActual = null;
    }
    #endregion

    // --- Lógica de Update (Sin cambios) ---
    void Update()
    {
        if (GameManagerTejo.instance != null && GameManagerTejo.instance.estadoActual != GameManagerTejo.GameState.Jugando)
        {
            return;
        }

        if (TurnManager.instance == null || !TurnManager.instance.IsHumanTurn()) return;
        if (!puedeLanzar) return;

        switch (estado)
        {
            case EstadoLanzamiento.Inactivo:
                ActualizarPuntoDestino();
                if (Input.GetMouseButtonDown(0))
                {
                    valorBarra = 0f;
                    barraSubiendo = true;
                    barraDeFuerzaSlider.gameObject.SetActive(true);
                    estado = EstadoLanzamiento.CargandoPoder;
                }
                break;

            case EstadoLanzamiento.CargandoPoder:
                float velocidadActualBarra = velocidadBarra;
                Habilidad aguardiente = HabilidadManager.instance.GetHabilidad("Aguardiente Doble Filo");
                if (HabilidadManager.instance.aguardienteActivo && aguardiente != null)
                {
                    // Leemos el multiplicador de velocidad (valor1) del asset
                    velocidadActualBarra *= aguardiente.valorNumerico1; // ej: 1.5f * 0.7f
                }
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

    // --- Corrutina de Lanzamiento (AQUÍ AÑADIMOS la cámara con retardo) ---
    private IEnumerator LanzarTejoSincronizado(float valorLanzamiento)
    {
        if (tejoActual == null) yield break;

        barraDeFuerzaSlider.gameObject.SetActive(false);
        if (fillImage != null)
            fillImage.color = colorDefaultFill;

        float poder = valorLanzamiento;
        float fuerza = poder * multiplicadorDeFuerza;

        float distanciaDelCentro = Mathf.Abs(valorLanzamiento - 0.5f);
        float precision = distanciaDelCentro / 0.5f;
        float desviacionActual = maxDesviacionAngular;

        Habilidad aguardiente = HabilidadManager.instance.GetHabilidad("Aguardiente Doble Filo");
        if (HabilidadManager.instance.aguardienteActivo && aguardiente != null)
        {
            // Leemos el multiplicador de desviación (valor2) del asset
            desviacionActual *= aguardiente.valorNumerico2; // ej: 15f * 0.5f

            Debug.Log("Desactivando Aguardiente después del tiro.");
            HabilidadManager.instance.aguardienteActivo = false;
            HabilidadManager.instance.QuitarHabilidad(aguardiente);
        }

        Vector3 direccion = puntoDestino - puntoDeLanzamiento.position;
        Vector3 direccionBase = new Vector3(direccion.x, 0, direccion.z).normalized;
        direccionBase.y = alturaDelArco;

        float anguloDesviacion = precision * maxDesviacionAngular;
        if (Random.value < 0.5f) anguloDesviacion *= -1f;
        Vector3 direccionFinal = Quaternion.Euler(0, anguloDesviacion, 0) * direccionBase;

        yield return null;
        yield return new WaitForFixedUpdate();

        tejoActual.Iniciar(puntoDeLanzamiento.position, direccionFinal, fuerza);

        Tejo tejoComp = tejoActual.GetComponent<Tejo>();
        if (tejoComp != null)
            tejoComp.ActivarDeteccion();

        if (GameManagerTejo.instance != null)
            GameManagerTejo.instance.RegistrarTejoLanzado();

        Debug.Log($"[Jugador] Lanzado! Valor: {valorLanzamiento:F2}, Poder: {poder:F2}, Precisión: {precision:F2}, Fuerza: {fuerza:F2}");

        // === NUEVO: Activar cámara diagonal tras 0.5 segundos ===
        yield return new WaitForSeconds(0.5f);
        CamaraSeguirTejo camSeg = FindObjectOfType<CamaraSeguirTejo>();
        if (camSeg != null)
        {
            camSeg.SeguirTejo(tejoActual.transform);
            Debug.Log("[Jugador] Cámara de seguimiento activada tras 0.5s.");
        }
        else
        {
            Debug.LogWarning("[Jugador] No se encontró CamaraSeguirTejo en la escena.");
        }

        tejoActual = null;
        puedeLanzar = false;
    }

    // --- Preparar Tejo ---
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