using UnityEngine;

// Este script debe ir en el Trigger que detecta cuando el tejo se va de la cancha.
public class ZonadeFallo : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 1. Comprueba si el objeto que entró es un tejo
        Tejo tejo = other.gameObject.GetComponent<Tejo>();

        if (tejo != null && tejo.jugadorID == 0) // Asumimos que solo el jugador puede fallar así
        {
            // 2. ¡NO DESTRUYAS EL TEJO!
            //    En lugar de eso, llama a TejoTermino manualmente.

            Debug.Log("¡Tejo fuera de límites! Notificando a GameManager...");

            if (GameManagerTejo.instance != null)
            {
                GameManagerTejo.instance.TejoTermino(tejo);
            }
            else
            {
                // Si el GameManager no existe, destrúyelo para evitar problemas.
                Destroy(tejo.gameObject);
            }

            // 3. ¡MUY IMPORTANTE!
            // La función TejoTermino ya se encarga de destruir el tejo
            // si la habilidad Bumerán se activa. No necesitas destruirlo aquí.
            // Si el Bumerán no se activa, el tejo quedará "muerto" 
            // y se limpiará al final de la ronda.
        }
    }
}