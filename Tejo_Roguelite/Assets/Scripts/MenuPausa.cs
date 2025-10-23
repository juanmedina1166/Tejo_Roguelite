using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPausa : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject panelMenuPausa; // Arrastra tu Panel de Pausa aquí

    [Header("Configuración")]
    [SerializeField] private string nombreEscenaMenu = "MenuPrincipal"; // El nombre de tu escena de Menú

    // Para saber si el juego ya está pausado
    private bool juegoPausado = false;

    // Asegúrate de que el menú de pausa esté oculto al empezar
    void Start()
    {
        if (panelMenuPausa != null)
        {
            panelMenuPausa.SetActive(false);
        }
        // Asegurarse de que el tiempo corre normal al inicio
        Time.timeScale = 1f;
        juegoPausado = false;
    }

    // Esta función la puedes llamar desde tu botón de Pausa (el del engranaje ⚙️)
    public void TogglePausa()
    {
        if (juegoPausado)
        {
            ResumirJuego();
        }
        else
        {
            PausarJuego();
        }
    }

    // Esta función es para el botón "Resumir" DENTRO del menú de pausa
    public void ResumirJuego()
    {
        if (panelMenuPausa != null)
        {
            panelMenuPausa.SetActive(false);
        }

        // Reanuda el tiempo del juego
        Time.timeScale = 1f;
        juegoPausado = false;

        // (Opcional) Desbloquea el input si lo habías bloqueado
        // if (GameManagerTejo.instance != null)
        //     GameManagerTejo.instance.DesbloquearInput();
    }

    // Esta función la llama TogglePausa()
    private void PausarJuego()
    {
        if (panelMenuPausa != null)
        {
            panelMenuPausa.SetActive(true);
        }

        // ¡La forma de pausar el juego en Unity!
        Time.timeScale = 0f;
        juegoPausado = true;

        // (Opcional) Bloquea el input del tejo para que no se mueva
        // if (GameManagerTejo.instance != null)
        //     GameManagerTejo.instance.BloquearInput();
    }

    // ¡ESTA ES LA FUNCIÓN CLAVE QUE NECESITAS!
    // Conéctala al botón "Guardar y Salir" de tu menú de pausa
    public void GuardarYSalir()
    {
        // 1. ¡MUY IMPORTANTE! Reanuda el tiempo antes de salir.
        // Si sales a otra escena con Time.timeScale = 0, esa escena estará "congelada".
        Time.timeScale = 1f;
        juegoPausado = false;

        // 2. Llama a la función que creamos en el GameManagerTejo
        if (GameManagerTejo.instance != null)
        {
            GameManagerTejo.instance.GuardarPartidaActual();
        }
        else
        {
            Debug.LogError("¡No se encontró instancia de GameManagerTejo para guardar!");
        }

        // 3. Carga la escena del Menú Principal
        SceneManager.LoadScene(nombreEscenaMenu);
    }
}
