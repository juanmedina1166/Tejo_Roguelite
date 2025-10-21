// BocinTrigger.cs
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class BocinTrigger : MonoBehaviour
{
    // Una lista para rastrear qué tejos están actualmente dentro del trigger
    private List<Tejo> tejosDentro = new List<Tejo>();

    private void OnTriggerEnter(Collider other)
    {
        Tejo tejo = other.GetComponent<Tejo>();
        if (tejo != null && !tejosDentro.Contains(tejo))
        {
            Debug.Log("Un tejo ha entrado al bocín.");
            tejosDentro.Add(tejo);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Tejo tejo = other.GetComponent<Tejo>();
        if (tejo != null && tejosDentro.Contains(tejo))
        {
            Debug.Log("Un tejo ha salido del bocín.");
            tejosDentro.Remove(tejo);
        }
    }

    /// <summary>
    /// Comprueba si un tejo específico está actualmente dentro del trigger del bocín.
    /// </summary>
    public bool EstaTejoDentro(Tejo tejo)
    {
        return tejosDentro.Contains(tejo);
    }

    /// <summary>
    /// Limpia la lista de tejos, usualmente al final de una ronda.
    /// </summary>
    public void LimpiarLista()
    {
        tejosDentro.Clear();
    }
}