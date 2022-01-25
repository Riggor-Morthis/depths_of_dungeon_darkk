using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonPlannerScript : MonoBehaviour
{
    //Private statics
    private static bool reload = false; //Est-ce qu'on recharge le meme niveau ou est-ce que c'est en un autre ?
    private static int levelWidth, levelHeight; //Taille du niveau
    private static Vector2 playerPosition, stairsPosition; //Positions initialies du joueur et du point de sortie
    private static List<Vector2> holesPositions, enemyPositions, treasuresPositions; //Index des tuiles avec un trou, un ennemi ou un tresor
    private static int budget; //Utilise pour savoir combien de certains elements on peut placer
    private static Vector2 currentVector; //Utilise pour stocker des vecteurs
    private static float currentFloat; //Stocker des distances notamment
    private static Vector2 lastHole; //Utilise pour stocker des positions lors de la creation de trou
    private static int currentDealBreaker; //Combien d'iterations de placage de trou on a deja essayer
    private static readonly int maxDealBreaker = 4; //Combien d'iterations on peut faire avant d'abandonner

    /// <summary>
    /// Prevois le set-up du donjon (pas la fonction qui le construit)
    /// </summary>
    public static void DungeonPlanner()
    {
        if (!reload)
        {
            reload = true;
            BasicGenerator();
            HolesGenerator();
            ProblemSolver();
            EnemiesGenerator();
            TreasuresGenerator();
        }
    }

    public static void LevelComplete()
    {
        reload = false;
    }

    /// <summary>
    /// Genere les elements essentiels pour notre donjon : des coordonnees, un joueur et une sortie
    /// </summary>
    private static void BasicGenerator()
    {
        //On commence par determiner les dimensions du niveau
        levelHeight = 9 + (Random.Range(0, 5) * 2);
        levelWidth = 22 - levelHeight;
        //On joue en 9:16
        if(levelWidth > levelHeight)
        {
            currentFloat = levelHeight;
            levelHeight = levelWidth;
            levelWidth = (int)currentFloat;
        }

        //Ensuite on place le joueur et la sortie en oppose
        playerPosition = new Vector2(Random.Range(0, levelWidth), 0);
        stairsPosition = new Vector2(levelWidth - playerPosition.x - 1, levelHeight - 1);
    }

    /// <summary>
    /// Permet de generer des groupes de trous dans notre niveau
    /// </summary>
    private static void HolesGenerator()
    {
        //Le nombre de trous qu'on va placer dans notre niveau
        budget = (int)(Random.Range(0.2f, 0.4f) * (levelHeight * levelWidth));
        //Initialisation de variable
        holesPositions = new List<Vector2>();

        //Tant qu'on a du budget, on va placer des trous
        do
        {
            //On commence par trouver des coordonnees au hasard
            lastHole = new Vector2(Random.Range(0, levelWidth), Random.Range(0, levelHeight));
            currentDealBreaker = 0;

            //On continue d'essayer de placer un nouveau trou en tant que voisin, tant qu'on prend pas trop de temps
            do
            {
                //On trouve des nouvelles coordonnees dans le voisinnage (echiquier) de notre dernier trou
                currentVector.x = Mathf.Clamp(lastHole.x + Random.Range(-1, 2), 0, levelWidth - 1);
                currentVector.y = Mathf.Clamp(lastHole.y + Random.Range(-1, 2), 0, levelHeight - 1);

                //Si on a pas deja place ce trou : tout va bien, on peut passer a la suite
                if (!holesPositions.Contains(currentVector))
                {
                    holesPositions.Add(currentVector);
                    lastHole = currentVector;
                    currentDealBreaker = 0;
                }
                //Si on a deja place ce trou, on ajoute un au compteur et on recommence le tirage au sort
                else currentDealBreaker++;

            } while (currentDealBreaker < maxDealBreaker && holesPositions.Count < budget);

        } while (holesPositions.Count < budget);
    }

    /// <summary>
    /// S'assure que le niveau est finissable, de maniere tres brutale (on fait des chemins en L entre la sortie et le joueur)
    /// </summary>
    private static void ProblemSolver()
    {
        //On commence par nettoyer les chemins horizontaux
        for(int i = Mathf.Min((int)playerPosition.x, (int)stairsPosition.x); i <= Mathf.Max((int)playerPosition.x, (int)stairsPosition.x); i++)
        {
            currentVector = new Vector2(i, 0);
            if (holesPositions.Contains(currentVector)) holesPositions.Remove(currentVector);
            currentVector = new Vector2(i, levelHeight - 1);
            if (holesPositions.Contains(currentVector)) holesPositions.Remove(currentVector);
        }
        
        //Ensuite les chemins verticaux
        for(int i = 0; i < levelHeight; i++)
        {
            currentVector = new Vector2(playerPosition.x, i);
            if (holesPositions.Contains(currentVector)) holesPositions.Remove(currentVector);
            currentVector = new Vector2(stairsPosition.x, i);
            if (holesPositions.Contains(currentVector)) holesPositions.Remove(currentVector);
        }
    }

    /// <summary>
    /// Place des ennemis aleatoirement dans notre niveau
    /// </summary>
    private static void EnemiesGenerator()
    {
        //Combien d'ennemis on va avoir dans notre niveau
        budget = (int)(Random.Range(0.03f, 0.06f) * (levelHeight * levelWidth)) + 1;
        //Initialisation de variables
        enemyPositions = new List<Vector2>();

        for(int i = 0; i < budget; i++)
        {
            //On invente des coordonnees jusqu'a trouver l'endroit adapte
            do
            {
                //On commence par trouver une position qui n'est ni le depart, ni l'arrivee, ni un trou
                do
                {
                    currentVector = new Vector2(Random.Range(0, levelWidth), Random.Range(0, levelHeight));
                } while (currentVector == playerPosition || currentVector == stairsPosition || holesPositions.Contains(currentVector));

                //On veut que cet ennemi soit a une distance minimum du joueur
                currentFloat = Vector2.Distance(currentVector, playerPosition);
                //On veut aussi qu'il soit a une distance minimum de ses potes
                foreach (Vector2 enemy in enemyPositions)
                {
                    //Notez le +1 parce que les distance minimale monstre/monstre est plus petite que la distance minimale monstre/joueur
                    if (currentFloat > Vector2.Distance(currentVector, enemy) + 1) currentFloat = Vector2.Distance(currentVector, enemy) + 1;
                }

            } while (currentFloat < 3);

            //On peut ajouter cette position valide a notre liste
            enemyPositions.Add(currentVector);
        }
    }

    /// <summary>
    /// Place des tresors aleatoirement dans notre niveau
    /// </summary>
    private static void TreasuresGenerator()
    {
        //Combien de tresors on veut dans notre donjon
        budget = (int)(Random.Range(0.02f, 0.05f) * (levelHeight * levelWidth));
        //Initialisation de variables
        treasuresPositions = new List<Vector2>();

        for (int i = 0; i < budget; i++)
        {
            //On invente des coordonnees jusqu'a trouver l'endroit adapte
            do
            {
                //On commence par trouver une position qui n'est ni le depart, ni l'arrivee, ni un trou ni un ennemi
                do
                {
                    currentVector = new Vector2(Random.Range(0, levelWidth), Random.Range(0, levelHeight));
                } while (currentVector == playerPosition || currentVector == stairsPosition || holesPositions.Contains(currentVector) || enemyPositions.Contains(currentVector));

                //On veut que ce tresor soit a une distance minimum du joueur
                currentFloat = Vector2.Distance(currentVector, playerPosition);
                // On veut aussi qu'il soit a une distance minimum des autres tresors
                foreach (Vector2 treasure in treasuresPositions) if (currentFloat < Vector2.Distance(currentVector, treasure)) currentFloat = Vector2.Distance(currentVector, treasure);
            } while (currentFloat < 2);

            //On peut ajouter cette position valide a notre liste
            treasuresPositions.Add(currentVector);
        }
    }

    public static int getLevelWidth() => levelWidth;
    public static int getLevelHeight() => levelHeight;
    public static Vector2 getPlayerPosition() => playerPosition;
    public static Vector2 getStairsPosition() => stairsPosition;
    public static List<Vector2> getHolesPositions() => holesPositions;
    public static List<Vector2> getEnemyPositions() => enemyPositions;
    public static List<Vector2> getTreasuresPositions() => treasuresPositions;
}
