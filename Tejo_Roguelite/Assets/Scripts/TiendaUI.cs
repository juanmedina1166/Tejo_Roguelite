using UnityEngine;
using TMPro;

public class TiendaUI : MonoBehaviour
{
    public TextMeshProUGUI textoDinero;

    private void OnEnable()
    {
        ActualizarDinero();
    }

    public void ActualizarDinero()
    {
        if (MetaProgressionManager.instance != null)
            textoDinero.text = "Monedas: " + MetaProgressionManager.instance.dineroTotal;
        else
            textoDinero.text = "Monedas: 0";
    }
}
