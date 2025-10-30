using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HabilidadTienda : MonoBehaviour
{
    [Header("Configuración de la habilidad")]
    public string nombreHabilidad;
    public int costoDesbloqueo = 100;
    public int costoMejora1 = 200;
    public int costoMejora2 = 400;

    [Header("Referencias UI")]
    public Button botonComprar;
    public TextMeshProUGUI textoCosto;
    public TiendaUI tiendaUI;

    private int nivelActual = 0;

    private void Start()
    {
        botonComprar.onClick.AddListener(Comprar);
        CargarProgreso();
        ActualizarTexto();
    }

    private void Comprar()
    {
        if (MetaProgressionManager.instance == null)
        {
            Debug.LogWarning("MetaProgressionManager no encontrado.");
            return;
        }

        if (nivelActual >= 3)
        {
            textoCosto.text = "";
            botonComprar.interactable = false;
            return;
        }

        int costoActual = ObtenerCostoActual();
        int dineroActual = MetaProgressionManager.instance.dineroTotal;

        if (dineroActual >= costoActual)
        {
            MetaProgressionManager.instance.GastarDinero(costoActual);
            nivelActual++;
            GuardarProgreso();
            AplicarMejoraEnHabilidadManager();
            ActualizarTexto();
            tiendaUI?.ActualizarDinero();

            Debug.Log($"Compraste {nombreHabilidad}, nivel actual: {nivelActual}");
        }
        else
        {
            Debug.Log($"No tienes suficiente dinero para comprar {nombreHabilidad} (cuesta {costoActual}).");
        }
    }

    private void AplicarMejoraEnHabilidadManager()
    {
        if (HabilidadManager.instance == null)
        {
            Debug.LogWarning("HabilidadManager no encontrado en la escena.");
            return;
        }

        //  Se usa el método público ahora
        var habilidad = HabilidadManager.instance.BuscarHabilidadPorNombre(nombreHabilidad);

        if (habilidad == null)
        {
            Debug.LogWarning($"No se encontró la habilidad '{nombreHabilidad}' en HabilidadManager.");
            return;
        }

        int nivel = nivelActual;
        float factor = 1f + (nivel * 0.25f);
        habilidad.valorNumerico1 *= factor;
        habilidad.valorNumerico2 *= factor;

        Debug.Log($" Habilidad '{nombreHabilidad}' mejorada al nivel {nivel} (x{factor} aplicado).");
    }

    private int ObtenerCostoActual()
    {
        return nivelActual switch
        {
            0 => costoDesbloqueo,
            1 => costoMejora1,
            2 => costoMejora2,
            _ => 0
        };
    }

    private void ActualizarTexto()
    {
        if (nivelActual >= 3)
        {
            textoCosto.text = "";
            botonComprar.interactable = false;
        }
        else
        {
            int costo = ObtenerCostoActual();
            textoCosto.text = costo.ToString();
            botonComprar.interactable = true;
        }
    }

    private void GuardarProgreso()
    {
        PlayerPrefs.SetInt("Habilidad_" + nombreHabilidad + "_Nivel", nivelActual);
        PlayerPrefs.Save();
    }

    private void CargarProgreso()
    {
        nivelActual = PlayerPrefs.GetInt("Habilidad_" + nombreHabilidad + "_Nivel", 0);
    }
}
