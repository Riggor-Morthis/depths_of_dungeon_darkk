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

    /// <summary>
    /// Lorsqu'on est frappe, on disparait dans une fontaine d'ossements et on arrete d'agir
    /// </summary>
    public override void GetDamaged()
    {
        for (int i = 0; i < howManyBones; i++) GameObject.Instantiate(bonePrefab, transform.GetChild(0).position, Quaternion.identity, transform.parent);
        gameObject.SetActive(false);
    }
}
