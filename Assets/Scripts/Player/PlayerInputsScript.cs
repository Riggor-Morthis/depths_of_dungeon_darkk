using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputsScript : MonoBehaviour
{
    //Privates
    private DungeonMasterScript dungeonMasterScript;
    private Vector2 startingMousePosition, endingMousePosition;
    private float mouseAngle;
    private Vector3 moveInput = Vector3.forward;
    private bool validMouseInput;

    private void Awake()
    {
        validMouseInput = false;
    }

    private void Update()
    {
        //Recuperer l'endroit ou on appuie
        if (Input.GetMouseButtonDown(0))
        {
            startingMousePosition = Input.mousePosition;
            validMouseInput = true;
        }

        //Recuperer l'endroit ou on lache et en profiter pour calculer le vecteur demande
        else if (Input.GetMouseButtonUp(0) && validMouseInput)
        {
            endingMousePosition = Input.mousePosition;
            CalculateMouseVector();
        }
    }

    /// <summary>
    /// Calcule le vecteur demande par le joueur et le rend utilisable par les scripts
    /// </summary>
    private void CalculateMouseVector()
    {
        //Commence par calculer l'angle du vecteur forme
        mouseAngle = Vector2.SignedAngle(endingMousePosition - startingMousePosition, Vector2.up);

        //On sert de l'angle pour deduire quel vecteur unitaire le joueur a forme
        if (Mathf.Abs(mouseAngle) > 135) moveInput = -moveInput;
        else if(Mathf.Abs(mouseAngle) >= 45)
        {
            if (mouseAngle >= 0) moveInput = new Vector3(moveInput.z, 0, -moveInput.x);
            else moveInput = new Vector3(-moveInput.z, 0, moveInput.x);
        }

        //On communique l'input
        GiveInput();
    }

    /// <summary>
    /// Permet de communiquer les inputs joueur a un autre script, avant de desactiver celui-ci
    /// </summary>
    private void GiveInput()
    {
        //On communique les inputs
        dungeonMasterScript.ReceiveInput(moveInput);
    }

    /// <summary>
    /// Permet d'initialiser la variable de maitre de donjon
    /// </summary>
    /// <param name="dms">Le maitre du donjon</param>
    public void ReceiveDungeonMaster(DungeonMasterScript dms)
    {
        dungeonMasterScript = dms;
    }
}
