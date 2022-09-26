using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundsManager : MonoBehaviour
{
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
