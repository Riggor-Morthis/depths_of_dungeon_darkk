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
    private List<TreasureScript> treasureList; //Tous les tresors presents dans notre niveau

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

    /// <summary>
    /// Recoit la liste des squelettes presents dans ce niveau
    /// </summary>
    /// <param name="sl">La liste des squelettes qu'on a place dans notre niveau</param>
    public void ReceiveSkeletons(List<ASkeletonDecisionScript> sl)
    {
        skeletonList = sl;
        foreach (ASkeletonDecisionScript skeleton in skeletonList) skeleton.ReceiveDungeonMaster(this);
    }
    
    /// <summary>
    /// Recoit la liste des tresors dans ce niveau
    /// </summary>
    /// <param name="tl">La liste des tresors qu'on a place dans notre niveau</param>
    public void ReceiveTreasures(List<TreasureScript> tl)
    {
        treasureList = tl;
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

    /// <summary>
    /// Ordonne a tous nos squelettes de faire un choix
    /// </summary>
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
        if (CheckMovementLegality(playerGameObject.transform.position, playerMoveInput) == 1) OrderPlayerMovement();
        else AllowPlayerMovement(true);
    }

    /// <summary>
    /// Permet de savoir le resultat d'un mouvement theorique : 0 pour un mouvement qui est impossible,
    /// 1 pour un mouvement qui est possible, 2 pour un mouvement qui est impossible mais avec une attaque qui est possible
    /// </summary>
    /// <param name="positionInitiale">L'endroit ou se trouve l'entite</param>
    /// <param name="mouvementDesire">La ou elle veut aller</param>
    /// <returns>0 si le mouvement est absolument impossible (pas de tuile a cet endroit)
    ///          1 si le mouvement est absolument possible (personne la ou on va)
    ///          2 si le mouvement est impossible mais l'attaque est possible (quelqu'un la ou on va)</returns>
    private int CheckMovementLegality(Vector3 positionInitiale, Vector3 mouvementDesire)
    {
        //On commence par s'assurer que le mouvement envoie bien sur une tuile qui existe (dans le tableau et non null)
        if(positionInitiale.x + mouvementDesire.x >= 0 && positionInitiale.x + mouvementDesire.x < levelWidth)
        {
            if(positionInitiale.z + mouvementDesire.z >= 0 && positionInitiale.z + mouvementDesire.z < levelHeight)
            {
                if(solDonjon[(int)(positionInitiale.x + mouvementDesire.x), (int)(positionInitiale.z + mouvementDesire.z)] != null)
                {
                    //Maintenant on veut savoir si la tuile est occupee ou non
                    positionInitiale += mouvementDesire;
                    if (Vector3.Distance(positionInitiale, playerGameObject.transform.position) < 0.1f) return 2;
                    else foreach (ASkeletonDecisionScript skeleton in skeletonList) if (Vector3.Distance(positionInitiale, skeleton.transform.position) < 0.1f) return 2;
                    return 1;
                }
            }
        }
        return 0;
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
        CollectMoney();
    }

    /// <summary>
    /// Demande a chacun des tresors dans la scene de savoir si le joueur l'a ramasse ou non
    /// </summary>
    private void CollectMoney()
    {
        //On inspecte chaque tresor
        foreach(TreasureScript treasure in treasureList)
        {
            if (treasure.gameObject.activeInHierarchy) if (Vector3.Distance(treasure.transform.position, playerGameObject.transform.position) < 0.1f) treasure.TreasureCollected();
        }
        //On peut lancer la suite de la boucle de gameplay
        TuilesReset();
    }

    /// <summary>
    /// Remet "a 0" les tuiles de notre grille pour effacer notre UI
    /// </summary>
    private void TuilesReset()
    {
        for(int i = 0; i < levelWidth; i++) for(int j = 0; j < levelHeight; j++) if (solDonjon[i, j] != null) solDonjon[i, j].ResetColor();

        //On peut lancer la suite de la boucle de gameplay
        PlayerDistance();
    }
}
