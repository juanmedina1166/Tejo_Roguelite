using UnityEngine;
using UnityEngine.UI;

public class TiendaUI : MonoBehaviour
{
    public Text textoDinero;

    void OnEnable()
    {
        ActualizarDinero();
    }

    public void ActualizarDinero()
    {
        if (MetaProgressionManager.instance != null)
            textoDinero.text = "Monedas: " + MetaProgressionManager.instance.dineroTotal;
    }
}
