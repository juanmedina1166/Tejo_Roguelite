using UnityEngine;

public class CentroController : MonoBehaviour
{
    [Header("�rea de Movimiento")]
    [Tooltip("El centro del �rea donde se puede mover el objetivo.")]
    public Vector3 centroDelArea;
    [Tooltip("El tama�o (ancho, alto, largo) del �rea de movimiento.")]
    public Vector3 tamanoDelArea;

    /// <summary>
    /// Mueve el centro a una nueva posici�n aleatoria dentro del �rea definida.
    /// </summary>
    public void MoverCentro()
    {
        float randomX = Random.Range(-tamanoDelArea.x / 2, tamanoDelArea.x / 2);
        float randomZ = Random.Range(-tamanoDelArea.z / 2, tamanoDelArea.z / 2);

        // Mantenemos la altura (Y) constante
        Vector3 nuevaPosicion = new Vector3(randomX, centroDelArea.y, randomZ) + centroDelArea;

        transform.position = nuevaPosicion;
        Debug.Log("El centro se ha movido a: " + nuevaPosicion);
    }

    // Dibuja el �rea de movimiento en el editor para que sea f�cil de visualizar
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 1, 0.5f); // Color cian semi-transparente
        Gizmos.DrawCube(centroDelArea, tamanoDelArea);
    }
}