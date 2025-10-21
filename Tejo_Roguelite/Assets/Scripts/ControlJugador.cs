using UnityEngine;
using System.Collections;

public class ControlJugador : MonoBehaviour
{
    [Header("Referencias Esenciales")]
    public Camera mainCamera;
    public LanzamientoTejo tejoPrefab;
    public Transform puntoDeLanzamiento;
    public BarraDeFuerza barraDeFuerza;

    [Header("Configuración de Lanzamiento")]
    public float alturaDelArco = 0.8f;
    public float multiplicadorDeFuerza = 60f;

    private LanzamientoTejo tejoActual;
    private bool puedeLanzar = true;

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
        }
        else
        {
            puedeLanzar = false;
        }
    }

    void Update()
    {
        if (TurnManager.instance == null || !TurnManager.instance.IsHumanTurn()) return;
        if (!puedeLanzar) return;

        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(LanzarTejoSincronizado());
        }
    }

    private IEnumerator LanzarTejoSincronizado()
    {
        // --- Paso 1: Calcular la Dirección con Raycasting ---
        Ray rayo = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(rayo, out hitInfo))
        {
            Vector3 puntoDestino = hitInfo.point;
            float fuerza = barraDeFuerza.GetValorFuerza() * multiplicadorDeFuerza;

            Vector3 direccion = puntoDestino - puntoDeLanzamiento.position;
            Vector3 direccionDeLanzamiento = new Vector3(direccion.x, 0, direccion.z).normalized;
            direccionDeLanzamiento.y = alturaDelArco;

            if (tejoActual != null)
            {
                yield return null;
                Debug.Log(" [Jugador] Esperando FixedUpdate antes de lanzar...");
                yield return new WaitForFixedUpdate();

                // === LANZAMIENTO FÍSICO DEL TEJO ===
                tejoActual.Iniciar(puntoDeLanzamiento.position, direccionDeLanzamiento, fuerza);

                //  PASO 2: ACTIVAR LA CÁMARA DE SEGUIMIENTO 
                var camara = FindObjectOfType<CamaraSeguirTejo>();
                if (camara != null)
                {
                    camara.SeguirTejo(tejoActual.transform);
                    Debug.Log(" Cámara ahora siguiendo al tejo.");
                }

                // Activar detección de colisiones del tejo
                Tejo tejoComp = tejoActual.GetComponent<Tejo>();
                if (tejoComp != null)
                    tejoComp.ActivarDeteccion();

                if (GameManagerTejo.instance != null)
                    GameManagerTejo.instance.RegistrarTejoLanzado();

                Debug.Log(" [Jugador] Lanzamiento sincronizado completado.");
                tejoActual = null;
                puedeLanzar = false;
            }
        }
        else
        {
            Debug.Log("Apuntando a un lugar inválido.");
        }
    }

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
}

