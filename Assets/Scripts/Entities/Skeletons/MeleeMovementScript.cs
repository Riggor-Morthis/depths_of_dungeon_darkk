using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeMovementScript : ASkeletonMovementScript
{
    //Privates
    private bool isForwarding = false, isReversing = false;

    /// <summary>
    /// Gere l'initialisation de l'animation d'attaque pour nos squelettes en melee
    /// </summary>
    /// <param name="tp">L'endroit ou on veut aller</param>
    public override void IsAnimatingAttack(Vector3 tp)
    {
        //On commence par de l'initialisation de variable
        currentMoveInput = tp - transform.position;
        targetPosition = models.position + currentMoveInput * 0.5f;
        isAttacking = true;
        isForwarding = true;
        isAnimating = true;

        //On regarde si on doit tourner
        ShouldWeRotate();
    }

    /// <summary>
    /// Gere l'animation d'attaque pour nos squelettes en melee
    /// </summary>
    protected override void AttackAnimation()
    {
        //Si on est en train de tourner, il faut gerer ce mouvement
        if (isRotating)
        {
            //Si on est presque a la fin...
            if (Vector3.Angle(pivotPoint.forward, currentMoveInput) <= Mathf.Abs(rotatingSpeed * Time.deltaTime))
            {
                //... on prend la bonne position, et on arrete de tourner
                pivotPoint.Rotate(Vector3.up, Vector3.SignedAngle(pivotPoint.forward, currentMoveInput, Vector3.up));
                isRotating = false;
            }
            //Sinon on tourne
            else pivotPoint.Rotate(Vector3.up, rotatingSpeed * Time.deltaTime);
        }
        //Si on est en train d'attaquer, il faut gerer un mouvement puis l'autre
        else if (isForwarding)
        {
            //Si on est presque a destination...
            if (Vector3.Distance(targetPosition, models.position) <= attackingSpeed * Time.deltaTime)
            {
                //... on prend la bonne position et on demande a blesser ce qui se trouve sur une tuile donnee
                models.position = targetPosition;
                targetPosition = transform.position + new Vector3(0.5f, 0, 0.5f);
                isForwarding = false;
                dungeonMasterScript.AttackTentative(transform.position + currentMoveInput, true);
                isReversing = true;
            }
            //Sinon on fait le mouvement d'attaque demande
            else models.position += currentMoveInput * attackingSpeed * Time.deltaTime;
        }
        else if (isReversing)
        {
            //Si on est presque a destination...
            if (Vector3.Distance(targetPosition, models.position) <= movingSpeed * Time.deltaTime)
            {
                //... on prend la bonne position et on le dit a mj
                models.position = targetPosition;
                isReversing = false;
                dungeonMasterScript.SkeletonHasActed();
                isAttacking = false;
                isAnimating = false;
            }
            //Sinon on fait l'inverse du mouvement d'attaque demande pour inverser ses effets
            else models.position -= currentMoveInput * movingSpeed * Time.deltaTime;
        }
    }
}
