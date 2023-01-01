using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ASkeletonDecisionScript : MonoBehaviour
{
    //Protected
    protected DungeonMasterScript dungeonMasterScript; //Le maitre du donjon
    protected int currentDistance; //Notre distance atuelle au joueur
    protected bool intentionAttaque; //Est-ce qu'on veut attaquer ou juste se deplacer ?
    protected List<Vector2> currentVectors; //Pour stocker des vecteurs
    protected Vector2 target; //La tuile qu'on cherche a attaquer
    protected bool deathAnimation, attackAnimation, movingAnimation, nothingAnimation; //Pour indiquer qu'on va connaitre des animations : mourir (i.e. ne rien faire), attaquer, bouger ou l'animation par defaut
    protected ASkeletonMovementScript skeletonMovementScript; //Le script pour nos animations

    /// <summary>
    /// Initialise la variable de maitre de donjon
    /// </summary>
    /// <param name="dms"></param>
    public void ReceiveDungeonMaster(DungeonMasterScript dms)
    {
        dungeonMasterScript = dms;
        skeletonMovementScript = GetComponent<ASkeletonMovementScript>();
        skeletonMovementScript.ReceiveDungeonMaster(dms);
    }

    /// <summary>
    /// Demande un prise de decision de la part du monstre
    /// </summary>
    public abstract void DecisionMaking();

    /// <summary>
    /// Permet de remettre les variables d'animation a 0 pour etre sur qu'on aura pas d'erreur
    /// </summary>
    protected void ResetAnimations()
    {
        deathAnimation = false;
        attackAnimation = false;
        movingAnimation = false;
        nothingAnimation = false;
    }

    /// <summary>
    /// Projette les consequences de notre attaque sur les autres squelettes
    /// </summary>
    public abstract void AttackProjection();

    /// <summary>
    /// Projette les consequences de notre mouvement sur les autres squelettes
    /// </summary>
    /// <summary>
    /// Permet de projetter son mouvement et de s'assurer qu'on va pas rentrer dans quelqu'un
    /// </summary>
    public void MovementProjection()
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

    /// <summary>
    /// On ordonne au script d'animation de faire son travail
    /// </summary>
    public void LaunchAnimation()
    {
        if (attackAnimation) skeletonMovementScript.IsAnimatingAttack(GetTarget());
        else if (movingAnimation) skeletonMovementScript.IsAnimatingMovement(GetTarget());
        else skeletonMovementScript.IsAnimatingNothing();
    }

    /// <summary>
    /// Signale a ce squelette qu'il va mourir a la prochaine update
    /// </summary>
    public void YouWillDie()
    {
        deathAnimation = true;
    }

    public bool GetIntentionAttaque() => intentionAttaque;

    public bool GetDeathAnimation() => deathAnimation;

    public Vector3 GetTarget() => new Vector3(target.x, 0, target.y);

    /// <summary>
    /// Permet de juste se deplacer vers une case valide aleatoire
    /// </summary>
    protected void Roam()
    {
        //On recupere les voisins de notre tuile
        currentVectors = dungeonMasterScript.GetTuileNeighbors((int)transform.position.x, (int)transform.position.z);
        //On en choisit un au hasard
        if (currentVectors.Count > 0) target = currentVectors[Random.Range(0, currentVectors.Count)];
        //Cas par defaut, on sait jamais
        else target = new Vector2(transform.position.x, transform.position.z);
    }
    
    /// <summary>
    /// Determine la meilleure tuile pour se rapprocher du joueur
    /// </summary>
    protected void CloserToThePlayer()
    {
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
            else if (currentDistance == dungeonMasterScript.GetTuileDistance((int)voisin.x, (int)voisin.y)) if (Random.Range(0, 2) == 0)
                {
                    currentDistance = dungeonMasterScript.GetTuileDistance((int)voisin.x, (int)voisin.y);
                    target = voisin;
                }
        }
    }

    /// <summary>
    /// Modifie les couleurs de la grille pour l'UI
    /// </summary>
    protected abstract void CommunicateIntent();
}
