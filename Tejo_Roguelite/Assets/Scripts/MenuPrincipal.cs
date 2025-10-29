using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    [Header("Botones del menú principal")]
    public Button botonContinuar;
    public Button botonNuevaPartida;
    public Button botonTienda;
    public Button botonVolver;
    public Button botonSalir;

    [Header("Paneles")]
    public GameObject panelMenuPrincipal;
    public GameObject panelTienda;

    [Header("Configuración de escena")]
    public string escenaJuego = "GameScene"; // Cambia según tu proyecto

    void Start()
    {
        ActualizarBotonContinuar();

        botonContinuar.onClick.AddListener(ContinuarPartida);
        botonNuevaPartida.onClick.AddListener(NuevaPartida);
        botonTienda.onClick.AddListener(Tienda);
        botonVolver.onClick.AddListener(CerrarTienda);
        botonSalir.onClick.AddListener(SalirJuego);

        // Al iniciar, nos aseguramos de que solo el menú principal esté activo
        panelMenuPrincipal.SetActive(true);
        panelTienda.SetActive(false);
    }

    void ActualizarBotonContinuar()
    {
        botonContinuar.interactable = SaveManager.DoesSaveExist();
    }

    public void ContinuarPartida()
    {
        SceneManager.LoadScene(escenaJuego);
    }

    public void NuevaPartida()
    {
        SaveManager.DeleteSave();
        SceneManager.LoadScene(escenaJuego);
    }

    public void Tienda()
    {
        // Oculta el menú principal y muestra el panel de tienda
        panelMenuPrincipal.SetActive(false);
        panelTienda.SetActive(true);

        Debug.Log("Abriendo la tienda de habilidades...");
    }

    public void CerrarTienda()
    {
        // Oculta la tienda y vuelve al menú principal
        panelTienda.SetActive(false);
        panelMenuPrincipal.SetActive(true);

        Debug.Log("Cerrando la tienda y volviendo al menú principal...");
    }

    public void SalirJuego()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}