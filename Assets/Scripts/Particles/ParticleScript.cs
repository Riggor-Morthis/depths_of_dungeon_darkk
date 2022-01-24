using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleScript : MonoBehaviour
{
    //Privates
    private float moveX, moveY = 5, moveZ; //Les mouvements que notre particle va faire
    private float gravity = 9.81f;
    private float rotationY; //La rotation que notre particle va adopter
    private GameObject model; //Un gameobject intermedaire necessaire pour la rotation

    public void Awake()
    {
        //Initilisation de variables aleatoires
        moveX = Random.Range(-1f, 1f);
        moveZ = Random.Range(-1f, 1f);
        rotationY = Random.Range(-360f, 360f);
        model = transform.GetChild(0).gameObject;
        //On fait deja un espacement avec l'entite histoire de
        transform.position += new Vector3(moveX * 0.5f, 0, moveZ * 0.5f);
    }

    private void Update()
    {
        //Si on est sous le sol est temps de disparaitre
        if (transform.position.y <= -0.5) GameObject.Destroy(gameObject);
        //Sinon on peut juste se mettre a jour
        moveY -= gravity * Time.deltaTime;
        transform.position += new Vector3(moveX, moveY, moveZ) * Time.deltaTime;
        model.transform.Rotate(Vector3.up, rotationY * Time.deltaTime);
    }
}
