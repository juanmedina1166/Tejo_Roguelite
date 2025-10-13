// Archivo: DeckUIManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Si usas TextMeshPro

public class DeckUIManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private HabilidadManager habilidadManager; // Arrastra tu Player con el HabilidadManager aquí
    [SerializeField] private Transform contenedorCartas; // El objeto con el Grid Layout Group
    [SerializeField] private GameObject prefabCartaHabilidad; // Tu prefab de la carta

    private void OnEnable()
    {
        // Nos suscribimos al evento. Cada vez que la baraja cambie, se llamará a DibujarBaraja.
        HabilidadManager.OnBarajaCambio += DibujarBaraja;
    }

    private void OnDisable()
    {
        // Nos damos de baja para evitar errores.
        HabilidadManager.OnBarajaCambio -= DibujarBaraja;
    }

    void Start()
    {
        // Dibuja el estado inicial de la baraja cuando el juego empieza.
        DibujarBaraja();
    }

    /// <summary>
    /// Limpia y vuelve a dibujar todas las cartas de la baraja en la UI.
    /// </summary>
    private void DibujarBaraja()
    {
        // 1. Limpiar las cartas viejas
        foreach (Transform child in contenedorCartas)
        {
            Destroy(child.gameObject);
        }

        // 2. Obtener la baraja actualizada del manager
        var barajaActual = habilidadManager.GetBaraja();

        // 3. Crear las nuevas cartas
        foreach (var habilidad in barajaActual)
        {
            GameObject cartaGO = Instantiate(prefabCartaHabilidad, contenedorCartas);

            // Rellenar la información de la carta (ajusta los nombres según tu prefab)
            cartaGO.transform.Find("NombreText").GetComponent<TextMeshProUGUI>().text = habilidad.nombre;
            cartaGO.transform.Find("IconoImage").GetComponent<Image>().sprite = habilidad.icono;

            // --- La parte clave: configurar el botón de quitar ---
            Button botonQuitar = cartaGO.transform.Find("BotonQuitar").GetComponent<Button>();

            // Limpiamos listeners anteriores para evitar bugs
            botonQuitar.onClick.RemoveAllListeners();

            // Añadimos un nuevo listener que sabe exactamente qué habilidad debe quitar.
            Habilidad habilidadActual = habilidad; // Creamos una copia local para el closure
            botonQuitar.onClick.AddListener(() => QuitarHabilidadSeleccionada(habilidadActual));
        }
    }

    private void QuitarHabilidadSeleccionada(Habilidad habilidad)
    {
        Debug.Log("Intentando quitar: " + habilidad.nombre);
        // Le decimos al manager de lógica que quite la habilidad.
        // Como estamos suscritos al evento OnBarajaCambio, la UI se redibujará automáticamente.
        habilidadManager.QuitarHabilidad(habilidad);
    }
}
