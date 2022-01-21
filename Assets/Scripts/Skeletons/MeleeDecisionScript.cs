using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeDecisionScript : ASkeletonDecisionScript
{
    //Privates
    private int currentDisance;
    private bool intentionAttaque;
    private List<Vector2> currentVectors;
    private Vector2 target;

    public override void DecisionMaking()
    {
        //Pour commencer, il faut qu'on connaisse notre distance au joueur
        currentDisance = dungeonMasterScript.GetTuileDistance((int)transform.position.x, (int)transform.position.z);
        //Si on est trop loin, on erre vaguement
        if(currentDisance > 6 || currentDisance == 0)
        {
            //On sait deja qu'on attaquera pas
            intentionAttaque = false;
            //On recupere les voisins de notre tuile
            currentVectors = dungeonMasterScript.GetTuileNeighbors((int)transform.position.x, (int)transform.position.z);
            //On en choisit un au hasard
            if (currentVectors.Count > 0) target = currentVectors[Random.Range(0, currentVectors.Count)];
            //Cas par defaut, on sait jamais
            else target = new Vector2(transform.position.x, transform.position.z);
        }
        //Sinon, on va essayer de se rapprocher du joueur et de le taper
        else
        {
            //Est-ce qu'on attaque ou pas ?
            if (Random.Range(0, 6) <= 6 - currentDisance) intentionAttaque = true;
            else intentionAttaque = false;

            //On recupere les voisins de notre tuile
            currentVectors = dungeonMasterScript.GetTuileNeighbors((int)transform.position.x, (int)transform.position.z);
            //On trouve celui qui nous rapproche du joueur
            foreach(Vector2 voisin in currentVectors) if(currentDisance >= dungeonMasterScript.GetTuileDistance((int)voisin.x, (int)voisin.y))
                {
                    currentDisance = dungeonMasterScript.GetTuileDistance((int)voisin.x, (int)voisin.y);
                    target = voisin;
                }
        }

        //Une fois qu'on a fait un choix, il faut l'afficher
        CommunicateIntent();
    }

    private void CommunicateIntent()
    {
        dungeonMasterScript.ChangeTuileColor((int)target.x, (int)target.y, intentionAttaque);
    }
}
