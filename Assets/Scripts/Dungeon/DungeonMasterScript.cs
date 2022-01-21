using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonMasterScript : MonoBehaviour
{
    //Privates
    private GameObject playerGameObject;
    private PlayerInputsScript playerInputScript;
    private PlayerMovementScript playerMovementScript;
    private Vector3 playerMoveInput;
    private FloorScript[,] solDonjon;

    private void Start()
    {
        //Pour commencer, on genere un donjon
        GetComponent<DungeonBuilderScript>().DungeonBuilder();
        //On peut lancer la premiere fonction de la boucle de gameplay
        AllowPlayerMovement(true);
    }

    /// <summary>
    /// Permet de recevoir le gameobject joueur lorsqu'il est cree + initialise nos references aux scripts joueur
    /// </summary>
    /// <param name="p">Le gameobject du joueur</param>
    public void ReceivePlayer(GameObject p)
    {
        //On commence par le gameobject
        playerGameObject = p;
        //Ensuite le player inputscript, qu'on initialise avant de le desactiver (conflits d'inputs)
        playerInputScript = playerGameObject.GetComponent<PlayerInputsScript>();
        playerInputScript.ReceiveDungeonMaster(this);
        AllowPlayerMovement(false);
        //Le script de mouvement joueur
        playerMovementScript = playerGameObject.GetComponent<PlayerMovementScript>();
    }

    /// <summary>
    /// Permet de recevoir une copie du tableau de tuiles qui compose notre sol
    /// </summary>
    /// <param name="sd">La liste des tuiles qui composent notre sol</param>
    public void ReceiveFloor(FloorScript[,] sd)
    {
        solDonjon = sd;
    }

    /// <summary>
    /// Active ou desactive les mouvements du joueur en desactivant le script d'input
    /// </summary>
    /// <param name="b">Est-ce qu'on autorise ou pas</param>
    private void AllowPlayerMovement(bool b)
    {
        playerInputScript.enabled = b;
    }

    /// <summary>
    /// Permet de recevoir les inputs demandes par le joueur
    /// </summary>
    /// <param name="pi">Le vecteur de deplacement desire</param>
    public void ReceiveInput(Vector3 pi)
    {
        playerMoveInput = pi;
        //On desactive les inputs (conflit) puis on verifie que le deplacement est legal
        AllowPlayerMovement(false);
        CheckMovementLegality();
    }

    /// <summary>
    /// Permet de s'assurer que le mouvement est legal (on finit pas dans le vide, on va se mettre a la place d'un squelette)
    /// </summary>
    private void CheckMovementLegality()
    {
        if (solDonjon[(int)(playerGameObject.transform.position.x + playerMoveInput.x), (int)(playerGameObject.transform.position.z + playerMoveInput.z)] != null) OrderPlayerMovement();
        else AllowPlayerMovement(true);
    }

    /// <summary>
    /// On ordonne au script de mouvement de gerer le deplacement qu'on indique
    /// </summary>
    private void OrderPlayerMovement()
    {
        playerMovementScript.ReceiveMoveInstruction(playerMoveInput);
        AllowPlayerMovement(true);
    }
}
