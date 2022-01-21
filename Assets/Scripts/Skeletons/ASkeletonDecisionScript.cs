using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ASkeletonDecisionScript : MonoBehaviour
{
    //Protected
    protected DungeonMasterScript dungeonMasterScript;

    public void ReceiveDungeonMaster(DungeonMasterScript dms)
    {
        dungeonMasterScript = dms;
    }

    public abstract void DecisionMaking();
}
