using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // Todo esto es para crear el sonido para saber cuando activarlo en los botones 

    public AudioSource audioSource;
    public AudioClip audioClip;


    private void Start()
    {
        audioSource.clip = audioClip;
    }

    public void Reproducir()
    {
        audioSource.Play();
    }
}
