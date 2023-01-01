using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
    //Privates
    private Transform pivotPoint, models; //L'endroit qu'on fait tourner pour gerer tout ce qui est modele et camera, et l'endroit qu'on utilise pour donner l'impression qu'on est un pion
    private DungeonMasterScript dungeonMasterScript; //Le maitre du donjon
    private Vector3 lastMoveInput = Vector3.forward, currentMoveInput; //Le dernier input donne au joueur, et l'input qu'on traite actuellement
    private bool isMoving = false, isRotating = false, isForwarding = false, isReversing = false; //Est-ce qu'on est en train de faire un deplacement, est-ce qu'on est en train de tourner ?
    private float movingSpeed = 2.5f, attackingSpeed = 5f, rotatingSpeed; //Les vitesses de mouvement et de rotation, respectivement
    private Vector3 targetPosition; //L'endroit ou on veut aller

    /// <summary>
    /// Recoit le maitre du jeu, et initialise nos variables
    /// </summary>
    /// <param name="dms">Le script maitre du jeu</param>
    public void ReceiveDungeonMaster(DungeonMasterScript dms)
    {
        dungeonMasterScript = dms;
        pivotPoint = transform.GetChild(0);
        models = pivotPoint.GetChild(0);
    }

    /// <summary>
    /// Permet de recevoir les instructions mouvement et de gerer le deplacement du joueur de maniere propre
    /// </summary>
    /// <param name="pmi">Le vecteur de mouvement qu'on nous demande</param>
    public void ReceiveMoveInstruction(Vector3 pmi)
    {
        //On commence par initialiser les variables de mouvement
        currentMoveInput = pmi;
        targetPosition = transform.position + currentMoveInput;
        isMoving = true;

        //On regarde si on doit tourner
        ShouldWeRotate();
    }

    /// <summary>
    /// Recoit les instructions d'attaque et engage la bonne animation en consequence
    /// </summary>
    /// <param name="pai">Le vecteur d'attaque demande par le joueur</param>
    public void ReceiveAttackInstruction(Vector3 pai)
    {
        //On commence par initialiser les variables d'attaque
        currentMoveInput = pai;
        targetPosition = models.position + currentMoveInput * 0.5f;
        isForwarding = true;

        //On regarde si on doit tourner
        ShouldWeRotate();
    }

    /// <summary>
    /// Permet de savoir si on doit tourner le personnage ou pas, et fait la preparation necessaire
    /// </summary>
    private void ShouldWeRotate()
    {
        if(lastMoveInput != currentMoveInput)
        {
            rotatingSpeed = Vector3.SignedAngle(lastMoveInput, currentMoveInput, Vector3.up) * 3f;
            lastMoveInput = currentMoveInput;
            isRotating = true;
        }
    }

    private void Update()
    {
        //Ici on tourne
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

        //Ici on bouge
        else if (isMoving)
        {
            //Si on est presque a destination...
            if (Vector3.Distance(targetPosition, transform.position) <= movingSpeed * Time.deltaTime)
            {
                //... on prend la bonne position, et on dit au DM qu'on a fini
                transform.position = targetPosition;
                models.localPosition = Vector3.zero;
                dungeonMasterScript.PlayerHasMoved();
                isMoving = false;
            }
            //Sinon on bouge
            else
            {
                transform.position += currentMoveInput * movingSpeed * Time.deltaTime;
                models.localPosition = Vector3.up * ElevationAccordingToDistance();
            }
        }

        //Ici on attaque
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
        //Ici on attaque(bis)
        else if (isReversing)
        {
            //Si on est presque a destination...
            if (Vector3.Distance(targetPosition, models.position) <= movingSpeed * Time.deltaTime)
            {
                //... on prend la bonne position et on passe a la seconde phase
                models.position = targetPosition;
                dungeonMasterScript.PlayerHasAttacked();
                isReversing = false;
            }
            //Sinon on fait l'inverse du mouvement d'attaque demande pour inverser ses effets
            else models.position -= currentMoveInput * movingSpeed * Time.deltaTime;
        }
    }

    /// <summary>
    /// Le personnage bouge en arc comme si c'etait un pion, grace a cette fonction
    /// </summary>
    /// <returns>La hauteur a prendre en fonction de la ou on en est dans le deplacement</returns>
    private float ElevationAccordingToDistance()
    {
        if (Vector3.Distance(targetPosition, transform.position) >= 0.5f) return 1 - (Vector3.Distance(targetPosition, transform.position));
        else return (Vector3.Distance(targetPosition, transform.position));
    }
}
