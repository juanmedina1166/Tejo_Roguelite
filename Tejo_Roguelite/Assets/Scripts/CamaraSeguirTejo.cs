using UnityEngine;
using System.Collections;

public class CamaraSeguirTejo : MonoBehaviour
{
    [Header("Referencias")]
    public Transform puntoInicial;      // Donde vuelve la c�mara al terminar el tiro
    private Transform objetivo;         // Tejo a seguir (se asigna din�micamente)

    [Header("Ajustes de movimiento")]
    public float suavizado = 5f;        // Cu�n suave se mueve la c�mara
    public Vector3 offset = new Vector3(0, 3, -6); // Posici�n relativa respecto al tejo

    [Header("Detecci�n de movimiento del tejo")]
    public float velocidadMinima = 0.2f;  // Cuando el tejo est� m�s lento que esto, se considera detenido
    public float tiempoParaDetener = 1f;  // Tiempo que debe pasar a esa velocidad antes de volver

    private Rigidbody rbTejo;
    private bool siguiendo = false;
    private float tiempoQuieto = 0f;

    void LateUpdate()
    {
        if (siguiendo && objetivo != null)
        {
            // Seguir la posici�n del tejo con suavizado
            Vector3 posicionDeseada = objetivo.position + offset;
            transform.position = Vector3.Lerp(transform.position, posicionDeseada, suavizado * Time.deltaTime);

            // Opcional: mira al tejo
            transform.LookAt(objetivo.position);

            // Verificar si el tejo ya se detuvo
            if (rbTejo != null)
            {
                if (rbTejo.linearVelocity.magnitude < velocidadMinima)
                {
                    tiempoQuieto += Time.deltaTime;
                    if (tiempoQuieto >= tiempoParaDetener)
                    {
                        // Dejar de seguir y volver a la posici�n original
                        StartCoroutine(VolverAPosicionInicial());
                    }
                }
                else
                {
                    tiempoQuieto = 0f;
                }
            }
        }
    }

    /// <summary>
    /// Se llama cuando un nuevo tejo es lanzado.
    /// </summary>
    public void SeguirTejo(Transform nuevoTejo)
    {
        objetivo = nuevoTejo;
        rbTejo = nuevoTejo.GetComponent<Rigidbody>();
        siguiendo = true;
        tiempoQuieto = 0f;
    }

    private IEnumerator VolverAPosicionInicial()
    {
        siguiendo = false;
        objetivo = null;
        rbTejo = null;

        // Esperar un momento antes de volver
        yield return new WaitForSeconds(0.5f);

        Vector3 posFinal = puntoInicial.position;
        Quaternion rotFinal = puntoInicial.rotation;

        float t = 0f;
        Vector3 inicio = transform.position;
        Quaternion inicioRot = transform.rotation;

        while (t < 1f)
        {
            t += Time.deltaTime * 1.5f; // velocidad de retorno
            transform.position = Vector3.Lerp(inicio, posFinal, t);
            transform.rotation = Quaternion.Lerp(inicioRot, rotFinal, t);
            yield return null;
        }
    }
}
