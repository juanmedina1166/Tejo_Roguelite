using UnityEngine;

public class GameLevelManager : MonoBehaviour
{
    public static GameLevelManager instance;

    [Range(0, 5)]
    public int nivelActual = 0; // 0 = Tutorial, 1�5 = Partidas normales
    public DificultadIA dificultadActual; // Guardar� la dificultad elegida en cada nivel

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // Determinar la dificultad al inicio del juego
        dificultadActual = ElegirDificultad();
    }

    public PersonajeIAData ObtenerDatosIA()
    {
        switch (nivelActual)
        {
            case 0: return new PersonajeIAData("Estefa", DificultadIA.Facil);
            case 1: return new PersonajeIAData("Brayan MID", dificultadActual);
            case 2: return new PersonajeIAData("Valentina Vinotinto", dificultadActual);
            case 3: return new PersonajeIAData("Don Alcides", dificultadActual);
            case 4: return new PersonajeIAData("Jerlumine Orozco", dificultadActual);
            case 5: return new PersonajeIAData("Do�a Gertrudis", dificultadActual);
            default: return new PersonajeIAData("Marcos Garzon", dificultadActual);
        }
    }

    private DificultadIA ElegirDificultad()
    {
        int rand = Random.Range(0, 3);
        return (DificultadIA)rand; // 0 = F�cil, 1 = Normal, 2 = Dif�cil
    }
}
