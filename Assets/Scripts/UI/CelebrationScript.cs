using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelebrationScript : MonoBehaviour
{
    [SerializeField]
    private GameObject coinPrefab;
    private DungeonMasterScript dungeonMasterSript;
    private GameObject playerGameObject;
    private int currentCoins, totalCoins; //Combien de pieces on veut faire apparaitre, combien de pieces sont apparus
    private float realCoins, coinSpeed; //Combien de pieces sont reellements apparues, et a quelle vitesse on doit les faire apparaitre
    private bool isCelebrating = false; //Est-ce qu'on est en train de celebrer la fin d'un niveau

    public void ReceiveDungeonMaster(DungeonMasterScript dms, GameObject pgo)
    {
        dungeonMasterSript = dms;
        playerGameObject = pgo;
    }

    /// <summary>
    /// Initialise nos variables et lance la celebration
    /// </summary>
    public void Celebration()
    {
        totalCoins = dungeonMasterSript.GetScore() / 1000;
        coinSpeed = totalCoins * 2;
        currentCoins = 0;
        realCoins = 0;
        isCelebrating = true;
    }

    private void Update()
    {
        if (isCelebrating)
        {
            //Si on a lance le bon nombre de pieces, on fait une pause puis on passe au niveau suivant
            if(currentCoins == totalCoins)
            {
                StartCoroutine(Pause());
            }
            //Sinon, on lance des pieces petit a petit
            else
            {
                realCoins += coinSpeed * Time.deltaTime;
                //Le vrai nombre de pieces est au dessus du faux nombre de pieces, on en lance une nouvelle
                if(realCoins > currentCoins)
                {
                    GameObject.Instantiate(coinPrefab, playerGameObject.transform.position, Quaternion.identity, transform);
                    currentCoins++;
                }
            }
        }
    }

    /// <summary>
    /// Pour faire une pause d'une seconde avant de continuer
    /// </summary>
    /// <returns>Une pause d'une seconde</returns>
    IEnumerator Pause()
    {
        yield return new WaitForSeconds(1.5f);
        dungeonMasterSript.ReloadScene();
    }
}
