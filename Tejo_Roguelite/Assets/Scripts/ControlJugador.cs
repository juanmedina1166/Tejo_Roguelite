using UnityEngine;
using UnityEngine.UI; // Importante
using System.Collections;

public class ControlJugador : MonoBehaviour
{
    [Header("Referencias Esenciales")]
    public Camera mainCamera;
    public LanzamientoTejo tejoPrefab;
    public Transform puntoDeLanzamiento;
    public Slider barraDeFuerzaSlider; // Sigue siendo tu Slider

    [Header("Configuración de Lanzamiento")]
    public float multiplicadorDeFuerza = 60f;
    public float velocidadBarra = 1.5f; // Velocidad de la barra oscilante
    public float maxDesviacionAngular = 15f;
    public float alturaDelArco = 0.8f;

    private LanzamientoTejo tejoActual;
    private bool puedeLanzar = true;

    // --- Estados (simplificados) ---
    private enum EstadoLanzamiento { Inactivo, CargandoPoder }
    private EstadoLanzamiento estado = EstadoLanzamiento.Inactivo;

    private float valorBarra = 0f;
    private bool barraSubiendo = true; // Para controlar la oscilación
    private Vector3 puntoDestino; // Dirección fijada al hacer click

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
            barraDeFuerzaSlider.gameObject.SetActive(false); // Ocultar barra al inicio

        if (TurnManager.instance == null || TurnManager.instance.IsHumanTurn())
        {
            PrepararNuevoTejo();
        }
    }

    void OnTurnChanged(int jugador)
    {
        if (TurnManager.instance != null && TurnManager.instance.IsHumanTurn())
        {
            PrepararNuevoTejo();
            puedeLanzar = true;
            estado = EstadoLanzamiento.Inactivo; // Reiniciar estado
        }
        else
        {
            puedeLanzar = false;
        }
    }
    #endregion

    // --- Lógica de Update (REHECHA) ---
    void Update()
    {
        if (TurnManager.instance == null || !TurnManager.instance.IsHumanTurn()) return;
        if (!puedeLanzar) return;

        // --- Máquina de Estados del Lanzamiento ---
        switch (estado)
        {
            case EstadoLanzamiento.Inactivo:
                // --- 1. Apuntar (antes de hacer click) ---
                ActualizarPuntoDestino();

                // Si el jugador PRESIONA el botón
                if (Input.GetMouseButtonDown(0))
                {
                    // Fijamos la dirección que tenía en ese instante
                    // (ActualizarPuntoDestino() se llamó justo antes)

                    // Empezar a cargar poder
                    valorBarra = 0f;
                    barraSubiendo = true;
                    barraDeFuerzaSlider.gameObject.SetActive(true);
                    estado = EstadoLanzamiento.CargandoPoder;
                }
                break;

            case EstadoLanzamiento.CargandoPoder:
                // --- 2. Cargar Poder (mientras mantiene presionado) ---
                // La barra oscila (sube y baja)
                if (barraSubiendo)
                {
                    valorBarra += velocidadBarra * Time.deltaTime;
                    if (valorBarra >= 1f)
                    {
                        valorBarra = 1f;
                        barraSubiendo = false;
                    }
                }
                else
                {
                    valorBarra -= velocidadBarra * Time.deltaTime;
                    if (valorBarra <= 0f)
                    {
                        valorBarra = 0f;
                        barraSubiendo = true;
                    }
                }

                barraDeFuerzaSlider.value = valorBarra;

                // --- 3. Lanzar (al soltar el botón) ---
                if (Input.GetMouseButtonUp(0))
                {
                    // ¡LANZAR!
                    // El 'valorBarra' actual (0-1) determinará TODO
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
        {
            puntoDestino = hitInfo.point;
        }
    }

    // --- Lanzamiento (Modificado para usar un solo valor) ---
    private IEnumerator LanzarTejoSincronizado(float valorLanzamiento) // Ahora solo recibe un valor (0-1)
    {
        if (tejoActual == null)
        {
            Debug.LogError("Se intentó lanzar pero no había tejo actual.");
            yield break;
        }

        // Ocultar la barra
        barraDeFuerzaSlider.gameObject.SetActive(false);
        barraDeFuerzaSlider.value = 0;

        // --- 1. Calcular Poder ---
        // El poder es directamente el valor de la barra
        float poder = valorLanzamiento;
        float fuerza = poder * multiplicadorDeFuerza;

        // --- 2. Calcular Precisión ---
        // Hacemos que la precisión sea MEJOR mientras MÁS PODER tenga.
        // Si valorLanzamiento es 1.0 (perfecto), la precisión es 0.0 (sin error).
        // Si valorLanzamiento es 0.1 (malo), la precisión es 0.9 (mucho error).
        float precision = 1.0f - valorLanzamiento;

        // --- 3. Calcular Dirección Base (con la dirección guardada) ---
        Vector3 direccion = puntoDestino - puntoDeLanzamiento.position;
        Vector3 direccionBase = new Vector3(direccion.x, 0, direccion.z).normalized;
        direccionBase.y = alturaDelArco; // Aplicamos tu altura de arco

        // --- 4. Aplicar Desviación (La parte RDR) ---
        float anguloDesviacion = precision * maxDesviacionAngular;
        if (Random.value < 0.5f)
        {
            anguloDesviacion *= -1f;
        }
        Vector3 direccionFinal = Quaternion.Euler(0, anguloDesviacion, 0) * direccionBase;

        // --- 5. Lanzar (Tu lógica de sincronización) ---
        yield return null; // Sincronizar frame
        yield return new WaitForFixedUpdate(); // Sincronizar física

        tejoActual.Iniciar(puntoDeLanzamiento.position, direccionFinal, fuerza);

        Tejo tejoComp = tejoActual.GetComponent<Tejo>();
        if (tejoComp != null)
            tejoComp.ActivarDeteccion();

        if (GameManagerTejo.instance != null)
            GameManagerTejo.instance.RegistrarTejoLanzado();

        Debug.Log($" [Jugador] Lanzado! Valor: {valorLanzamiento:F2} (Poder: {poder:F2}, Precisión: {precision:F2}), Fuerza: {fuerza:F2}");
        tejoActual = null;
        puedeLanzar = false;
    }

    // --- Lógica de Preparar Tejo (Sin Cambios) ---
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
        Debug.Log("ControlJugador: nuevo tejo asignado correctamente.");
    }
    #endregion
}