using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonMasterScript : MonoBehaviour
{
    //Privates
    private GameObject playerGameObject; //Pour stocker le joueur
    private PlayerInputsScript playerInputScript; //Script d'inputs joueur
    private PlayerMovementScript playerMovementScript; //Script de mouvement joueur
    private Vector3 playerMoveInput; //La direction que le joueur veut prendre
    private FloorScript[,] solDonjon; //Les tuiles de notre donjon pour verifier les deplacements
    private int levelWidth, levelHeight; //Les dimensions du donjon
    private List<Vector2> currentTuiles, prevTuiles, nextTuiles, neighboringTuiles; //Utilisees par l'algo de distance au joueur, respectivement :
                                                                                    //les tuiles en cours de traitement, celles deja traitees, celles traitees a la prochaine iteration, celles renvoyees par la tuile en cours
    private int currentInt; //Pour stocker un entier temporaire
    private List<ASkeletonDecisionScript> skeletonList; //Tous les squelettes presents dans notre niveau

    private void Start()
    {
        //Pour commencer, on genere un donjon
        GetComponent<DungeonBuilderScript>().DungeonBuilder();
        //On demande ensuite a la grille de gerer la distance au joueur
        PlayerDistance();
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
        playerMovementScript.ReceiveDungeonMaster(this);
    }

    /// <summary>
    /// Permet de recevoir une copie du tableau de tuiles qui compose notre sol
    /// </summary>
    /// <param name="sd">La liste des tuiles qui composent notre sol</param>
    public void ReceiveFloor(FloorScript[,] sd, int lw, int lh)
    {
        solDonjon = sd;
        levelWidth = lw;
        levelHeight = lh;
    }

    public void ReceiveSkeletons(List<ASkeletonDecisionScript> sl)
    {
        skeletonList = sl;
        foreach (ASkeletonDecisionScript skeleton in skeletonList) skeleton.ReceiveDungeonMaster(this);
    }

    /// <summary>
    /// Attribue la distance au joueur a chaque tuile (pour les decisions de monstres)
    /// </summary>
    private void PlayerDistance()
    {
        //On reinitialise les variables
        currentTuiles = new List<Vector2>();
        nextTuiles = new List<Vector2>();
        prevTuiles = new List<Vector2>();
        neighboringTuiles = new List<Vector2>();
        currentInt = 0;

        //On amorce la boucle
        currentTuiles.Add(new Vector2(playerGameObject.transform.position.x, playerGameObject.transform.position.z));
        //La boucle
        do
        {
            //Tant qu'on a des tuiles a traiter, on va tourner
            nextTuiles.Clear();
            //On s'interesse a toutes les tuiles actuellement dans la pile
            foreach (Vector2 tuile in currentTuiles)
            {
                //On donne la distance et on recupere les voisins
                neighboringTuiles = solDonjon[(int)tuile.x, (int)tuile.y].SetDistance(currentInt);
                //Si on a encore jamais traite ce voisin, on l'ajoute a la pile
                foreach (Vector2 voisin in neighboringTuiles) if (!currentTuiles.Contains(voisin) && !prevTuiles.Contains(voisin) && !nextTuiles.Contains(voisin)) nextTuiles.Add(voisin);
            }
            //On fait les echanges qu'il faut
            prevTuiles.AddRange(currentTuiles);
            currentTuiles.Clear();
            currentTuiles.AddRange(nextTuiles);

            //On oublie pas d'incrementer la distance
            currentInt++;
        } while (currentTuiles.Count != 0);

        //On peut lancer la suite de la boucle de gameplay
        SkeletonDecisions();
    }

    private void SkeletonDecisions()
    {
        foreach (ASkeletonDecisionScript skeleton in skeletonList) skeleton.DecisionMaking();
        
        //On peut lancer la suite de la boucle de gameplay
        AllowPlayerMovement(true);
    }

    public int GetTuileDistance(int x, int y) => solDonjon[x, y].GetDistance();

    public List<Vector2> GetTuileNeighbors(int x, int y) => solDonjon[x, y].GetVoisins();

    public void ChangeTuileColor(int x, int y, bool i)
    {
        solDonjon[x, y].ChangeColor(i);
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
        if (CheckMovementLegality()) OrderPlayerMovement();
        else AllowPlayerMovement(true);
    }

    /// <summary>
    /// Permet de s'assurer que le mouvement est legal (on finit pas dans le vide, on va se mettre a la place d'un squelette)
    /// </summary>
    private bool CheckMovementLegality()
    {
        if(playerGameObject.transform.position.x + playerMoveInput.x >= 0 && playerGameObject.transform.position.x + playerMoveInput.x < levelWidth)
        {
            if(playerGameObject.transform.position.z + playerMoveInput.z >= 0 && playerGameObject.transform.position.z + playerMoveInput.z < levelHeight)
            {
                if (solDonjon[(int)(playerGameObject.transform.position.x + playerMoveInput.x), (int)(playerGameObject.transform.position.z + playerMoveInput.z)] != null) return true;
            }
        }
        return false;
    }

    /// <summary>
    /// On ordonne au script de mouvement de gerer le deplacement qu'on indique
    /// </summary>
    private void OrderPlayerMovement()
    {
        playerMovementScript.ReceiveMoveInstruction(playerMoveInput);
    }

    /// <summary>
    /// Le joueur a fini son mouvement et on peut donc passer a la suite
    /// </summary>
    public void PlayerHasMoved()
    {
        //On peut lancer la suite de la boucle de gameplay
        TuilesReset();
    }

    private void TuilesReset()
    {
        for(int i = 0; i < levelWidth; i++) for(int j = 0; j < levelHeight; j++) if (solDonjon[i, j] != null) solDonjon[i, j].ResetColor();

        //On peut lancer la suite de la boucle de gameplay
        PlayerDistance();
    }
}
