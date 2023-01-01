using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ASkeletonMovementScript : MonoBehaviour
{
    //Protected
    protected Transform pivotPoint, models; //L'endroit qu'on fait tourner pour gerer tout ce qui est modele, et l'endroit qu'on utilise pour donner l'impression qu'on est un pion
    protected DungeonMasterScript dungeonMasterScript; //Le maitre du donjon
    protected Vector3 lastMoveInput = Vector3.forward, currentMoveInput; //Le mouvement fait, et l'input qu'on traite actuellement
    protected bool isNothing = false, isMoving = false, isAttacking = false, isRotating = false; //Qu'est ce qu'on va faire a ce tour : une attaque, un mouvement ou rien
    protected float movingSpeed = 2.5f, attackingSpeed = 5f, rotatingSpeed; //Les vitesses de mouvement et de rotation, respectivement
    protected Vector3 targetPosition; //L'endroit ou on veut aller
    protected bool isAnimating = false; //Indique qu'on est en train de s'animer
    protected bool intentionAttaque = false; //Est-ce qu'on compte attaquer/quel model on utilise
    protected GameObject roamModel, attackModel; //Les deux modeles graphiques du squelette

    /// <summary>
    /// Permet de montrer plus explicitement nos intentions en changeant notre modele lorsqu'on s'apprete a attaquer
    /// </summary>
    /// <param name="intention"></param>
    public void ShowIntention(bool intention)
    {
        //Si l'intention actuelle est differente de la derniere, on doit changer de modele
        if(intention != intentionAttaque)
        {
            intentionAttaque = intention;
            if (intentionAttaque)
            {
                roamModel.SetActive(false);
                attackModel.SetActive(true);
            }
            else
            {
                attackModel.SetActive(false);
                roamModel.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Recoit le maitre du jeu, et initialise nos variables
    /// </summary>
    /// <param name="dms">Le script maitre du jeu</param>
    public void ReceiveDungeonMaster(DungeonMasterScript dms)
    {
        dungeonMasterScript = dms;
        pivotPoint = transform.GetChild(0);
        models = pivotPoint.GetChild(0);
        roamModel = models.GetChild(0).gameObject;
        attackModel = models.GetChild(1).gameObject;
        attackModel.SetActive(false);
    }

    /// <summary>
    /// On nous signale qu'on va rien faire ce tour
    /// </summary>
    public void IsAnimatingNothing()
    {
        isNothing = true;
        isAnimating = true;
    }

    /// <summary>
    /// On nous signale qu'on va animer du mouvement pour ce tour
    /// </summary>
    public void IsAnimatingMovement(Vector3 tp)
    {
        //On commence par de l'initialisation de variable
        targetPosition = tp;
        currentMoveInput = targetPosition - transform.position;
        isMoving = true;
        isAnimating = true;

        //On regarde si on doit tourner
        ShouldWeRotate();
    }

    /// <summary>
    /// Comme les animations changent un peu selon le squelette qui agit, on va faire ca dans chacune des classes enfants
    /// </summary>
    /// <param name="tp">La case a attaquer</param>
    public abstract void IsAnimatingAttack(Vector3 tp);

    /// <summary>
    /// Permet de savoir si on doit tourner le personnage ou pas, et fait la preparation necessaire
    /// </summary>
    protected void ShouldWeRotate()
    {
        if (lastMoveInput != currentMoveInput)
        {
            rotatingSpeed = Vector3.SignedAngle(lastMoveInput, currentMoveInput, Vector3.up) * 3f;
            lastMoveInput = currentMoveInput;
            isRotating = true;
        }
    }

    protected void Update()
    {
        if (isAnimating)
        {
            if (isAttacking)
            {
                AttackAnimation();
            }
            else if (isMoving)
            {
                MovementAnimation();
            }
            else if (isNothing)
            {
                //On a juste a dire au maitre du jeu qu'on a fini de rien, puis il suffit de mettre les variables en ordre
                dungeonMasterScript.SkeletonHasActed();
                isNothing = false;
                isAnimating = false;
            }
        }
    }

    /// <summary>
    /// Permet de gerer les animations de mouvement de nos squelettes (on integre la rotation dans la translation)
    /// </summary>
    protected void MovementAnimation()
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
        //Si on est en train de bouger, il faut gerer ce mouvement
        if (isMoving)
        {
            //Si on est presque a destination...
            if (Vector3.Distance(targetPosition, transform.position) <= movingSpeed * Time.deltaTime)
            {
                //... on prend la bonne position, et on dit au DM qu'on a fini
                transform.position = targetPosition;
                models.localPosition = Vector3.zero;
                isMoving = false;
                dungeonMasterScript.SkeletonHasActed();
                isAnimating = false;
            }
            //Sinon on bouge
            else
            {
                transform.position += currentMoveInput * movingSpeed * Time.deltaTime;
                models.localPosition = Vector3.up * ElevationAccordingToDistance();
            }
        }
    }

    /// <summary>
    /// Le personnage bouge en arc comme si c'etait un pion, grace a cette fonction
    /// </summary>
    /// <returns>La hauteur a prendre en fonction de la ou on en est dans le deplacement</returns>
    protected float ElevationAccordingToDistance()
    {
        if (Vector3.Distance(targetPosition, transform.position) >= 0.5f) return 1 - (Vector3.Distance(targetPosition, transform.position));
        else return (Vector3.Distance(targetPosition, transform.position));
    }

    /// <summary>
    /// Comme les animations changent un peu selon le squelette qui agit, on va faire ca dans chacune des classes enfants
    /// </summary>
    protected abstract void AttackAnimation();
}
