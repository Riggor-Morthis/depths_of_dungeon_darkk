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
        if (currentDistance > 6 || currentDistance == 0)
        {
            //On sait deja qu'on attaquera pas
            intentionAttaque = false;
            Roam();
        }
        //Sinon, on va essayer de se rapprocher du joueur et de le taper
        else
        {
            //Est-ce qu'on attaque ou pas ?
            if (currentDistance != 0)
            {
                if (Random.Range(0f, 1f) <= (1f / currentDistance)) intentionAttaque = true;
                else intentionAttaque = false;
            }
            else intentionAttaque = false;

            CloserToThePlayer();
        }

        //Une fois qu'on a fait un choix, il faut l'afficher
        skeletonMovementScript.ShowIntention(intentionAttaque);
        CommunicateIntent();
    }

    /// <summary>
    /// Modifie les couleurs de la grille pour l'UI
    /// </summary>
    protected override void CommunicateIntent()
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
}
