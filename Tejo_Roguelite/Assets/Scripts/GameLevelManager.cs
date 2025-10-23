using UnityEngine;

public class GameLevelManager : MonoBehaviour
{
    public static GameLevelManager instance;

    [Range(0, 5)]
    public int nivelActual = 0; // 0 = Tutorial, 1–5 = Partidas normales

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

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
}
