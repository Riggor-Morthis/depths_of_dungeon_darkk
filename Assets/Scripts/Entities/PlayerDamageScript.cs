using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamageScript : ADamageableScript
{
    [SerializeField]
    private GameObject helmetPrefab; //Qu'est-ce qu'un casque
    private AudioManagerScript audioManager; //Pour jouer les sons
    private bool helmeted = true; //Est-ce qu'on a un casque actuellement ?
    private DungeonMasterScript dungeonMasterScript; //Le maitre du donjon
    private GameObject helmetedModel, nakedModel; //Les 2 modeles joueur selon si on a un casque ou pas
    private GameObject helmetedCelebration, nakedCelebration; //Comme c'est deja ici qu'on gere les modeles, c'est ici qu'on va activer les modeles de celebration

    /// <summary>
    /// Permet de recevoir le maitre du donjon et aussi d'initialiser nos variables
    /// </summary>
    /// <param name="dms">Le script maitre du donjon</param>
    public void ReceiveDungeonMaster(DungeonMasterScript dms, AudioManagerScript ams)
    {
        dungeonMasterScript = dms;
        audioManager = ams;

        helmetedModel = transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
        nakedModel = transform.GetChild(0).GetChild(0).GetChild(1).gameObject;
        nakedModel.SetActive(false);
        helmetedCelebration = transform.GetChild(0).GetChild(0).GetChild(2).gameObject;
        helmetedCelebration.SetActive(false);
        nakedCelebration = transform.GetChild(0).GetChild(0).GetChild(3).gameObject;
        nakedCelebration.SetActive(false);
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
        if (helmeted == h)
        {
            if (!helmeted) audioManager.Play("PlayerHurt");
            return false;
        }
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
                audioManager.Play("Helmet");
                helmeted = false;
                helmetedModel.SetActive(false);
                nakedModel.SetActive(true);
            }
            return true;
        }
    }

    /// <summary>
    /// Permet de changer les modeles pour charger ceux de celebration
    /// </summary>
    public void Celebration()
    {
        if (helmeted)
        {
            helmetedModel.SetActive(false);
            helmetedCelebration.SetActive(true);
        }
        else
        {
            nakedModel.SetActive(false);
            nakedCelebration.SetActive(true);
        }
    }
}
