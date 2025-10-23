using UnityEngine;

public class MechaFalsa : MonoBehaviour
{
    [Tooltip("Arrastra aquí el prefab del efecto de partículas de confeti")]
    public GameObject particulasConfeti;

    // Usamos OnCollisionEnter para detectar el golpe del tejo
    private void OnCollisionEnter(Collision collision)
    {
        // Comprobamos si lo que golpeó fue un tejo
        Tejo tejo = collision.gameObject.GetComponent<Tejo>();
        if (tejo != null)
        {
            Debug.Log("¡Falsa Alarma! El rival golpeó el señuelo.");

            // Instanciamos el confeti en la posición de la mecha
            if (particulasConfeti != null)
            {
                Instantiate(particulasConfeti, transform.position, Quaternion.identity);
            }

            // La mecha falsa no da puntos. Solo desaparece.
            Destroy(gameObject);
        }
    }
}