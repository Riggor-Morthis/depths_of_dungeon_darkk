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
    public abstract void MovementProjection();

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
}
