using UnityEngine;
using System.Collections;

public class CamaraSeguirTejo : MonoBehaviour
{
    [Header("Referencias")]
    public Transform puntoInicial;      // Donde vuelve la cámara al terminar el tiro
    private Transform objetivo;         // Tejo a seguir (se asigna dinámicamente)

    [Header("Ajustes de movimiento")]
    public float suavizado = 5f;

    //  Ahora vista diagonal: ligeramente detrás y a un lado del tejo
    public Vector3 offsetDiagonal = new Vector3(-5f, 3f, -5f);

    [Header("Detección de movimiento del tejo")]
    public float velocidadMinima = 0.2f;
    public float tiempoParaDetener = 1f;

    private Rigidbody rbTejo;
    private bool siguiendo = false;
    private float tiempoQuieto = 0f;

    void LateUpdate()
    {
        if (siguiendo && objetivo != null)
        {
            //  Seguir desde la diagonal
            Vector3 posicionDeseada = objetivo.position + offsetDiagonal;
            transform.position = Vector3.Lerp(transform.position, posicionDeseada, suavizado * Time.deltaTime);

            transform.LookAt(objetivo.position);

            // Verificar si el tejo ya se detuvo
            if (rbTejo != null)
            {
                if (rbTejo.linearVelocity.magnitude < velocidadMinima)
                {
                    tiempoQuieto += Time.deltaTime;
                    if (tiempoQuieto >= tiempoParaDetener)
                    {
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
        StartCoroutine(EsperarYSeguir(nuevoTejo)); //  Añadimos retardo antes de activar seguimiento
    }

    /// <summary>
    /// Espera 0.5s antes de activar el seguimiento.
    /// </summary>
    private IEnumerator EsperarYSeguir(Transform nuevoTejo)
    {
        yield return new WaitForSeconds(0.5f); //  retardo antes de seguir

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

        yield return new WaitForSeconds(0.5f);

        Vector3 posFinal = puntoInicial.position;
        Quaternion rotFinal = puntoInicial.rotation;

        float t = 0f;
        Vector3 inicio = transform.position;
        Quaternion inicioRot = transform.rotation;

        while (t < 1f)
        {
            t += Time.deltaTime * 1.5f;
            transform.position = Vector3.Lerp(inicio, posFinal, t);
            transform.rotation = Quaternion.Lerp(inicioRot, rotFinal, t);
            yield return null;
        }
    }
}
