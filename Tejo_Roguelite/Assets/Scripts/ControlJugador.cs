using UnityEngine;

public class ControlJugador : MonoBehaviour
{
    [Header("Referencias Esenciales")]
    public Camera mainCamera;           // La cámara principal de la escena
    public LanzamientoTejo tejoPrefab;  // El Prefab de tu tejo para crearlo
    public Transform puntoDeLanzamiento; // Un objeto vacío que marca de dónde sale el tejo
    public BarraDeFuerza barraDeFuerza; // Referencia a tu barra de fuerza

    [Header("Configuración de Lanzamiento")]
    [Tooltip("Controla qué tan alto será el arco del tiro. Un valor entre 0.5 y 1.5 funciona bien.")]
    public float alturaDelArco = 0.8f;
    [Tooltip("Ajusta la potencia general del lanzamiento.")]
    public float multiplicadorDeFuerza = 60f;

    private LanzamientoTejo tejoActual;
    private bool puedeLanzar = true; // Para evitar lanzamientos múltiples

    void OnEnable()
    {
        // Suscribimos para crear el tejo cuando empiece el turno humano
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

    System.Collections.IEnumerator WaitAndSubscribe()
    {
        while (TurnManager.instance == null)
            yield return null;
        TurnManager.instance.OnTurnChanged += OnTurnChanged;
    }

    void Start()
    {
        // Sólo preparamos el tejo si es turno humano en inicio (evita pre-instantiar cuando empieza IA)
        if (TurnManager.instance == null || TurnManager.instance.IsHumanTurn())
        {
            PrepararNuevoTejo();
        }
    }

    void OnTurnChanged(int jugador)
    {
        // Si ahora es el turno humano, preparamos el tejo
        if (TurnManager.instance != null && TurnManager.instance.IsHumanTurn())
        {
            PrepararNuevoTejo();
            puedeLanzar = true;
        }
        else
        {
            // Si no es turno humano, aseguramos que no pueda lanzar y no dejamos tejos preparados
            puedeLanzar = false;
        }
    }

    void Update()
    {
        // Solo permitimos input si es el turno humano
        if (TurnManager.instance == null || !TurnManager.instance.IsHumanTurn()) return;

        // Si no podemos lanzar, no hacemos nada.
        if (!puedeLanzar) return;

        // Detectamos el clic izquierdo del mouse
        if (Input.GetMouseButtonDown(0))
        {
            LanzarTejo();
        }
    }

    private void LanzarTejo()
    {
        // --- Paso 1: Calcular la Dirección con Raycasting ---
        Ray rayo = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(rayo, out hitInfo))
        {
            Vector3 puntoDestino = hitInfo.point;

            // --- Paso 2: Obtener la Fuerza ---
            float fuerza = barraDeFuerza.GetValorFuerza() * multiplicadorDeFuerza;

            // --- Paso 3: Calcular el Vector de Lanzamiento ---
            Vector3 direccion = puntoDestino - puntoDeLanzamiento.position;
            Vector3 direccionDeLanzamiento = new Vector3(direccion.x, 0, direccion.z).normalized;
            direccionDeLanzamiento.y = alturaDelArco;

            // --- Paso 4: Lanzar el Tejo ---
            if (tejoActual != null)
            {
                tejoActual.Iniciar(puntoDeLanzamiento.position, direccionDeLanzamiento, fuerza);

                Tejo tejoComp = tejoActual.GetComponent<Tejo>();
                if (tejoComp != null)
                    tejoComp.ActivarDeteccion();

                if (GameManagerTejo.instance != null)
                    GameManagerTejo.instance.RegistrarTejoLanzado();

                tejoActual = null;
                puedeLanzar = false;
            }
        }
        else
        {
            Debug.Log("Apuntando a un lugar inválido.");
        }
    }

    // Método para crear un nuevo tejo y prepararlo para el lanzamiento
    public void PrepararNuevoTejo()
    {
        // Evitar instanciar si ya existe uno preparado
        if (tejoActual != null) return;

        // Sólo crear si es turno humano (protección adicional)
        if (TurnManager.instance != null && !TurnManager.instance.IsHumanTurn())
            return;

        if (tejoPrefab != null)
        {
            tejoActual = Instantiate(tejoPrefab, puntoDeLanzamiento.position, puntoDeLanzamiento.rotation);
            puedeLanzar = true; // Permitimos el lanzamiento
        }
    }
}
