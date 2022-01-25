using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamageScript : ADamageableScript
{
    [SerializeField]
    private GameObject helmetPrefab; //Qu'est-ce qu'un casque
    private bool helmeted = true; //Est-ce qu'on a un casque actuellement ?
    private DungeonMasterScript dungeonMasterScript; //Le maitre du donjon
    private GameObject helmetedModel, nakedModel; //Les 2 modeles joueur selon si on a un casque ou pas

    /// <summary>
    /// Permet de recevoir le maitre du donjon et aussi d'initialiser nos variables
    /// </summary>
    /// <param name="dms">Le script maitre du donjon</param>
    public void ReceiveDungeonMaster(DungeonMasterScript dms)
    {
        dungeonMasterScript = dms;
        helmetedModel = transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
        nakedModel = transform.GetChild(0).GetChild(0).GetChild(1).gameObject;
        nakedModel.SetActive(false);
    }

    /// <summary>
    /// Permet de recevoir des degats et de reagir en consequence (perdre son casque ou relancer le niveau)
    /// </summary>
    public override void GetDamaged()
    {
        if (!HelmetChange(false)) dungeonMasterScript.ReloadScene();
        else GameObject.Instantiate(helmetPrefab, transform.GetChild(0).position, Quaternion.identity, transform.parent);
    }

    /// <summary>
    /// Permet de changer l'etat du casque et de savoir si il y a eu un changement
    /// </summary>
    /// <param name="h">true si on veut lui donner un casque
    ///                 false si on veut lui retirer un casque</param>
    /// <returns>true si il y a eu un changement dans l'etat du casque
    ///          false sinon</returns>
    public bool HelmetChange(bool h)
    {
        if (helmeted == h) return false;
        else
        {
            if (!helmeted)
            {
                helmeted = true;
                nakedModel.SetActive(false);
                helmetedModel.SetActive(true);
            }
            else
            {
                helmeted = false;
                helmetedModel.SetActive(false);
                nakedModel.SetActive(true);
            }
            return true;
        }
    }
}
