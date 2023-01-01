using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputsScript : MonoBehaviour
{
    //Privates
    private DungeonMasterScript dungeonMasterScript;
    private Vector3 startingMousePosition, endingMousePosition;
    private float mouseAngle;
    private Vector3 moveInput = Vector3.forward;
    private bool validMouseInput = false;
    private Camera mainCamera;
    private Plane flatPlane = new Plane(Vector3.up, Vector3.zero);
    private float currentFloat;
    private Ray currentRay;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        //Recuperer l'endroit ou on appuie
        if (Input.GetMouseButtonDown(0))
        {
            currentRay = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (flatPlane.Raycast(currentRay, out currentFloat)) startingMousePosition = currentRay.GetPoint(currentFloat);
            validMouseInput = true;
        }

        //Recuperer l'endroit ou on lache et en profiter pour calculer le vecteur demande
        if (Input.GetMouseButtonUp(0) && validMouseInput)
        {
            currentRay = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (flatPlane.Raycast(currentRay, out currentFloat)) endingMousePosition = currentRay.GetPoint(currentFloat);
            CalculateMouseVector();
        }
    }

    /// <summary>
    /// Calcule le vecteur demande par le joueur et le rend utilisable par les scripts
    /// </summary>
    private void CalculateMouseVector()
    {
        //Reset de variables
        validMouseInput = false;

        //On fait l'angle entre notre vector d'input et le "haut" du plateau
        mouseAngle = Vector3.SignedAngle(transform.forward, endingMousePosition - startingMousePosition,Vector3.up);
        //On utilise l'angle pour determiner quel vecteur le joueur voulait faire
        if (Mathf.Abs(mouseAngle) < 45) moveInput = transform.forward;
        else if (Mathf.Abs(mouseAngle) > 135) moveInput = - transform.forward;
        else
        {
            if (mouseAngle > 0) moveInput = transform.right;
            else moveInput = - transform.right;
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
