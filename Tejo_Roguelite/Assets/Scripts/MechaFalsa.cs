using UnityEngine;

public class MechaFalsa : MonoBehaviour
{
    [Tooltip("Arrastra aqu� el prefab del efecto de part�culas de confeti")]
    public GameObject particulasConfeti;

    // Usamos OnCollisionEnter para detectar el golpe del tejo
    private void OnCollisionEnter(Collision collision)
    {
        // Comprobamos si lo que golpe� fue un tejo
        Tejo tejo = collision.gameObject.GetComponent<Tejo>();
        if (tejo != null)
        {
            Debug.Log("�Falsa Alarma! El rival golpe� el se�uelo.");

            // Instanciamos el confeti en la posici�n de la mecha
            if (particulasConfeti != null)
            {
                Instantiate(particulasConfeti, transform.position, Quaternion.identity);
            }

            // La mecha falsa no da puntos. Solo desaparece.
            Destroy(gameObject);
        }
    }
}