using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
    //Privates
    private Transform pivotPoint, models; //L'endroit qu'on fait tourner pour gerer tout ce qui est modele et camera, et l'endroit qu'on utilise pour donner l'impression qu'on est un pion
    private DungeonMasterScript dungeonMasterScript; //Le maitre du donjon
    private Vector3 lastMoveInput = Vector3.forward, currentMoveInput, currentAttackInput; //Le dernier input donne au joueur, et l'input qu'on traite actuellement
    private bool isMoving = false, isRotating = false, isForwarding = false, isReversing = false; //Est-ce qu'on est en train de faire un deplacement, est-ce qu'on est en train de tourner ?
    private float movingSpeed = 2f, rotatingSpeed; //Les vitesses de mouvement et de rotation, respectivement
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
        //On commence par initialiser les variables
        currentMoveInput = pmi;
        targetPosition = transform.position + currentMoveInput;

        //Si on fait le meme mouvement que la derniere fois, on a pas besoin de bouger notre camera
        if(lastMoveInput == currentMoveInput) isMoving = true;
        else
        {
            rotatingSpeed = Vector3.SignedAngle(lastMoveInput, currentMoveInput, Vector3.up) * 2f;
            lastMoveInput = currentMoveInput;
            isRotating = true;
        }
    }

    public void ReceiveAttackInstruction(Vector2 pai)
    {
        currentAttackInput = pai;
        targetPosition = models.position + currentAttackInput * 0.5f;
        isForwarding = true;
    }

    private void Update()
    {
        //GESTION MOUVEMENT//
        //Ici on tourne
        if (isRotating)
        {
            //Si on est presque a la fin...
            if (Vector3.Angle(pivotPoint.forward, currentMoveInput) <= Mathf.Abs(rotatingSpeed * Time.deltaTime))
            {
                //... on prend la bonne position, et on engage la phase de deplacement
                pivotPoint.Rotate(Vector3.up, Vector3.SignedAngle(pivotPoint.forward, currentMoveInput, Vector3.up));
                isRotating = false;
                isMoving = true;
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
        //GESTION ATTAQUE//
        //La partie avancer
        else if (isForwarding)
        {
            //Si on est presque a destination...
            if (Vector3.Distance(targetPosition, models.position) <= movingSpeed * Time.deltaTime)
            {
                //... on prend la bonne position et on passe a la seconde phase
                models.position = targetPosition;
                targetPosition = transform.position + new Vector3(0.5f, 0, 0.5f);
                isForwarding = false;
                isReversing = true;
            }
            //Sinon on fait le mouvement d'attaque demande
            else models.position += currentAttackInput * Time.deltaTime;
        }
        //La partie reculer
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
            else models.position -= currentAttackInput * Time.deltaTime;
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
