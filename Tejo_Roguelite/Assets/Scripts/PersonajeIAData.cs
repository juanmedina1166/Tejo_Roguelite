using UnityEngine;



public class PersonajeIAData
{
    public string nombre;
    public DificultadIA dificultad;
    public float chanceFallar;
    public float decisionDelay;
    public Vector2 rangoFuerza;
    public Vector2 missFactor;

    public PersonajeIAData(string nombre, DificultadIA dificultad)
    {
        this.nombre = nombre;
        this.dificultad = dificultad;

        // Asigna configuración según nombre + dificultad
        ConfigurarIA();
    }

    private void ConfigurarIA()
    {
        switch (nombre)
        {
            case "Estefa":
                ConfigurarEstefa();
                break;
            case "Brayan MID":
                ConfigurarBrayan();
                break;
            case "Valentina Vinotinto":
                ConfigurarValentina();
                break;
            case "Don Alcides":
                ConfigurarAlcides();
                break;
            case "Jerlumine Orozco":
                ConfigurarJerlumine();
                break;
            case "Doña Gertrudis":
                ConfigurarGertrudis();
                break;
            case "Marcos Garzon":
                ConfigurarMarcos();
                break;
            default:
                Debug.LogWarning($"IA {nombre} no configurada, usando valores por defecto.");
                chanceFallar = 0.3f;
                decisionDelay = 1.5f;
                rangoFuerza = new Vector2(0.9f, 1.1f);
                missFactor = new Vector2(1.2f, 1.8f);
                break;
        }
    }

    private void ConfigurarEstefa()
    {
        switch (dificultad)
        {
            case DificultadIA.Facil:
                chanceFallar = 0.3f;
                decisionDelay = 1.2f;
                rangoFuerza = new Vector2(0.85f, 1.05f);
                missFactor = new Vector2(1.5f, 2.2f);
                break;
            case DificultadIA.Media:
                chanceFallar = 0.18f;
                decisionDelay = 1.0f;
                rangoFuerza = new Vector2(0.9f, 1.1f);
                missFactor = new Vector2(1.2f, 1.6f);
                break;
            case DificultadIA.Dificil:
                chanceFallar = 0.08f;
                decisionDelay = 0.8f;
                rangoFuerza = new Vector2(0.95f, 1.05f);
                missFactor = new Vector2(1.0f, 1.3f);
                break;
        }
    }

    private void ConfigurarBrayan()
    {
        switch (dificultad)
        {
            case DificultadIA.Facil:
                chanceFallar = 0.5f;
                decisionDelay = 2f;
                rangoFuerza = new Vector2(0.8f, 1.2f);
                missFactor = new Vector2(1.3f, 2.5f);
                break;
            case DificultadIA.Media:
                chanceFallar = 0.3f;
                decisionDelay = 1.3f;
                rangoFuerza = new Vector2(0.9f, 1.1f);
                missFactor = new Vector2(1.1f, 1.8f);
                break;
            case DificultadIA.Dificil:
                chanceFallar = 0.1f;
                decisionDelay = 0.8f;
                rangoFuerza = new Vector2(0.95f, 1.05f);
                missFactor = new Vector2(1.0f, 1.4f);
                break;
        }
    }

    private void ConfigurarValentina()
    {
        switch (dificultad)
        {
            case DificultadIA.Facil:
                chanceFallar = 0.25f;
                decisionDelay = 1.5f;
                rangoFuerza = new Vector2(0.9f, 1.1f);
                missFactor = new Vector2(1.3f, 1.8f);
                break;
            case DificultadIA.Media:
                chanceFallar = 0.15f;
                decisionDelay = 1.2f;
                rangoFuerza = new Vector2(0.95f, 1.05f);
                missFactor = new Vector2(1.1f, 1.4f);
                break;
            case DificultadIA.Dificil:
                chanceFallar = 0.05f;
                decisionDelay = 1f;
                rangoFuerza = new Vector2(0.98f, 1.02f);
                missFactor = new Vector2(1.0f, 1.2f);
                break;
        }
    }

    private void ConfigurarAlcides()
    {
        switch (dificultad)
        {
            case DificultadIA.Facil:
                chanceFallar = 0.25f;
                decisionDelay = 1.8f;
                rangoFuerza = new Vector2(0.92f, 1.08f);
                missFactor = new Vector2(1.1f, 1.6f);
                break;
            case DificultadIA.Media:
                chanceFallar = 0.15f;
                decisionDelay = 1.3f;
                rangoFuerza = new Vector2(0.95f, 1.05f);
                missFactor = new Vector2(1.0f, 1.3f);
                break;
            case DificultadIA.Dificil:
                chanceFallar = 0.05f;
                decisionDelay = 1.0f;
                rangoFuerza = new Vector2(0.98f, 1.02f);
                missFactor = new Vector2(0.9f, 1.1f);
                break;
        }
    }

    private void ConfigurarJerlumine()
    {
        switch (dificultad)
        {
            case DificultadIA.Facil:
                chanceFallar = 0.35f;
                decisionDelay = 2.0f;
                rangoFuerza = new Vector2(0.85f, 1.15f);
                missFactor = new Vector2(1.4f, 2.0f);
                break;
            case DificultadIA.Media:
                chanceFallar = 0.25f;
                decisionDelay = 1.4f;
                rangoFuerza = new Vector2(0.9f, 1.1f);
                missFactor = new Vector2(1.2f, 1.7f);
                break;
            case DificultadIA.Dificil:
                chanceFallar = 0.1f;
                decisionDelay = 1.0f;
                rangoFuerza = new Vector2(0.95f, 1.05f);
                missFactor = new Vector2(1.0f, 1.4f);
                break;
        }
    }

    private void ConfigurarGertrudis()
    {
        switch (dificultad)
        {
            case DificultadIA.Facil:
                chanceFallar = 0.45f;
                decisionDelay = 2.2f;
                rangoFuerza = new Vector2(0.8f, 1.2f);
                missFactor = new Vector2(1.5f, 2.4f);
                break;
            case DificultadIA.Media:
                chanceFallar = 0.3f;
                decisionDelay = 1.6f;
                rangoFuerza = new Vector2(0.9f, 1.1f);
                missFactor = new Vector2(1.2f, 1.8f);
                break;
            case DificultadIA.Dificil:
                chanceFallar = 0.18f;
                decisionDelay = 1.2f;
                rangoFuerza = new Vector2(0.95f, 1.05f);
                missFactor = new Vector2(1.0f, 1.4f);
                break;
        }
    }

    private void ConfigurarMarcos()
    {
        switch (dificultad)
        {
            case DificultadIA.Facil:
                chanceFallar = 0.2f;
                decisionDelay = 2.0f;
                rangoFuerza = new Vector2(0.9f, 1.1f);
                missFactor = new Vector2(1.1f, 1.5f);
                break;
            case DificultadIA.Media:
                chanceFallar = 0.1f;
                decisionDelay = 1.4f;
                rangoFuerza = new Vector2(0.96f, 1.04f);
                missFactor = new Vector2(1.0f, 1.3f);
                break;
            case DificultadIA.Dificil:
                chanceFallar = 0.03f;
                decisionDelay = 0.9f;
                rangoFuerza = new Vector2(0.98f, 1.02f);
                missFactor = new Vector2(0.9f, 1.1f);
                break;
        }
    }
}
