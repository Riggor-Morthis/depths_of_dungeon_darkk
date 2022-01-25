using UnityEngine;
using TMPro;
public class ScoreModifierScript : MonoBehaviour
{
    //Privates
    [SerializeField]
    private TMP_Text scoreNumber; //Le champ de texte qu'on modifie dans l'UI
    private float currentScore, scoreSpeed; //Notre score actuel, et la vitesse a laquelle il change
    private int targetScore; //Le score qu'on veut atteindre

    private void Awake()
    {
        UpdateUI();
    }

    /// <summary>
    /// On veut ajouter un certain nombre a notre score et laisser le script gerer ce changement de maniere esthetique
    /// </summary>
    /// <param name="change"></param>
    public void ChangeScore(int change)
    {
        //On commence par se servir de ce changement pour savoir quel score on veut atteindre
        if (targetScore + change < 0) targetScore = 0;
        else targetScore += change;

        //En fonction du score a atteindre et du score actuel, on peut determiner la vitesse de changement de score
        scoreSpeed = (targetScore - currentScore)*4;
    }

    /// <summary>
    /// Revois le score a zero
    /// </summary>
    public void Reset()
    {
        targetScore = 0;
        scoreSpeed = -currentScore * 0.5f;
    }

    private void Update()
    {
        //Pas besoin de faire d'update si on a deja le bon score
        if(targetScore != currentScore)
        {
            //Si on est assez proche du score vise, on s'y arrete de force
            if (Mathf.Abs(targetScore - currentScore) <= Mathf.Abs(scoreSpeed * Time.deltaTime)) currentScore = targetScore;
            //Sinon, on change notre score petit a petit
            else currentScore += scoreSpeed * Time.deltaTime;

            //On oublie pas de mettre l'UI a jour
            UpdateUI();
        }
    }

    /// <summary>
    /// Donne la bonne valeur a l'UI
    /// </summary>
    private void UpdateUI(){
        scoreNumber.text = ((int)currentScore).ToString();
    }

    public int GetScore() => targetScore;
}
