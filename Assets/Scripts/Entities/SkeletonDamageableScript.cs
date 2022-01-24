using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonDamageableScript : ADamageableScript
{
    //Privates
    [SerializeField]
    private GameObject bonePrefab; //Qu'est ce qu'un os
    private int howManyBones; //Combien d'os on utilise

    private void Awake()
    {
        howManyBones = Random.Range(4, 10);
    }

    public override void GetDamaged()
    {
        for (int i = 0; i < howManyBones; i++) GameObject.Instantiate(bonePrefab, transform.position, Quaternion.identity, transform.parent);
        gameObject.SetActive(false);
    }
}
