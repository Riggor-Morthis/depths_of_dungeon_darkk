using System;
using UnityEngine.Audio;
using UnityEngine;

public class AudioManagerScript : MonoBehaviour
{
    //Public
    [SerializeField]
    public Sound[] sounds; //La liste de tous les sons disponibles dans notre jeu
    public static AudioManagerScript instance; //Pour s'assurer qu'on a qu'une seule instance de ce manager

    //Private
    Sound currentSound; //Le son actuel

    /// <summary>
    /// Utilise au tout debut pour bien creer la liste comme il faut
    /// </summary>
    private void Awake()
    {
        if (instance == null) instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        //On cree un audio source pour chaque son dans la base de son
        foreach (Sound sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume;
        }

        //Enfin, on lance la musique du jeu (en repetition)
        currentSound = Array.Find(sounds, sound => sound.name == "Music");
        currentSound.source.loop = true;
        currentSound.source.Play();
    }

    /// <summary>
    /// Ordonne a un clip de jouer
    /// </summary>
    /// <param name="name">Le nom du clip qu'on veut jouer</param>
    public void Play(string name)
    {
        currentSound = Array.Find(sounds, sound => sound.name == name);
        currentSound.source.PlayOneShot(currentSound.clip);
    }
}
