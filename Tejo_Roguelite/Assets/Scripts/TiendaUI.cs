using UnityEngine;
using UnityEngine.UI;

public class TiendaUI : MonoBehaviour
{
    public Text textoDinero;

    private void OnEnable()
    {
        // Cuando se activa el panel de la tienda, actualizamos el texto
        ActualizarDinero();

        // Nos suscribimos al evento de cambio de dinero
        if (MetaProgressionManager.instance != null)
        {
            MetaProgressionManager.instance.OnDineroCambiado += ActualizarDinero;
        }
    }

    private void OnDisable()
    {
        // Nos desuscribimos para evitar errores cuando se cierre el panel
        if (MetaProgressionManager.instance != null)
        {
            MetaProgressionManager.instance.OnDineroCambiado -= ActualizarDinero;
        }
    }

    public void ActualizarDinero()
    {
        if (MetaProgressionManager.instance != null)
            textoDinero.text = "Monedas: " + MetaProgressionManager.instance.dineroTotal;
        else
            textoDinero.text = "Monedas: 0";
    }
}
