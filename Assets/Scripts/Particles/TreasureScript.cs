using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureScript : MonoBehaviour
{
    //Privates
    [SerializeField]
    private GameObject coinPrefab; //Qu'est-ce qu'une piece
    private int coinsInTreasure; //Combien de pieces on va lacher

    private void Awake()
    {
        coinsInTreasure = Random.Range(4, 10);
    }

    public void TreasureCollected()
    {
        for(int i = 0; i < coinsInTreasure; i++) GameObject.Instantiate(coinPrefab, transform.position, Quaternion.identity, transform.parent);
        gameObject.SetActive(false);
    }
}
