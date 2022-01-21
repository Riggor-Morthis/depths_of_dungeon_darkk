using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
    /// <summary>
    /// Permet de recevoir les instructions mouvement et de gerer le deplacement du joueur de maniere propre
    /// </summary>
    /// <param name="pmi">Le vecteur de mouvement qu'on nous demande</param>
    public void ReceiveMoveInstruction(Vector3 pmi)
    {
        transform.position += pmi;
    }
}
