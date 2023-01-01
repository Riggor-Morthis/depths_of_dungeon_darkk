using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedMovementScript : ASkeletonMovementScript
{
    //Privates
    [SerializeField]
    private GameObject projectileParticle;
    private GameObject currentProjectile;
    private AudioManagerScript audioManager;

    private void Awake()
    {
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManagerScript>();
    }

    /// <summary>
    /// Tire un projectile en ligne droite
    /// </summary>
    /// <param name="tp">L'endroit ou on veut aller</param>
    public override void IsAnimatingAttack(Vector3 tp)
    {
        //Initialisation de variables
        currentMoveInput = (tp - transform.position).normalized;
        targetPosition = tp;
        attackingSpeed = (tp - transform.position).magnitude * 3f;
        isAttacking = true;
        isAnimating = true;

        //On cree notre projectile
        audioManager.Play("Magic");
        currentProjectile = GameObject.Instantiate(projectileParticle, transform.position, Quaternion.identity, transform);
        currentProjectile.transform.GetChild(0).transform.Rotate(Vector3.up, -Vector3.SignedAngle(currentMoveInput, Vector3.forward, Vector3.up));

        //On regarde si on doit tourner au passage
        ShouldWeRotate();
    }

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
        //Si on est train d'attaquer, il faut faire avancer notre projectile
        else if (isAttacking)
        {
            //Si on est presque a destination...
            if (Vector3.Distance(targetPosition, currentProjectile.transform.position) <= attackingSpeed * Time.deltaTime)
            {
                //... on demande au maitre de donjon de faire une attaque et on detruit notre projectile
                dungeonMasterScript.AttackTentative(targetPosition, false);
                GameObject.Destroy(currentProjectile);
                dungeonMasterScript.SkeletonHasActed();
                isAttacking = false;
                isAnimating = false;
            }
            //Sinon on fait avancer le projectile
            else currentProjectile.transform.position += currentMoveInput * attackingSpeed * Time.deltaTime;
        }
    }
}
