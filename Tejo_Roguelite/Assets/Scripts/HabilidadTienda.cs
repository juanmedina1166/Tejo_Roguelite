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
    public TextMeshProUGUI textoCosto; // SOLO mostrará el precio
    public TiendaUI tiendaUI; // referencia para actualizar el texto de monedas

    private int nivelActual = 0; // 0 = bloqueada, 1 = desbloqueada, 2-3 = mejoras
    private bool cargado = false;

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
            textoCosto.text = ""; // sin texto cuando llega al máximo
            botonComprar.interactable = false;
            return;
        }

        int costoActual = ObtenerCostoActual();
        int dineroActual = MetaProgressionManager.instance.dineroTotal;

        if (dineroActual >= costoActual)
        {
            // Gasta el dinero
            MetaProgressionManager.instance.GastarDinero(costoActual);

            // Aumenta el nivel de la habilidad
            nivelActual++;
            GuardarProgreso();

            // Actualiza UI
            ActualizarTexto();
            tiendaUI?.ActualizarDinero(); // actualiza el texto de monedas arriba

            Debug.Log($"Compraste {nombreHabilidad}, nivel actual: {nivelActual}");
        }
        else
        {
            Debug.Log($"No tienes suficiente dinero para comprar {nombreHabilidad} (cuesta {costoActual}).");
        }
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
            textoCosto.text = ""; // vacío cuando llega al máximo
            botonComprar.interactable = false;
        }
        else
        {
            int costo = ObtenerCostoActual();
            textoCosto.text = costo.ToString(); // solo el número del precio
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