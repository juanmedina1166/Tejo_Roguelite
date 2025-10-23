using UnityEngine;
using System.Collections.Generic;

// 1. EL STRUCT PARA GUARDAR EL ESTADO DE UNA HABILIDAD
[System.Serializable]
public struct HabilidadData
{
    public string nombre;
    public float valorNumerico1; // Para el contador de "Fiebre del Oro"
    // Añade más campos si necesitas guardar otros valores (ej: valorNumerico2)

    public HabilidadData(string nombre, float valor1)
    {
        this.nombre = nombre;
        this.valorNumerico1 = valor1;
    }
}

// 2. EL STRUCT PARA GUARDAR LOS TEJOS EN LA CANCHA
[System.Serializable]
public struct TejoData
{
    public int jugadorID; // 0 = Humano, 1 = IA
    public Vector3 position;
    public Quaternion rotation;

    public TejoData(int id, Vector3 pos, Quaternion rot)
    {
        jugadorID = id;
        position = pos;
        rotation = rot;
    }
}

// 3. LA CLASE DE DATOS PRINCIPAL
[System.Serializable]
public class GameData
{
    // --- De GameLevelManager ---
    public int nivelActual;

    // --- De GameManagerTejo ---
    public int[] puntajes;
    public int rondasJugadas;
    public bool mechaExplotadaEnRonda;

    // --- De TurnManager ---
    public int jugadorActual; // 1 = Humano, 2 = IA

    // --- De HabilidadManager ---
    // ¡Usamos nuestro nuevo struct!
    public List<HabilidadData> barajaHabilidades;

    // --- Estado del Tablero ---
    public List<TejoData> tejosEnCancha;

    /// <summary>
    /// Constructor para una NUEVA PARTIDA (datos por defecto)
    /// </summary>
    public GameData()
    {
        nivelActual = 0; // O 1 si el tutorial es aparte
        puntajes = new int[2] { 0, 0 }; // Asumo 2 jugadores (J1 y IA)
        rondasJugadas = 0;
        mechaExplotadaEnRonda = false;
        jugadorActual = 1; // Siempre empieza el humano
        barajaHabilidades = new List<HabilidadData>();
        tejosEnCancha = new List<TejoData>();
    }
}