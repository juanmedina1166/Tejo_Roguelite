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

    void Start()
    {
        // Preparamos el primer tejo al iniciar el juego
        PrepararNuevoTejo();
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

        // Lanzamos el rayo y comprobamos si choca con algo en la escena
        if (Physics.Raycast(rayo, out hitInfo))
        {
            // Si el rayo choca, 'hitInfo.point' nos da la coordenada 3D exacta del impacto.
            Vector3 puntoDestino = hitInfo.point;

            // --- Paso 2: Obtener la Fuerza ---
            // Obtenemos el valor actual de la barra (entre 0 y 1) y lo multiplicamos para darle potencia
            float fuerza = barraDeFuerza.GetValorFuerza() * multiplicadorDeFuerza;

            // --- Paso 3: Calcular el Vector de Lanzamiento ---
            // Calculamos la dirección desde el punto de lanzamiento hasta el destino del mouse
            Vector3 direccion = puntoDestino - puntoDeLanzamiento.position;

            // Le damos una inclinación hacia arriba para crear una parábola
            // Normalizamos para mantener solo la dirección y luego ajustamos la altura del arco
            Vector3 direccionDeLanzamiento = new Vector3(direccion.x, 0, direccion.z).normalized;
            direccionDeLanzamiento.y = alturaDelArco; // Añadimos la componente vertical

            // --- Paso 4: Lanzar el Tejo ---
            if (tejoActual != null)
            {
                // Le pasamos todos los datos al script del tejo para que aplique la física
                tejoActual.Iniciar(puntoDeLanzamiento.position, direccionDeLanzamiento, fuerza);

                // Si el prefab tiene el componente Tejo, activamos la detección de parada
                Tejo tejoComp = tejoActual.GetComponent<Tejo>();
                if (tejoComp != null)
                    tejoComp.ActivarDeteccion();

                // Registramos el lanzamiento en el GameManager para que controle el cambio de turno
                if (GameManagerTejo.instance != null)
                    GameManagerTejo.instance.RegistrarTejoLanzado();

                // Marcamos el tejo como "lanzado" para que no podamos controlarlo más
                tejoActual = null;

                // Desactivamos el control hasta que sea nuestro turno de nuevo
                puedeLanzar = false;
            }
        }
        else
        {
            // Opcional: si el jugador hace clic apuntando al cielo, no hacemos nada.
            Debug.Log("Apuntando a un lugar inválido.");
        }
    }

    // Método para crear un nuevo tejo y prepararlo para el lanzamiento
    public void PrepararNuevoTejo()
    {
        if (tejoPrefab != null)
        {
            // Creamos una nueva instancia del tejo en la posición de lanzamiento
            tejoActual = Instantiate(tejoPrefab, puntoDeLanzamiento.position, puntoDeLanzamiento.rotation);
            puedeLanzar = true; // Permitimos el lanzamiento
        }
    }
}
