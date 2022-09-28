using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Contador : MonoBehaviour
{
    public int minutos;
    public int segundos;
    public TMP_Text tiempo;
    private float restantes;
    public bool enMarcha;

    public void Awake() // Calculamos el tiempo restante 
    {
        restantes = (minutos * 60) + segundos; 
    }

    public void Update() // Creamos la condicion de perdida por tiempo y calculamos el tiempo 
    {
        if (enMarcha)
        {
            restantes -= Time.deltaTime;

            if (restantes <= 0) // 1 Creamos la condicion de perdida por tiempo
            {
                //SceneManager.LoadScene("Game_Over");
                Debug.Log("Perdiste");
            }

            int tempMin = Mathf.FloorToInt(restantes / 60);
            int tempSegundos = Mathf.FloorToInt(restantes % 60);

            tiempo.text = string.Format("{00:00} : {01:00}", tempMin, tempSegundos); // convertimos el texto a strings y se pueda mostrar en el juego
        }
    }


}
