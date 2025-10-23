using System.IO;
using UnityEngine;

public static class SaveManager
{
    // Usamos Path.Combine para que funcione en Windows, Mac, Android, etc.
    private static string savePath = Path.Combine(Application.persistentDataPath, "currentRun.json");

    /// <summary>
    /// Guarda el objeto GameData en un archivo JSON.
    /// </summary>
    public static void SaveGame(GameData data)
    {
        // 'true' al final formatea el JSON para que sea legible (bueno para debuggear)
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Partida guardada en: " + savePath);
    }

    /// <summary>
    /// Carga el GameData desde el archivo JSON.
    /// Devuelve null si no existe.
    /// </summary>
    public static GameData LoadGame()
    {
        if (DoesSaveExist())
        {
            string json = File.ReadAllText(savePath);
            GameData data = JsonUtility.FromJson<GameData>(json);
            return data;
        }

        Debug.LogWarning("No se encontró archivo de guardado.");
        return null;
    }

    /// <summary>
    /// Borra el archivo de guardado.
    /// </summary>
    public static void DeleteSave()
    {
        if (DoesSaveExist())
        {
            File.Delete(savePath);
            Debug.Log("Archivo de guardado borrado.");
        }
    }

    /// <summary>
    /// Comprueba si el archivo de guardado existe.
    /// La función clave para tu botón "Continuar".
    /// </summary>
    public static bool DoesSaveExist()
    {
        return File.Exists(savePath);
    }
}
