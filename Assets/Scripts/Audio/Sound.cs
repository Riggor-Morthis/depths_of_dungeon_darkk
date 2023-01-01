using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name; //Le nom de ce clip, pour le retrouver
    public AudioClip clip; //Le clip a jouer
    [Range(0f, 1f)]
    public float volume; //Le volume du clip a jouer
    [HideInInspector]
    public AudioSource source; //L'audiosource qui jouera ce clip
}
