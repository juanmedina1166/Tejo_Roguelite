using UnityEngine;
using TMPro;

public class MostrarMonedas : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textoMonedas;

    void Start()
    {
        ActualizarTexto();
    }

    public void ActualizarTexto()
    {
        if (MetaProgressionManager.instance != null)
        {
            int dineroActual = MetaProgressionManager.instance.dineroTotal;
            textoMonedas.text = "Puntos: " + dineroActual.ToString();
        }
        else
        {
            textoMonedas.text = "Puntos: 0";
            Debug.LogWarning("MetaProgressionManager no encontrado en la escena.");
        }
    }
}
