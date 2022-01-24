using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ADamageableScript : MonoBehaviour
{
    /// <summary>
    /// Indique ce que l'entite fait lorsqu'elle se fait attaquer
    /// </summary>
    public abstract void GetDamaged();
}
