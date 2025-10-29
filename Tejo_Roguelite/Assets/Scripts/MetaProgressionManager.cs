using UnityEngine;

public class MetaProgressionManager : MonoBehaviour
{
    public static MetaProgressionManager instance;

    [Header("Progreso meta")]
    public int dineroTotal = 0;
    [SerializeField] private int factorVictoria = 10;
    [SerializeField] private int factorDerrota = 1;

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

        string resultado = gano ? "victoria" : "derrota";
        Debug.Log($"[{resultado.ToUpper()}] Ganaste {dineroGanado} monedas (puntaje: {puntaje}). Total actual: {dineroTotal}");
    }

    public void GastarDinero(int cantidad)
    {
        dineroTotal = Mathf.Max(0, dineroTotal - cantidad);
        GuardarProgreso();
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

    public void ReiniciarProgreso()
    {
        dineroTotal = 0;
        PlayerPrefs.DeleteKey("DineroTotal");
    }
}
