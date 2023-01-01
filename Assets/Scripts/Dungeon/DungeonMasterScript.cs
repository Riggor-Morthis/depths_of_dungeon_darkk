using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class DungeonMasterScript : MonoBehaviour
{
    //Privates
    private AudioManagerScript audioManager; //Pour les sons
    private ScoreModifierScript scoreScript; //Le script pour changer le score actuel
    private GameObject playerGameObject; //Pour stocker le joueur
    private Vector3 stairsPosition; //Pour savoir ou on finit le niveau
    private PlayerInputsScript playerInputScript; //Script d'inputs joueur
    private PlayerMovementScript playerMovementScript; //Script de mouvement joueur
    private PlayerDamageScript playerDamageScript; //Script des points de vie du joueur
    private CelebrationScript celebrationScript; //Le script qu'on utilise a la fin pour celebrer le niveau
    private Vector3 playerMoveInput; //La direction que le joueur veut prendre
    private FloorScript[,] solDonjon; //Les tuiles de notre donjon pour verifier les deplacements
    private int levelWidth, levelHeight; //Les dimensions du donjon
    private List<Vector2> currentTuiles, prevTuiles, nextTuiles, neighboringTuiles; //Utilisees par l'algo de distance au joueur, respectivement :
                                                                                    //les tuiles en cours de traitement, celles deja traitees, celles traitees a la prochaine iteration, celles renvoyees par la tuile en cours
    private int currentInt; //Pour stocker un entier temporaire
    private GameObject currentGameObject; //Pour stocker un gameobject
    private List<ASkeletonDecisionScript> skeletonList; //Tous les squelettes presents dans notre niveau
    private List<TreasureScript> treasureList; //Tous les tresors presents dans notre niveau
    private int skeletonActions; //Pour stocker le nombre de squelettes qui ont fini de bouger
    private bool stepSound; //Pour ne joueur qu'un seul bruit de pas pour tous les squelettes
    private bool tooManySkeleton;

    private void Start()
    {
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManagerScript>();
        scoreScript = GetComponent<ScoreModifierScript>();
        //Pour commencer, on genere un donjon
        GetComponent<DungeonBuilderScript>().DungeonBuilder();
        //On demande ensuite a la grille de gerer la distance au joueur
        PlayerDistance();
    }

    /// <summary>
    /// Permet de recevoir le gameobject joueur lorsqu'il est cree + initialise nos references aux scripts joueur
    /// </summary>
    /// <param name="p">Le gameobject du joueur</param>
    public void ReceivePlayer(GameObject p, Vector3 sp)
    {
        //On commence par le gameobject
        playerGameObject = p;
        stairsPosition = sp;
        //Ensuite le player inputscript, qu'on initialise avant de le desactiver (conflits d'inputs)
        playerInputScript = playerGameObject.GetComponent<PlayerInputsScript>();
        playerInputScript.ReceiveDungeonMaster(this);
        AllowPlayerMovement(false);
        //Le script de mouvement joueur
        playerMovementScript = playerGameObject.GetComponent<PlayerMovementScript>();
        playerMovementScript.ReceiveDungeonMaster(this);
        //Le script joueur pour savoir si il a pris des degats
        playerDamageScript = playerGameObject.GetComponent<PlayerDamageScript>();
        playerDamageScript.ReceiveDungeonMaster(this, audioManager);
        //Le script qu'on utilise a la toute fin pour celebrer une victoire
        celebrationScript = GetComponent<CelebrationScript>();
        celebrationScript.ReceiveDungeonMaster(this, playerGameObject, audioManager);
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
        foreach (ASkeletonDecisionScript skeleton in skeletonList) if(skeleton.isActiveAndEnabled) skeleton.DecisionMaking();
        
        //On peut lancer la suite de la boucle de gameplay
        AllowPlayerMovement(true);
    }

    public int GetTuileDistance(int x, int y) => solDonjon[x, y].GetDistance();

    public List<Vector2> GetTuileNeighbors(int x, int y) => solDonjon[x, y].GetVoisins();

    /// <summary>
    /// Permet de changer la couleur d'une tuile, et indique si cette tuile est dans le quadrillage ou pas
    /// </summary>
    /// <param name="x">coordonnee de la tuile</param>
    /// <param name="y">coordonnee de la tuile</param>
    /// <param name="i">true si on compte attaquer, false sinon</param>
    /// <returns>true si la tuile est dans les limites du niveau, false sinon</returns>
    public bool ChangeTuileColor(int x, int y, bool i)
    {
        if (x < 0 || x >= levelWidth) return false;
        else if (y < 0 || y >= levelHeight) return false;
        else if (solDonjon[x, y] != null) solDonjon[x, y].ChangeColor(i);
        return true;
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
        else if (CheckMovementLegality(playerGameObject.transform.position, playerMoveInput) == 2) OrderPlayerAttack();
        else
        {
            audioManager.Play("Error");
            AllowPlayerMovement(true);
        }
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
                    else foreach (ASkeletonDecisionScript skeleton in skeletonList) if(skeleton.isActiveAndEnabled) if (Vector3.Distance(positionInitiale, skeleton.transform.position) < 0.1f) return 2;
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
        audioManager.Play("Step");
        playerMovementScript.ReceiveMoveInstruction(playerMoveInput);
    }

    /// <summary>
    /// On ordonne au script de mouvement de gerer l'attaque du joueur vers un squelette
    /// </summary>
    private void OrderPlayerAttack()
    {
        playerMovementScript.ReceiveAttackInstruction(playerMoveInput);
    }

    /// <summary>
    /// Le joueur a fini son mouvement et on peut donc passer a la suite, que ce soit une attaque ou pas
    /// </summary>
    public void PlayerHasMoved()
    {
        //On commence par s'assurer qu'on a pas encore fini le niveau
        EndLevelCheck();
        //Si on a une possibilite d'attaquer, il faut le faire
        if (CheckMovementLegality(playerGameObject.transform.position, playerMoveInput) == 2) OrderPlayerAttack();
        //Sinon on peut lancer la suite de la boucle de gameplay
        else TuilesReset();
    }

    /// <summary>
    /// Si on est a la sortie, on lance la celebration de fin de niveau
    /// </summary>
    private void EndLevelCheck()
    {
        if (Vector3.Distance(playerGameObject.transform.position, stairsPosition) < 0.1f)
        {
            audioManager.Play("Triumph");
            DungeonPlannerScript.LevelComplete();
            playerDamageScript.Celebration();
            celebrationScript.Celebration();
            scoreScript.Reset();
        }
    }

    /// <summary>
    /// Le joueur a fini son attaque et on peut donc passer a la suite
    /// </summary>
    public void PlayerHasAttacked()
    {
        //On peut lancer la suite de la boucle de gameplay
        TuilesReset();
    }

    /// <summary>
    /// Permet de demander au maitre de jeu de faire une attaque a un endroit donne
    /// </summary>
    /// <param name="tuileCible">Quelle est la position qu'on vient d'essayer d'attaquer</param>
    public void AttackTentative(Vector3 tuileCible, bool melee)
    {
        if (melee) audioManager.Play("Melee");

        if (Vector3.Distance(tuileCible, playerGameObject.transform.position) < 0.1f)
        {
            //On endommage le joueur, il faut donc le signaler a son script et ajuster le score en consequence
            playerGameObject.GetComponent<ADamageableScript>().GetDamaged();
            scoreScript.ChangeScore(Random.Range(-500, 0));
        }
        else foreach (ASkeletonDecisionScript skeleton in skeletonList) if (skeleton.isActiveAndEnabled)
                    if (Vector3.Distance(tuileCible, skeleton.transform.position) < 0.1f)
                    {
                        //On a endommage un squelette, il faut donc le signaler a son script et changer notre score
                        audioManager.Play("EnemyDeath");
                        skeleton.gameObject.GetComponent<ADamageableScript>().GetDamaged();
                        if (skeleton.GetComponent<MeleeDecisionScript>() != null) scoreScript.ChangeScore(Random.Range(1000, 2000));
                        else scoreScript.ChangeScore(Random.Range(3000, 4000));
                    }
    }

    /// <summary>
    /// Remet "a 0" les tuiles de notre grille pour effacer notre UI
    /// </summary>
    private void TuilesReset()
    {
        for (int i = 0; i < levelWidth; i++) for (int j = 0; j < levelHeight; j++) if (solDonjon[i, j] != null) solDonjon[i, j].ResetColor();

        //On peut lancer la suite de la boucle de gameplay
        CollectMoney();
    }

    /// <summary>
    /// Demande a chacun des tresors dans la scene de savoir si le joueur l'a ramasse ou non
    /// </summary>
    private void CollectMoney()
    {
        //On inspecte chaque tresor
        foreach (TreasureScript treasure in treasureList)
        {
            if (treasure.gameObject.activeInHierarchy) if (Vector3.Distance(treasure.transform.position, playerGameObject.transform.position) < 0.1f)
                    if (treasure.isActiveAndEnabled)
                    {
                        audioManager.Play("Coins");
                        treasure.TreasureCollected();
                        //On oublie pas de rendre son casque au joueur, et d'ajuster le score en consequence
                        if (playerDamageScript.HelmetChange(true)) scoreScript.ChangeScore(Random.Range(2000, 3000));
                        else scoreScript.ChangeScore(Random.Range(4000, 5000));
                    }
        }

        //On peut lancer la suite de la boucle de gameplay
        SkeletonsActions();
    }

    /// <summary>
    /// Permet de demander a chaque squelette de calculer les consequences de ses intentions, puis de lancer les animations
    /// </summary>
    private void SkeletonsActions()
    {
        //On commence par nettoyer notre liste de squelettes
        skeletonActions = 0;
        for(int i = skeletonList.Count -1; i >= 0; i--) if (!skeletonList[i].isActiveAndEnabled)
            {
                currentGameObject = skeletonList[i].gameObject;
                skeletonList.RemoveAt(i);
                GameObject.Destroy(currentGameObject);
            }

        //Si il ne reste plus de squelettes dans notre donjon, on peut enclencher la suite
        if (skeletonList.Count == 0) PlayerDistance();

        //On va interroger en priorite les squelettes qui veulent attaquer
        foreach (ASkeletonDecisionScript skeleton in skeletonList) if (skeleton.GetIntentionAttaque())
            {
                skeleton.AttackProjection();
            }
        //On peut ensuite interroger le reste des squelettes pour savoir ce qu'ils vont faire
        foreach(ASkeletonDecisionScript skeleton in skeletonList) if (!skeleton.GetIntentionAttaque())
            {
                skeleton.MovementProjection();
            }

        //Maintenant que tous le monde sait ce qu'il peut faire, ils vont tous agir en consequence
        stepSound = true;
        tooManySkeleton = true;
        foreach (ASkeletonDecisionScript skeleton in skeletonList)
        {
            if (!skeleton.GetIntentionAttaque() && !skeleton.GetDeathAnimation() && stepSound)
            {
                stepSound = false;
                audioManager.Play("Step");
            }
            skeleton.LaunchAnimation();
        }
    }

    /// <summary>
    /// Lorsque les squelettes projettent leur attaque, cette fonction permet de verifier s'ils touchent un autre squelette, et de prevenir ce dernier le cas echeant
    /// </summary>
    /// <param name="tuileCible">L'endroit ou on veut taper</param>
    /// <returns>true si on a un squelette sous la tuile ciblee ou si on en dehors de la grille
    ///          false sinon</returns>
    public bool CheckSkeletonTarget(Vector3 tuileCible)
    {
        //On verifie que la tuile est dans la grille
        if (tuileCible.x < 0 || tuileCible.x >= levelWidth) return true;
        else if (tuileCible.z < 0 || tuileCible.z >= levelHeight) return true;

        //Sinon, si on touche bien un squelette on va lui signaler
        foreach (ASkeletonDecisionScript skeleton in skeletonList) if (skeleton.isActiveAndEnabled) if (Vector3.Distance(tuileCible, skeleton.transform.position) < 0.1f)
                {
                    skeleton.YouWillDie();
                    return true;
                }
        return false;
    }

    /// <summary>
    /// Necessaire pour arreter les tirs des squelettes distance au premier joueur trouve
    /// </summary>
    /// <param name="tuileCible">La ou on veut tirer</param>
    /// <returns>true si il y a un joueur, false sinon</returns>
    public bool CheckSkeletonRangedTarget(Vector3 tuileCible)
    {
        if (Vector3.Distance(playerGameObject.transform.position, tuileCible) < 0.1f) return true;
        return false;
    }

    /// <summary>
    /// Fonctionne similairement a la legalite des mouvements, sauf que :
    /// on sait deja que le mouvement est legal, donc on check pas la grille
    /// on ne veut pas rentrer dans le joueur, donc on check sa position actuelle
    /// on ne veut pas rentrer dans un autre squelette lors de notre mouvement, donc on compare notre position cible avec des autres squelettes qui vont bouger, plutot qu'avec leurs positions actuelles
    /// </summary>
    /// <param name="tuile">L'endroit ou on veut aller</param>
    /// <returns>true si le mouvement va etre possible
    ///          false sinon</returns>
    public bool CheckSkeletonMovement(Vector3 tuileCible, ASkeletonDecisionScript ourself)
    {
        if (Vector3.Distance(tuileCible, playerGameObject.transform.position) < 0.1f) return false;
        //On cherche les squelettes active qui veulent a la fois se deplacer et qui ne vont pas mourir
        else foreach (ASkeletonDecisionScript skeleton in skeletonList) if (skeleton.isActiveAndEnabled) if (!skeleton.GetDeathAnimation())
                        //Si le squelette compte se deplacer, on check la position qu'il VA occuper
                        if (!skeleton.GetIntentionAttaque())
                        {
                            //On oublie pas de s'assurer qu'on s'inspecte pas nous meme
                            if (Vector3.Distance(tuileCible, skeleton.GetTarget()) < 0.1f && skeleton != ourself) return false;
                        }
                        //Sinon, on check la position qu'il occupe ACTUELLEMENT
                        else
                        {
                            //On oublie pas de s'assurer qu'on s'inspecte pas nous meme
                            if (Vector3.Distance(tuileCible, skeleton.transform.position) < 0.1f && skeleton != ourself) return false;
                        }
        return true;
    }

    /// <summary>
    /// Utilise pour tenir compte du nombre de squelettes qui ont agis, et le nombre de squelettes qui doivent encore agir
    /// </summary>
    public void SkeletonHasActed()
    {
        //On compte le nombre de squelettes qui ont agis
        skeletonActions++;
        //On s'assure que le nombre de squelettes est a jour
        for (int i = skeletonList.Count - 1; i >= 0; i--) if (!skeletonList[i].isActiveAndEnabled)
            {
                currentGameObject = skeletonList[i].gameObject;
                skeletonList.RemoveAt(i);
                GameObject.Destroy(currentGameObject);
            }
        //Si tous les squelettes ont agis, on engage la suite de la boucle de gameplay
        if (skeletonActions >= skeletonList.Count && tooManySkeleton)
        {
            tooManySkeleton = false;
            StartCoroutine(Pause());
        }
    }

    /// <summary>
    /// Pour attendre que tous les squelettes aient finis
    /// </summary>
    /// <returns>Une courte pause</returns>
    IEnumerator Pause()
    {
        yield return new WaitForSeconds(0.1f);
        PlayerDistance();
    }

    /// <summary>
    /// Permet de recharger le niveau actuel lorsque le joueur meurt
    /// </summary>
    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public int GetScore() => scoreScript.GetScore();

    public Vector3 GetPlayerPosition() => playerGameObject.transform.position;
}
