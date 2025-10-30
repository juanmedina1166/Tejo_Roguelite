using UnityEngine;

// Este script debe ir en el Trigger que detecta cuando el tejo se va de la cancha.
public class ZonadeFallo : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 1. Comprueba si el objeto que entr� es un tejo
        Tejo tejo = other.gameObject.GetComponent<Tejo>();

        if (tejo != null && tejo.jugadorID == 0) // Asumimos que solo el jugador puede fallar as�
        {
            // 2. �NO DESTRUYAS EL TEJO!
            //    En lugar de eso, llama a TejoTermino manualmente.

            Debug.Log("�Tejo fuera de l�mites! Notificando a GameManager...");

            if (GameManagerTejo.instance != null)
            {
                GameManagerTejo.instance.TejoTermino(tejo);
            }
            else
            {
                // Si el GameManager no existe, destr�yelo para evitar problemas.
                Destroy(tejo.gameObject);
            }

            // 3. �MUY IMPORTANTE!
            // La funci�n TejoTermino ya se encarga de destruir el tejo
            // si la habilidad Bumer�n se activa. No necesitas destruirlo aqu�.
            // Si el Bumer�n no se activa, el tejo quedar� "muerto" 
            // y se limpiar� al final de la ronda.
        }
    }
}