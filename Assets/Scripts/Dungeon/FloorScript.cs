using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorScript : MonoBehaviour
{
    //Privates
    Color color, UIcolor; //Couleur de base de la tuile, couleur de la tuile pour afficher des informations (temporaires)
    MeshRenderer meshRenderer;
    bool altered; //Est-ce que la couleur de la tuile a ete alteree
    List<Vector2> voisins; //La liste des voisins de la tuile
    int distance = 0; //Instance actuelle au joueur (manhattan)

    /// <summary>
    /// Initialise les couleurs de notre tuile
    /// </summary>
    /// <param name="pairImpair">Pour la quinconce de nos couleurs</param>
    /// <param name="endTile">Est-ce qu'on sur la tuile de fin ou pas</param>
    public void Initialize(int pairImpair, bool endTile)
    {
        //Initialisation de variable
        altered = false;
        voisins = new List<Vector2>();
        //On recupere le mesh renderer et on lui file la bonne couleur
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (endTile) color = new Color(161f / 255, 188f / 255, 201f / 255);
        else if (pairImpair == 0) color = new Color(173f / 255, 199f / 255, 204f / 255);
        else color = new Color(145f / 255, 170f / 255, 194f / 255);
        ChangeColor(color);
    }

    /// <summary>
    /// Permet de changer la couleur actuelle de la tuile
    /// </summary>
    /// <param name="col">Le parametre a donne pour faire la modification</param>
    private void ChangeColor(Color col)
    {
        meshRenderer.material.color = col;
    }

    /// <summary>
    /// Pour changer la tuile afin d'afficher les intentions des ennemis
    /// </summary>
    /// <param name="intentionAttaque">Est-ce qu'on veut attaquer (true) ou pas ?</param>
    public void ChangeColor(bool intentionAttaque)
    {
        //Si on a pas encore ete modifie, on prend une couleur faite pour
        if (!altered)
        {
            //Selon les intentions, on aura pas la meme apparence
            if (intentionAttaque) UIcolor = new Color(217f / 255, 108f / 255, 126f / 255);
            else UIcolor = new Color(108f / 255, 217f / 255, 126f / 255);

            altered = true;
        }
        //Si on a ete modifie, on ajuste simplement la teinte actuelle pour mieux coller a cette consigne
        else
        {
            //On devient de plus en plus fonce selon ce qu'on veut y faire
            UIcolor.b = Mathf.Max(0f, UIcolor.b - 0.1f);
            if(intentionAttaque) UIcolor.g = Mathf.Max(0f, UIcolor.g - 0.1f);
            else UIcolor.r = Mathf.Max(0f, UIcolor.r - 0.1f);
        }

        //Une fois qu'on a trouve la couleur, on l'applique
        ChangeColor(UIcolor);
    }

    /// <summary>
    /// Renvoie la tuile sur sa couleur d'origine
    /// </summary>
    public void ResetColor()
    {
        ChangeColor(color);
        altered = false;
    }

    /// <summary>
    /// Permet d'ajouter un voisin dans la liste
    /// </summary>
    /// <param name="v">Le script voisin de celui-ci</param>
    public void AddNeighbor(Vector2 v)
    {
        voisins.Add(v);
    }

    /// <summary>
    /// Recoit la distance au joueur, et donne la liste de ses voisins en echange
    /// </summary>
    /// <param name="d">La distance au joueur</param>
    /// <returns>La liste des voisins de la tuile</returns>
    public List<Vector2> SetDistance(int d)
    {
        distance = d;
        return voisins;
    }

    public int GetDistance() => distance;

    public List<Vector2> GetVoisins() => voisins;
}
