using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeDecisionScript : ASkeletonDecisionScript
{
    /// <summary>
    /// Utiliser pour determiner ce que le squelette fait au prochain tour
    /// </summary>
    public override void DecisionMaking()
    {
        ResetAnimations();

        //Pour commencer, il faut qu'on connaisse notre distance au joueur
        currentDistance = dungeonMasterScript.GetTuileDistance((int)transform.position.x, (int)transform.position.z);
        //Si on est trop loin, on erre vaguement
        if(currentDistance > 6 || currentDistance == 0)
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
            if (currentDistance != 0)
            {
                if (Random.Range(0f, 1f) <= (1f/currentDistance)) intentionAttaque = true;
                else intentionAttaque = false;
            }
            else intentionAttaque = false;

            //On recupere les voisins de notre tuile
            currentVectors = dungeonMasterScript.GetTuileNeighbors((int)transform.position.x, (int)transform.position.z);
            //On initialise nos variables
            currentDistance = dungeonMasterScript.GetTuileDistance((int)currentVectors[0].x, (int)currentVectors[0].y);
            target = currentVectors[0];
            //On trouve celui qui nous rapproche du joueur
            foreach (Vector2 voisin in currentVectors)
            {
                if (currentDistance > dungeonMasterScript.GetTuileDistance((int)voisin.x, (int)voisin.y))
                {
                    currentDistance = dungeonMasterScript.GetTuileDistance((int)voisin.x, (int)voisin.y);
                    target = voisin;
                }
                else if (currentDistance == dungeonMasterScript.GetTuileDistance((int)voisin.x, (int)voisin.y)) if(Random.Range(0,2) == 0)
                    {
                        currentDistance = dungeonMasterScript.GetTuileDistance((int)voisin.x, (int)voisin.y);
                        target = voisin;
                    }
            }
        }

        //Une fois qu'on a fait un choix, il faut l'afficher
        skeletonMovementScript.ShowIntention(intentionAttaque);
        CommunicateIntent();
    }

    /// <summary>
    /// Modifie les couleurs de la grille pour l'UI
    /// </summary>
    private void CommunicateIntent()
    {
        dungeonMasterScript.ChangeTuileColor((int)target.x, (int)target.y, intentionAttaque);
    }

    /// <summary>
    /// Permet de projetter son attaque sur les autres squelettes du jeu
    /// </summary>
    public override void AttackProjection()
    {
        //On commence par projeter les consequences de son attaque
        dungeonMasterScript.CheckSkeletonTarget(GetTarget());
        //On enregistre le fait qu'on va avoir une animation d'attaque
        attackAnimation = true;
    }

    /// <summary>
    /// Permet de projetter son mouvement et de s'assurer qu'on va pas rentrer dans quelqu'un
    /// </summary>
    public override void MovementProjection()
    {
        //Si c'est pas prevu qu'on meurt, on peut agir
        if (!deathAnimation)
        {
            //On demande au maitre du donjon si notre mouvement sera possible
            if (dungeonMasterScript.CheckSkeletonMovement(GetTarget(), this)) movingAnimation = true; //Il sera possible, donc on s'initialise pour avoir la bonne animation
            else
            {
                //Il n'est pas possible, on s'initialise pour ne rien faire et on indique aux autres squelettes qu'on va rester sur place
                nothingAnimation = true;
                target = new Vector2(transform.position.x, transform.position.z);
            }
        }
    }
}
