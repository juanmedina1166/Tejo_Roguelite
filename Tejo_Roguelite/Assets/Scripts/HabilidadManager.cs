using System.Collections.Generic;
using UnityEngine;
using System; // Necesario para 'Action'

public class HabilidadManager : MonoBehaviour
{
    public static HabilidadManager instance;
    [Header("Configuración de la Baraja")]
    [SerializeField] private int tamanoMaximoBaraja = 8; // Límite de habilidades

    [Tooltip("Arrastra AQUÍ TODOS tus ScriptableObjects de Habilidad (de la carpeta Assets)")]
    [SerializeField] private List<Habilidad> todasLasHabilidadesMaestra; // <-- Esta es la lista maestra

    // --- BANDERAS PARA HABILIDADES ACTIVAS ---
    // Estas se resetean después de usarse
    public bool aguardienteActivo = false;
    public bool imanBocinActivo = false;
    public bool tejoLigeroActivo = false; // Para el rival

    // Usamos una Lista porque es fácil añadir y quitar elementos.
    private List<Habilidad> barajaDeHabilidades = new List<Habilidad>();

    // Evento para notificar a la UI cuando la baraja cambie.
    public static event Action OnBarajaCambio;

    private void Awake() // <-- AÑADIR ESTE MÉTODO
    {
        if (instance == null)
        {
            instance = this;
            // Opcional: DontDestroyOnLoad(gameObject); si persiste entre escenas
        }
        else
        {
            // Si ya existe un GameManager, destruye este nuevo
            Debug.LogWarning("¡GameManagerTejo DUPLICADO detectado! Destruyendo esta copia.");
            Destroy(gameObject);
        }
    }
    // Método para que otros scripts obtengan una copia segura de la baraja
    public List<Habilidad> GetBaraja()
    {
        return new List<Habilidad>(barajaDeHabilidades);
    }
    private void OnEnable()
    {
        // Le decimos al sistema de eventos: "Cuando ocurra OnMechaExploded, llama a mi método OnMechaExploded_Handler"
        GameEvents.OnMechaExploded += OnMechaExploded_Handler;
        GameEvents.OnMoñonaScored += OnMoñonaScored_Handler;
        GameEvents.OnManoScored += OnManoScored_Handler;
        GameEvents.OnAimStarted += OnAimStarted_Handler;
    }

    private void OnDisable()
    {
        // Le decimos al sistema de eventos que ya no necesitamos que nos avise.
        // Esto es muy importante para evitar errores.
        GameEvents.OnMechaExploded -= OnMechaExploded_Handler;
        GameEvents.OnMoñonaScored -= OnMoñonaScored_Handler;
        GameEvents.OnManoScored -= OnManoScored_Handler;
        GameEvents.OnAimStarted -= OnAimStarted_Handler;
    }

    /// <summary>
    /// Intenta añadir una habilidad a la baraja.
    /// </summary>
    /// <returns>True si la habilidad fue añadida, false si la baraja está llena.</returns>
    public bool AnadirHabilidad(Habilidad nuevaHabilidad)
    {
        // Comprobamos si hay espacio en la baraja
        if (barajaDeHabilidades.Count >= tamanoMaximoBaraja)
        {
            Debug.LogWarning("¡La baraja está llena! No se pudo añadir: " + nuevaHabilidad.nombre);
            return false; // No se pudo añadir
        }

        if (nuevaHabilidad.nombre == "Fiebre del Oro")
        {
            nuevaHabilidad = Instantiate(nuevaHabilidad);
            // ¡OJO! Aquí usamos valorNumerico1 DEL ASSET para setear el contador inicial
            // Asumimos que pusiste '3' en el asset.
            nuevaHabilidad.valorNumerico1 = nuevaHabilidad.valorNumerico1;
        }

        barajaDeHabilidades.Add(nuevaHabilidad);
        Debug.Log(nuevaHabilidad.nombre + " fue añadida a la baraja.");

        // Avisamos a quien esté escuchando (la UI) que la baraja ha cambiado.
        OnBarajaCambio?.Invoke();

        return true; // Éxito
    }

    /// <summary>
    /// Quita una habilidad específica de la baraja.
    /// </summary>
    public void QuitarHabilidad(Habilidad habilidadParaQuitar)
    {
        if (barajaDeHabilidades.Contains(habilidadParaQuitar))
        {
            barajaDeHabilidades.Remove(habilidadParaQuitar);
            Debug.Log(habilidadParaQuitar.nombre + " fue quitada de la baraja.");

            if (!Application.isEditor) // Evita errores en el editor
            {
                Destroy(habilidadParaQuitar);
            }

            // Avisamos a la UI que la baraja ha cambiado.
            OnBarajaCambio?.Invoke();
        }
    }
   
    private void OnMechaExploded_Handler(int puntosBase)
    {
        Debug.Log("==> PASO 2: HabilidadManager ha escuchado el evento.");
        GameManagerTejo.instance.MarcarMechaExplotada(); // Esto avisa a la ronda que no hay "mano"

        // 1. Le preguntamos al TurnManager de quién es el turno (0 para humano, 1 para IA).
        int idDelGanador = TurnManager.instance.CurrentPlayerIndex();

        // 2. ? ¡CAMBIO CLAVE! En lugar de sumar puntos, REGISTRAMOS la mecha en el GameManager.
        // El GameManager decidirá si fue Mecha (3) o Moñona (9) cuando el tejo se detenga.
        GameManagerTejo.instance.RegistrarMecha(idDelGanador, puntosBase);

        // 3. Revisamos si el jugador actual tiene habilidades de bonus.
        // ESTO SÍ SE QUEDA, ya que son puntos EXTRA de la habilidad.
        if (idDelGanador == 0) // Asumiendo que solo el jugador humano tiene habilidades
        {
            foreach (var habilidad in barajaDeHabilidades)
            {
                if (habilidad.nombre == "Mecha Explosiva")
                {
                    int puntosExtra = (int)habilidad.valorNumerico1;
                    // Si la tiene, le decimos al GameManager que sume los puntos EXTRA.
                    GameManagerTejo.instance.SumarPuntos(idDelGanador, puntosExtra);
                }
            }
        }
        else if (idDelGanador == 1) // Si la IA (jugador 1) hizo mecha
        {
            Habilidad vengativa = GetHabilidad("Mecha Vengativa"); // Obtenemos el asset
            if (vengativa != null)
            {
                Debug.Log("¡HABILIDAD: Mecha Vengativa!");
                GameManagerTejo.instance.SumarPuntos(0, (int)vengativa.valorNumerico1);  // Ganas valor1
                GameManagerTejo.instance.RestarPuntos(1, (int)vengativa.valorNumerico2); // IA pierde valor2
            }
        }
    }
    private void OnManoScored_Handler(int playerID)
    {
        Habilidad mano = GetHabilidad("El Mano"); // Obtenemos el asset
        if (playerID == 0 && mano != null)
        {
            Debug.Log("¡HABILIDAD: El Mano!");
            GameManagerTejo.instance.SumarPuntos(0, (int)mano.valorNumerico1); // Sumas valor1
        }
    }
    private void OnMoñonaScored_Handler(int playerID)
    {
        Habilidad monona = GetHabilidad("La moñona va con toda"); // Obtenemos el asset
        if (playerID == 0 && monona != null)
        {
            Debug.Log("¡HABILIDAD: La moñona va con toda!");
            GameManagerTejo.instance.SumarPuntos(0, (int)monona.valorNumerico1); // Sumas valor1
        }
    }
    private void OnAimStarted_Handler()
    {
        if (aguardienteActivo)
        {
            // Aquí puedes llamar a un script de cámara para que tiemble
            // Ejemplo: CameraShaker.instance.Shake(0.5f, 0.1f);
            Debug.Log("Pantalla tiembla por Aguardiente");
        }
    }
    /// <summary>
    /// Comprueba si el jugador tiene una habilidad por su nombre.
    /// </summary>
    public bool TieneHabilidad(string nombreHabilidad)
    {
        foreach (var habilidad in barajaDeHabilidades)
        {
            if (habilidad.nombre == nombreHabilidad)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Devuelve una habilidad de la baraja (para leer/modificar sus valores, como contadores).
    /// </summary>
    public Habilidad GetHabilidad(string nombreHabilidad)
    {
        foreach (var habilidad in barajaDeHabilidades)
        {
            if (habilidad.nombre == nombreHabilidad)
            {
                return habilidad;
            }
        }
        return null; // Devuelve null si no la tiene
    }
    public void ActivarHabilidad(Habilidad habilidad)
    {
        // Lógica para activar la habilidad y consumirla
        Debug.Log("Activando: " + habilidad.nombre);

        switch (habilidad.nombre)
        {
            case "Aguardiente Doble Filo":
                aguardienteActivo = true;
                break;
            case "Iman del Bocin":
                imanBocinActivo = true;
                break;
            case "Tejo Ligero":
                tejoLigeroActivo = true; // El script del rival revisará esto
                break;
            case "Falsa Alarma":
                // Llama al GameManager para que instancie las mechas falsas
                GameManagerTejo.instance.ColocarMechasFalsas();
                break;
        }
    }
    /// <summary>
    /// Recoge los datos de la baraja actual para guardarlos.
    /// </summary>
    public List<HabilidadData> GetDatosDeBaraja()
    {
        List<HabilidadData> datos = new List<HabilidadData>();
        foreach (var habilidad in barajaDeHabilidades)
        {
            // Guardamos el nombre y su valorNumerico1 (para contadores)
            datos.Add(new HabilidadData(habilidad.nombre, habilidad.valorNumerico1));
        }
        return datos;
    }

    /// <summary>
    /// Limpia la baraja y la carga desde los datos guardados.
    /// </summary>
    public void CargarBarajaDesdeDatos(List<HabilidadData> datos)
    {
        // Limpiamos la baraja actual (instancias)
        foreach (var hab in barajaDeHabilidades)
        {
            if (!Application.isEditor) // Previene error en editor
                Destroy(hab);
        }
        barajaDeHabilidades.Clear();

        // Validamos que la lista maestra esté llena
        if (todasLasHabilidadesMaestra == null || todasLasHabilidadesMaestra.Count == 0)
        {
            Debug.LogError("¡'Todas Las Habilidades Maestra' no está asignada en HabilidadManager! No se puede cargar la baraja.");
            return;
        }

        // Volvemos a crear la baraja
        foreach (HabilidadData data in datos)
        {
            // 1. Encontrar el asset original en la lista maestra
            Habilidad habAsset = todasLasHabilidadesMaestra.Find(h => h.nombre == data.nombre);

            if (habAsset != null)
            {
                // 2. Crear una INSTANCIA (copia) para no modificar el asset
                Habilidad nuevaHabilidad = Instantiate(habAsset);
                nuevaHabilidad.name = habAsset.name; // Quita el "(Clone)" del nombre

                // 3. Restaurar el estado guardado (el contador)
                nuevaHabilidad.valorNumerico1 = data.valorNumerico1;

                barajaDeHabilidades.Add(nuevaHabilidad);
            }
        }

        // Avisamos a la UI que actualice las cartas
        OnBarajaCambio?.Invoke();
    }
}
