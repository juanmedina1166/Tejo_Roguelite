using UnityEngine;
using UnityEngine.UI;

public class BarraDeFuerza : MonoBehaviour
{
    [Header("Config Barra de Fuerza")]
    public Slider barraPoderSlider; // Asigna aqu� el slider 'BarraFuerzaPoder'
    public float velocidadSubida = 1.5f;
    public float velocidadBajada = 0.5f;

    [Header("Colores por turno")]
    public Color[] coloresPorTurno; // Asigna los colores en e

    private float valorActual = 0f;
    private bool subiendo = true;

    void Update()
    {

        if (subiendo)
        {
            valorActual += velocidadSubida * Time.deltaTime;
            if (valorActual >= 1f)
            {
                valorActual = 1f;
                subiendo = false;
            }
        }
        else
        {
            valorActual -= velocidadBajada * Time.deltaTime;
            if (valorActual <= 0f)
            {
                valorActual = 0f;
                subiendo = true;
            }
        }

        barraPoderSlider.value = valorActual;


    }

    // M�todo p�blico para obtener el valor de la barra cuando presiones un bot�n
    public float GetValorFuerza()
    {
        return valorActual;
    }
}
