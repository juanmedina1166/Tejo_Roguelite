// Archivo: RewardScreen.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardScreen : MonoBehaviour
{
    [Header("Referencias Principales")]
    [SerializeField] private GameObject rewardScreenPanel; // El panel principal que se activa/desactiva
    [SerializeField] private HabilidadDatabase habilidadDatabase; // Tu asset de la biblioteca de habilidades
    [SerializeField] private HabilidadManager habilidadManager; // El manager en tu jugador

    [Header("Configuraci�n de UI")]
    [SerializeField] private Transform opcionesContainer; // El objeto con el Horizontal Layout Group
    [SerializeField] private GameObject prefabCartaHabilidad; // Tu prefab de la carta visual

    /// <summary>
    /// Muestra la pantalla de recompensas con 3 opciones aleatorias.
    /// </summary>
    public void MostrarRecompensas()
    {
        // 1. Limpiar las opciones anteriores
        foreach (Transform child in opcionesContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. Obtener 3 habilidades aleatorias de la biblioteca
        List<Habilidad> opciones = habilidadDatabase.GetHabilidadesAleatorias(3);

        // 3. Crear las cartas en la UI para cada opci�n
        foreach (var habilidadOpcion in opciones)
        {
            GameObject cartaGO = Instantiate(prefabCartaHabilidad, opcionesContainer);

            // Rellenar los datos visuales de la carta
            cartaGO.transform.Find("FondoImage").GetComponent<Image>().sprite = habilidadOpcion.fondo;
            cartaGO.transform.Find("NombreText").GetComponent<TextMeshProUGUI>().text = habilidadOpcion.nombre;
            cartaGO.transform.Find("IconoImage").GetComponent<Image>().sprite = habilidadOpcion.icono;

            // Ocultar el bot�n de quitar si existe y no lo queremos aqu�
            Transform botonQuitarTransform = cartaGO.transform.Find("BotonQuitar");
            if (botonQuitarTransform != null)
            {
                botonQuitarTransform.gameObject.SetActive(false);
            }

            // Hacemos que TODA la carta sea un bot�n para seleccionarla
            Button cartaButton = cartaGO.GetComponent<Button>();
            if (cartaButton == null) // Si la carta no tiene un bot�n en su ra�z, se lo a�adimos
            {
                cartaButton = cartaGO.AddComponent<Button>();
            }

            // Configuramos el clic del bot�n
            cartaButton.onClick.RemoveAllListeners();
            cartaButton.onClick.AddListener(() => SeleccionarHabilidad(habilidadOpcion));
        }

        // 4. Finalmente, mostrar la pantalla
        rewardScreenPanel.SetActive(true);
    }

    /// <summary>
    /// Se llama cuando el jugador hace clic en una de las cartas de recompensa.
    /// </summary>
    private void SeleccionarHabilidad(Habilidad habilidadElegida)
    {
        Debug.Log("Habilidad seleccionada: " + habilidadElegida.nombre);

        // Intentamos a�adir la habilidad a la baraja del jugador
        habilidadManager.AnadirHabilidad(habilidadElegida);

        // Aqu� podr�as a�adir l�gica por si la baraja est� llena,
        // pero por ahora, simplemente ocultamos la pantalla.

        // Ocultamos la pantalla de recompensa
        rewardScreenPanel.SetActive(false);

        // �NUEVO! Le decimos al GameManager que reinicie para la siguiente partida.
        GameManagerTejo.instance.ReiniciarParaNuevaPartida();
    }
}
