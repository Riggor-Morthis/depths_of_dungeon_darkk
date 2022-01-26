using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonBuilderScript : MonoBehaviour
{
    //Prefabs
    [SerializeField]
    private GameObject playerPrefab, meleeSkeletonPrefab, rangedSkeletonPrefab, dungeonFloorPrefab, dungeonStairsPrefab, treasurePrefab;
    
    //Parent
    [SerializeField]
    private GameObject currentFloor, currentPlayer, currentSkeletons, currentTreasures;

    //Privates
    private Vector2 currentVector2; //Stocke un vecteur temporaire
    private GameObject currentGameObject; //Stocker un game object temporairement
    private int levelWidth, levelHeight; //Taille du niveau
    private Vector2 playerPosition, stairsPosition; //Position du debut et de la fin
    private List<Vector2> holesPositions, enemyPositions, treasuresPositions; //Liste des trous dans le niveau
    private FloorScript[,] tuiles; //La liste des dalles qui composent notre sol
    private List<ASkeletonDecisionScript> skeletonList; //La liste des squelettes dans le niveau
    private List<TreasureScript> treasureList; //La liste des tresors dans le niveau
    private DungeonMasterScript dungeonMasterScript; //Le maitre de ce donjon
    private int currentInt; //Pour stocker un int temporaire

    /// <summary>
    /// La fonction chargee d'assembler les donnes du planner pour avoir un vrai donjon
    /// </summary>
    public void DungeonBuilder()
    {
        GetPlannedFloor();
        BuildFloor();
        LinkNeighbors();
        PlaceEnemies();
        PlaceTreasures();
    }

    /// <summary>
    /// Recupere les donnees du planner pour constuire le niveau
    /// </summary>
    private void GetPlannedFloor()
    {
        //On demande un nouveau niveau
        DungeonPlannerScript.DungeonPlanner();

        //On recupere les ressources
        levelWidth = DungeonPlannerScript.getLevelWidth();
        levelHeight = DungeonPlannerScript.getLevelHeight();
        playerPosition = DungeonPlannerScript.getPlayerPosition();
        stairsPosition = DungeonPlannerScript.getStairsPosition();
        holesPositions = DungeonPlannerScript.getHolesPositions();
        enemyPositions = DungeonPlannerScript.getEnemyPositions();
        treasuresPositions = DungeonPlannerScript.getTreasuresPositions();
    }

    /// <summary>
    /// Assemble le sol du niveau, en s'assurant qu'on a bien des trous aux bons endroits
    /// </summary>
    private void BuildFloor()
    {
        //Initialisation de variables
        tuiles = new FloorScript[levelWidth,levelHeight];
        //On construit la base du niveau
        for(int i = 0; i < levelWidth; i++)
        {
            for(int j = 0; j < levelHeight; j++)
            {
                currentVector2 = new Vector2(i, j);
                if (!holesPositions.Contains(currentVector2))
                {
                    if (currentVector2 == stairsPosition)
                    {
                        currentGameObject = GameObject.Instantiate(dungeonStairsPrefab, new Vector3(i, 0, j), Quaternion.identity, currentFloor.transform);
                        currentGameObject.GetComponent<FloorScript>().Initialize((i + j) % 2, true);
                    }
                    else
                    {
                        currentGameObject = GameObject.Instantiate(dungeonFloorPrefab, new Vector3(i, 0, j), Quaternion.identity, currentFloor.transform);
                        currentGameObject.GetComponent<FloorScript>().Initialize((i + j) % 2, false);
                    }

                    tuiles[i, j] = currentGameObject.GetComponent<FloorScript>();
                }
                else tuiles[i, j] = null;
            }
        }

        //On place le joueur
        currentGameObject = GameObject.Instantiate(playerPrefab, new Vector3(playerPosition.x, 0, playerPosition.y), Quaternion.identity, currentPlayer.transform);
        dungeonMasterScript = GetComponent<DungeonMasterScript>();
        dungeonMasterScript.ReceivePlayer(currentGameObject, new Vector3(stairsPosition.x, 0, stairsPosition.y));
    }

    /// <summary>
    /// Permet d'indiquer a chaque tuile qui sont ses voisins
    /// </summary>
    private void LinkNeighbors()
    {
        //On ajoute les voisins si ils existent
        for (int i = 0; i < levelWidth; i++)
        {
            for (int j = 0; j < levelHeight; j++)
            {
                if(tuiles[i,j] != null)
                {
                    if (i - 1 >= 0 && tuiles[i - 1, j] != null) tuiles[i, j].AddNeighbor(new Vector2(i - 1, j));
                    if (i + 1 <= levelWidth - 1 && tuiles[i + 1, j] != null) tuiles[i, j].AddNeighbor(new Vector2(i + 1, j));
                    if (j - 1 >= 0 && tuiles[i, j - 1] != null) tuiles[i, j].AddNeighbor(new Vector2(i, j - 1));
                    if (j + 1 <= levelHeight - 1 && tuiles[i, j + 1] != null) tuiles[i, j].AddNeighbor(new Vector2(i, j + 1));
                }
            }
        }

        //On communique les tuiles au maitre du donjon
        dungeonMasterScript.ReceiveFloor(tuiles, levelWidth, levelHeight);
    }

    /// <summary>
    /// Charger de placer les ennemis, et gere la quantite de melee vs ranged
    /// </summary>
    private void PlaceEnemies()
    {
        currentInt = 0;
        skeletonList = new List<ASkeletonDecisionScript>();
        foreach (Vector2 enemy in enemyPositions)
        {
            currentInt++;
            if(currentInt % 3 == 0) currentGameObject = GameObject.Instantiate(rangedSkeletonPrefab, new Vector3(enemy.x, 0, enemy.y), Quaternion.identity, currentSkeletons.transform);
            else currentGameObject =  GameObject.Instantiate(meleeSkeletonPrefab, new Vector3(enemy.x, 0, enemy.y), Quaternion.identity, currentSkeletons.transform);
            skeletonList.Add(currentGameObject.GetComponent<ASkeletonDecisionScript>());
        }
        dungeonMasterScript.ReceiveSkeletons(skeletonList);
    }

    /// <summary>
    /// Place les tresores dans notre niveau
    /// </summary>
    private void PlaceTreasures()
    {
        treasureList = new List<TreasureScript>();
        foreach (Vector2 treasure in treasuresPositions)
        {
            currentGameObject = GameObject.Instantiate(treasurePrefab, new Vector3(treasure.x, 0f, treasure.y), Quaternion.identity, currentTreasures.transform);
            treasureList.Add(currentGameObject.GetComponent<TreasureScript>());
        }
        dungeonMasterScript.ReceiveTreasures(treasureList);
    }
}
