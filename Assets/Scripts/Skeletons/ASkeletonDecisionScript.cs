using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ASkeletonDecisionScript : MonoBehaviour
{
    //Protected
    protected DungeonMasterScript dungeonMasterScript;

    /// <summary>
    /// Initialise la variable de maitre de donjon
    /// </summary>
    /// <param name="dms"></param>
    public void ReceiveDungeonMaster(DungeonMasterScript dms)
    {
        dungeonMasterScript = dms;
    }

    /// <summary>
    /// Demande un prise de decision de la part du monstre
    /// </summary>
    public abstract void DecisionMaking();
}
