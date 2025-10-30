using UnityEngine;
using System;
using System.Collections.Generic;

public class MetaProgressionManager : MonoBehaviour
{
    public static MetaProgressionManager instance;

    [Header("Progreso meta")]
    public int dineroTotal = 0;
    [SerializeField] private int factorVictoria = 10;
    [SerializeField] private int factorDerrota = 1;

    // Evento para notificar a la UI
    public event Action OnDineroCambiado;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            CargarProgreso();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ConvertirPuntajeEnDinero(int puntaje, bool gano)
    {
        int factor = gano ? factorVictoria : factorDerrota;
        int dineroGanado = puntaje * factor;
        dineroTotal += dineroGanado;
        GuardarProgreso();

        OnDineroCambiado?.Invoke();

        string resultado = gano ? "victoria" : "derrota";
        Debug.Log($"[{resultado.ToUpper()}] Ganaste {dineroGanado} monedas (puntaje: {puntaje}). Total actual: {dineroTotal}");
    }

    public void GastarDinero(int cantidad)
    {
        dineroTotal = Mathf.Max(0, dineroTotal - cantidad);
        GuardarProgreso();
        OnDineroCambiado?.Invoke();
    }

    public void GuardarProgreso()
    {
        PlayerPrefs.SetInt("DineroTotal", dineroTotal);
        PlayerPrefs.Save();
    }

    public void CargarProgreso()
    {
        dineroTotal = PlayerPrefs.GetInt("DineroTotal", 0);
    }

    //  Nueva versión extendida
    public void ReiniciarProgreso()
    {
        // Reiniciar dinero
        dineroTotal = 0;
        PlayerPrefs.DeleteKey("DineroTotal");

        // Eliminar progreso de habilidades
        foreach (var key in PlayerPrefsKeysConPrefijo("Habilidad_"))
        {
            PlayerPrefs.DeleteKey(key);
        }

        PlayerPrefs.Save();

        // Notificar cambio
        OnDineroCambiado?.Invoke();

        Debug.Log(" Todo el progreso (dinero y habilidades) ha sido reiniciado.");
    }

    //  Helper: busca todas las claves que empiecen con un prefijo
    private List<string> PlayerPrefsKeysConPrefijo(string prefijo)
    {
        List<string> keys = new List<string>();

        // Lista de habilidades que usas en la tienda
        string[] habilidades = { "Salto", "Velocidad", "Escudo", "Fuerza" }; // cámbialas según tus nombres reales

        foreach (string habilidad in habilidades)
        {
            string key = "Habilidad_" + habilidad + "_Nivel";
            if (PlayerPrefs.HasKey(key))
                keys.Add(key);
        }

        return keys;
    }
}
