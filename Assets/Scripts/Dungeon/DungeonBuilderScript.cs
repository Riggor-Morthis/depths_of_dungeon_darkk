using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonBuilderScript : MonoBehaviour
{
    //Prefabs
    [SerializeField]
    private GameObject PlayerPrefab, MeleeSkeletonPrefab, RangedSkeletonPrefab, DungeonFloorPrefab, DungeonStairsPrefab, TreasurePrefab;
    
    //Parent
    [SerializeField]
    private GameObject CurrentFloor;

    //Privates
    private Vector2 currentVector2; //Stocke un vecteur temporaire
    private Vector3 currentVector3; //cf. au dessus
    private int levelWidth, levelHeight; //Taille du niveau
    private Vector2 playerPosition, stairsPosition; //Position du debut et de la fin
    private List<Vector2> holesPositions, enemyPositions, treasuresPositions; //Liste des trous dans le niveau

    private void Awake()
    {
        DungeonBuilder();
    }

    /// <summary>
    /// La fonction chargee d'assembler les donnes du planner pour avoir un vrai donjon
    /// </summary>
    private void DungeonBuilder()
    {
        GetPlannedFloor();
        BuildFloor();
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
        //On construit la base du niveau
        for(int i = 0; i < levelWidth; i++)
        {
            for(int j = 0; j < levelHeight; j++)
            {
                currentVector2 = new Vector2(i, j);
                if (!holesPositions.Contains(currentVector2))
                {
                    if(currentVector2 == stairsPosition) GameObject.Instantiate(DungeonStairsPrefab, new Vector3(i, 0, j), Quaternion.identity, CurrentFloor.transform);
                    else GameObject.Instantiate(DungeonFloorPrefab, new Vector3(i, 0, j), Quaternion.identity, CurrentFloor.transform);
                }
            }
        }

        //On place le joueur
        GameObject.Instantiate(PlayerPrefab, new Vector3(playerPosition.x, 0, playerPosition.y), Quaternion.identity, CurrentFloor.transform);
    }

    /// <summary>
    /// Charger de placer les ennemis, et gere la quantite de melee vs ranged
    /// </summary>
    private void PlaceEnemies()
    {
        foreach (Vector2 enemy in enemyPositions) GameObject.Instantiate(MeleeSkeletonPrefab, new Vector3(enemy.x, 0, enemy.y), Quaternion.identity, CurrentFloor.transform);
    }

    /// <summary>
    /// Place les tresores dans notre niveau
    /// </summary>
    private void PlaceTreasures()
    {
        foreach (Vector2 treasure in treasuresPositions) GameObject.Instantiate(TreasurePrefab, new Vector3(treasure.x, 0f, treasure.y), Quaternion.identity, CurrentFloor.transform);
    }
}
