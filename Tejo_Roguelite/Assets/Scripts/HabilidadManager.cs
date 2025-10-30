using System.Collections.Generic;
using UnityEngine;
using System; // Necesario para 'Action'

public class HabilidadManager : MonoBehaviour
{
    public static HabilidadManager instance;

    [Header("Configuración de la Baraja")]
    [SerializeField] private int tamanoMaximoBaraja = 8;

    [Tooltip("Arrastra AQUÍ TODOS tus ScriptableObjects de Habilidad (de la carpeta Assets)")]
    [SerializeField] private List<Habilidad> todasLasHabilidadesMaestra;

    public bool aguardienteActivo = false;
    public bool imanBocinActivo = false;
    public bool tejoLigeroActivo = false;

    private List<Habilidad> barajaDeHabilidades = new List<Habilidad>();
    private Dictionary<string, Habilidad> runtimeHabilidades = new Dictionary<string, Habilidad>();

    public static event Action OnBarajaCambio;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            AplicarProgresoGuardado();
        }
        else
        {
            Debug.LogWarning("¡HabilidadManager DUPLICADO detectado! Destruyendo esta copia.");
            Destroy(gameObject);
        }
    }

    // ============================================================
    // MÉTODOS NUEVOS Y PÚBLICOS
    // ============================================================

    public List<Habilidad> ObtenerTodasLasHabilidades()
    {
        return todasLasHabilidadesMaestra;
    }

    public Habilidad BuscarHabilidadPorNombre(string nombre)
    {
        if (todasLasHabilidadesMaestra == null) return null;
        return todasLasHabilidadesMaestra.Find(h => h != null && h.nombre == nombre);
    }

    public Habilidad GetOrCreateRuntimeHabilidad(string nombreHabilidad)
    {
        if (runtimeHabilidades.TryGetValue(nombreHabilidad, out var runH)) return runH;

        var maestro = BuscarHabilidadPorNombre(nombreHabilidad);
        if (maestro == null) return null;

        var instancia = Instantiate(maestro);
        instancia.name = maestro.name;
        runtimeHabilidades[nombreHabilidad] = instancia;
        return instancia;
    }

    public void AplicarProgresoGuardado()
    {
        if (todasLasHabilidadesMaestra == null || todasLasHabilidadesMaestra.Count == 0)
        {
            Debug.LogWarning("No hay habilidades maestras asignadas para aplicar progreso.");
            return;
        }

        foreach (var maestro in todasLasHabilidadesMaestra)
        {
            if (maestro == null) continue;

            int nivel = PlayerPrefs.GetInt("Habilidad_" + maestro.nombre + "_Nivel", 0);
            float factor = 1f + (nivel * 0.25f);

            var runHabilidad = GetOrCreateRuntimeHabilidad(maestro.nombre);
            if (runHabilidad != null)
            {
                runHabilidad.valorNumerico1 = maestro.valorNumerico1 * factor;
                runHabilidad.valorNumerico2 = maestro.valorNumerico2 * factor;
                Debug.Log($"[Runtime] {maestro.nombre} nivel {nivel} aplicado (x{factor})");
            }
        }

        OnBarajaCambio?.Invoke();
    }

    // ============================================================
    // RESTO DE TU CÓDIGO ORIGINAL
    // ============================================================

    public List<Habilidad> GetBaraja() => new List<Habilidad>(barajaDeHabilidades);

    private void OnEnable()
    {
        GameEvents.OnMechaExploded += OnMechaExploded_Handler;
        GameEvents.OnMoñonaScored += OnMoñonaScored_Handler;
        GameEvents.OnManoScored += OnManoScored_Handler;
        GameEvents.OnAimStarted += OnAimStarted_Handler;
    }

    private void OnDisable()
    {
        GameEvents.OnMechaExploded -= OnMechaExploded_Handler;
        GameEvents.OnMoñonaScored -= OnMoñonaScored_Handler;
        GameEvents.OnManoScored -= OnManoScored_Handler;
        GameEvents.OnAimStarted -= OnAimStarted_Handler;
    }

    public bool AnadirHabilidad(Habilidad nuevaHabilidad)
    {
        if (barajaDeHabilidades.Count >= tamanoMaximoBaraja)
        {
            Debug.LogWarning("¡La baraja está llena! No se pudo añadir: " + nuevaHabilidad.nombre);
            return false;
        }

        if (nuevaHabilidad.nombre == "Fiebre del Oro")
        {
            nuevaHabilidad = Instantiate(nuevaHabilidad);
            nuevaHabilidad.valorNumerico1 = nuevaHabilidad.valorNumerico1;
        }

        barajaDeHabilidades.Add(nuevaHabilidad);
        Debug.Log(nuevaHabilidad.nombre + " fue añadida a la baraja.");
        OnBarajaCambio?.Invoke();
        return true;
    }

    public void QuitarHabilidad(Habilidad habilidadParaQuitar)
    {
        if (barajaDeHabilidades.Contains(habilidadParaQuitar))
        {
            barajaDeHabilidades.Remove(habilidadParaQuitar);
            Debug.Log(habilidadParaQuitar.nombre + " fue quitada de la baraja.");

            if (!Application.isEditor)
                Destroy(habilidadParaQuitar);

            OnBarajaCambio?.Invoke();
        }
    }

    private void OnMechaExploded_Handler(int puntosBase)
    {
        Debug.Log("==> PASO 2: HabilidadManager ha escuchado el evento.");
        GameManagerTejo.instance.MarcarMechaExplotada();
        int idDelGanador = TurnManager.instance.CurrentPlayerIndex();
        GameManagerTejo.instance.RegistrarMecha(idDelGanador, puntosBase);

        if (idDelGanador == 0)
        {
            foreach (var habilidad in barajaDeHabilidades)
            {
                if (habilidad.nombre == "Mecha Explosiva")
                {
                    int puntosExtra = (int)habilidad.valorNumerico1;
                    GameManagerTejo.instance.SumarPuntos(idDelGanador, puntosExtra);
                }
            }
        }
        else if (idDelGanador == 1)
        {
            Habilidad vengativa = GetHabilidad("Mecha Vengativa");
            if (vengativa != null)
            {
                Debug.Log("¡HABILIDAD: Mecha Vengativa!");
                GameManagerTejo.instance.SumarPuntos(0, (int)vengativa.valorNumerico1);
                GameManagerTejo.instance.RestarPuntos(1, (int)vengativa.valorNumerico2);
            }
        }
    }

    private void OnManoScored_Handler(int playerID)
    {
        Habilidad mano = GetHabilidad("El Mano");
        if (playerID == 0 && mano != null)
        {
            Debug.Log("¡HABILIDAD: El Mano!");
            GameManagerTejo.instance.SumarPuntos(0, (int)mano.valorNumerico1);
        }
    }

    private void OnMoñonaScored_Handler(int playerID)
    {
        Habilidad monona = GetHabilidad("La moñona va con toda");
        if (playerID == 0 && monona != null)
        {
            Debug.Log("¡HABILIDAD: La moñona va con toda!");
            GameManagerTejo.instance.SumarPuntos(0, (int)monona.valorNumerico1);
        }
    }

    private void OnAimStarted_Handler()
    {
        if (aguardienteActivo)
        {
            Debug.Log("Pantalla tiembla por Aguardiente");
        }
    }

    public bool TieneHabilidad(string nombreHabilidad)
    {
        foreach (var habilidad in barajaDeHabilidades)
        {
            if (habilidad.nombre == nombreHabilidad)
                return true;
        }
        return false;
    }

    public Habilidad GetHabilidad(string nombreHabilidad)
    {
        if (runtimeHabilidades.TryGetValue(nombreHabilidad, out var run))
            return run;

        foreach (var habilidad in barajaDeHabilidades)
        {
            if (habilidad.nombre == nombreHabilidad)
                return habilidad;
        }
        return null;
    }

    public void ActivarHabilidad(Habilidad habilidad)
    {
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
                tejoLigeroActivo = true;
                break;
            case "Falsa Alarma":
                GameManagerTejo.instance.ColocarMechasFalsas();
                break;
        }
    }

    public List<HabilidadData> GetDatosDeBaraja()
    {
        List<HabilidadData> datos = new List<HabilidadData>();
        foreach (var habilidad in barajaDeHabilidades)
        {
            datos.Add(new HabilidadData(habilidad.nombre, habilidad.valorNumerico1));
        }
        return datos;
    }

    public void CargarBarajaDesdeDatos(List<HabilidadData> datos)
    {
        foreach (var hab in barajaDeHabilidades)
        {
            if (!Application.isEditor)
                Destroy(hab);
        }
        barajaDeHabilidades.Clear();

        if (todasLasHabilidadesMaestra == null || todasLasHabilidadesMaestra.Count == 0)
        {
            Debug.LogError("¡'Todas Las Habilidades Maestra' no está asignada en HabilidadManager! No se puede cargar la baraja.");
            return;
        }

        foreach (HabilidadData data in datos)
        {
            Habilidad habAsset = todasLasHabilidadesMaestra.Find(h => h.nombre == data.nombre);
            if (habAsset != null)
            {
                Habilidad nuevaHabilidad = Instantiate(habAsset);
                nuevaHabilidad.name = habAsset.name;
                nuevaHabilidad.valorNumerico1 = data.valorNumerico1;
                barajaDeHabilidades.Add(nuevaHabilidad);
            }
        }

        OnBarajaCambio?.Invoke();
    }
}

