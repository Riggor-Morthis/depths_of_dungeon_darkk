using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedDecisionScript : ASkeletonDecisionScript
{
    //Privates
    private Vector3 playerPosition;
    private Vector3 currentVector;

    /// <summary>
    /// Utiliser pour determiner ce que le squelette fait au prochain tour
    /// </summary>
    public override void DecisionMaking()
    {
        ResetAnimations();

        //On attaque toujours et uniquement si on est sur la meme colonne ou la meme ligne que le joueur
        playerPosition = dungeonMasterScript.GetPlayerPosition();
        if (Mathf.Abs(playerPosition.x - transform.position.x) < 0.1f || Mathf.Abs(playerPosition.z - transform.position.z) < 0.1f)
        {
            intentionAttaque = true;
            if(Mathf.Abs(playerPosition.x - transform.position.x) > Mathf.Abs(playerPosition.z - transform.position.z)) target = Vector2.right * Mathf.Sign(playerPosition.x - transform.position.x);
            else target = Vector2.up * Mathf.Sign(playerPosition.z - transform.position.z);
        }
        //Sinon, soit on erre quand on est loin du joueur, soit on va vers lui
        else
        {
            intentionAttaque = false;
            if (currentDistance > 6) Roam();
            else CloserToThePlayer();
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
        //Si on a pas l'intention d'attaquer c'est tres simple
        if (!intentionAttaque) dungeonMasterScript.ChangeTuileColor((int)target.x, (int)target.y, intentionAttaque);
        //Sinon, il faut colorier une ligne ou une colonne entiere
        else
        {
            currentVector = transform.position;
            do
            {
                currentVector += GetTarget();
            } while (dungeonMasterScript.ChangeTuileColor((int)currentVector.x, (int)currentVector.z, intentionAttaque));
        }
    }

    /// <summary>
    /// Permet de projetter son attaque sur les autres squelettes du jeu
    /// </summary>
    public override void AttackProjection()
    {
        //On commence par projeter les consequences de son attaque
        currentVector = transform.position;
        do
        {
            currentVector += GetTarget();
        } while (!dungeonMasterScript.CheckSkeletonTarget(currentVector) && !dungeonMasterScript.CheckSkeletonRangedTarget(currentVector));
        //Maintenant qu'on sait ou le tir va atterir, on peut l'enregistrer
        target = new Vector2((int)currentVector.x, (int)currentVector.z);
        //On enregistre le fait qu'on va avoir une animation d'attaque
        attackAnimation = true;
    }
}
