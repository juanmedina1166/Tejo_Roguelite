using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    // Arrastra tu botón "Continuar" a este campo en el Inspector
    public Button botonContinuar;

    // Arrastra tu botón "Nueva Partida"
    public Button botonNuevaPartida;

    // Arrastra tu botón "Salir"
    public Button botonSalir;

    // El nombre de la escena de tu juego
    public string escenaJuego = "GameScene"; // ¡Cambia esto por el nombre de tu escena!

    void Start()
    {
        // 1. Revisa si existe un archivo de guardado
        ActualizarBotonContinuar();

        // 2. Asigna las funciones a los clics de los botones
        botonContinuar.onClick.AddListener(ContinuarPartida);
        botonNuevaPartida.onClick.AddListener(NuevaPartida);
        botonSalir.onClick.AddListener(SalirJuego);
    }

    void ActualizarBotonContinuar()
    {
        // Esta es la lógica principal que pediste:
        // Activa o desactiva el botón si el archivo existe.
        if (SaveManager.DoesSaveExist())
        {
            botonContinuar.interactable = true;
        }
        else
        {
            botonContinuar.interactable = false;
        }
    }

    public void ContinuarPartida()
    {
        // Simplemente carga la escena del juego.
        // El GameManager de esa escena se encargará de cargar los datos.
        SceneManager.LoadScene(escenaJuego);
    }

    public void NuevaPartida()
    {
        // 1. Borra cualquier partida guardada anterior
        SaveManager.DeleteSave();

        // 2. Carga la escena del juego
        SceneManager.LoadScene(escenaJuego);
    }

    public void SalirJuego()
    {
        Application.Quit();
        // (Si estás en el editor de Unity, esto no hará nada.
        // Usa Debug.Log("Saliendo del juego...") para probar)
    }
}