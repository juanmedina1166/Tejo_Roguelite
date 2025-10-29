using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLevelManager : MonoBehaviour
{
    public static GameLevelManager instance;

    [Range(0, 5)]
    public int nivelActual = 0; // 0 = Tutorial, 1–5 = Partidas normales

    // --- ¡NUEVAS VARIABLES AÑADIDAS! ---
    [Header("Lógica del Run")]
    [Tooltip("El número de victorias (niveles) necesarias para ganar el Run")]
    public int nivelesParaGanarRun = 3;

    [Tooltip("Arrastra aquí tu panel/pantalla de '¡Ganaste el Run!'")]
    public GameObject pantallaVictoriaDelRun;

    [Tooltip("Arrastra aquí tu panel/pantalla de '¡Perdiste!'")]
    public GameObject pantallaDerrotaDelRun;
    // --- FIN DE VARIABLES NUEVAS ---


    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    // --- TU CÓDIGO EXISTENTE (¡PERFECTO!) ---
    public PersonajeIAData ObtenerDatosIA()
    {
        DificultadIA dificultad = DeterminarDificultadSegunNivel(nivelActual);

        switch (nivelActual)
        {
            case 0: return new PersonajeIAData("Estefa", DificultadIA.Facil);
            case 1: return new PersonajeIAData("Brayan MID", dificultad);
            case 2: return new PersonajeIAData("Valentina Vinotinto", dificultad);
            case 3: return new PersonajeIAData("Don Alcides", dificultad);
            case 4: return new PersonajeIAData("Jerlumine Orozco", dificultad);
            case 5: return new PersonajeIAData("Doña Gertrudis", dificultad);
            default: return new PersonajeIAData("Marcos Garzon", dificultad);
        }
    }

    private DificultadIA DeterminarDificultadSegunNivel(int nivel)
    {
        switch (nivel)
        {
            case 0: return DificultadIA.Facil; // Tutorial
            case 1: return DificultadIA.Facil;
            case 2: return (Random.value < 0.5f) ? DificultadIA.Facil : DificultadIA.Media;
            case 3: return DificultadIA.Media;
            case 4: return (Random.value < 0.5f) ? DificultadIA.Media : DificultadIA.Dificil;
            case 5: return DificultadIA.Dificil;
            default: return DificultadIA.Facil;
        }
    }
    // --- FIN DE TU CÓDIGO EXISTENTE ---


    // --- ¡NUEVOS MÉTODOS AÑADIDOS! ---

    /// <summary>
    /// Lo llama GameManagerTejo cuando el jugador gana una partida.
    /// </summary>
    /// <returns>True si el run se completó, False si aún faltan niveles.</returns>
    public bool RegistrarVictoria()
    {
        Debug.Log($"[GameLevelManager] ¡Nivel {nivelActual} superado!");
        nivelActual++; // Avanzamos al siguiente nivel

        // 3. Condición de Victoria del RUN
        if (nivelActual >= nivelesParaGanarRun)
        {
            Debug.Log("¡FELICIDADES! ¡Has ganado el RUN completo!");
            if (pantallaVictoriaDelRun != null)
            {
                pantallaVictoriaDelRun.SetActive(true); // Muestra la pantalla final
            }
            // (Opcional: resetear el nivel si quieres que vuelva al menú)
            // nivelActual = 0; 
            return true; // El run terminó
        }

        // Si no, guardamos el progreso para el siguiente nivel
        // (Tu SaveManager debería guardar el 'nivelActual' actualizado)
        return false; // El run continúa
    }

    /// <summary>
    /// Lo llama GameManagerTejo cuando el jugador pierde una partida.
    /// </summary>
    public void RegistrarDerrota()
    {
        Debug.Log($"[GameLevelManager] Has sido derrotado en el nivel {nivelActual}.");

        // Aquí decides qué pasa.
        // Opción A: Mostrar una pantalla de "Fin del Run" y reiniciar
        if (pantallaDerrotaDelRun != null)
        {
            pantallaDerrotaDelRun.SetActive(true);
        }
        // (Y al hacer clic en "Reintentar" en esa pantalla, pones nivelActual = 0)
        nivelActual = 0; // Reinicia el progreso del run

        // Opción B: Dejar que el jugador reintente el mismo nivel (no hagas nada)
        // (En este caso, no reinicies 'nivelActual')
    }
    // --- FIN DE MÉTODOS NUEVOS ---

    // Lo llamará el botón "Reintentar" de la PantallaDerrotaRun
    public void ReintentarRun()
    {
        // Reinicia el nivel y recarga la escena actual
        nivelActual = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Lo llamará el botón "Volver al Menú"
    public void VolverAlMenu()
    {
        // Asegúrate de que tu escena de menú se llame "Menu"
        SceneManager.LoadScene("Menu Principal");
    }
}