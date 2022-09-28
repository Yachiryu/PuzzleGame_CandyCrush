using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Puntaje : MonoBehaviour
{
    private int puntos;
    private int multiplicador;
    private int puntajeAlmacenado;
    private TextMeshProUGUI textMesh;

    public int puntajeNecesario;

    private void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        textMesh.text = puntos.ToString("Score : 0");
    }

    public void SumatoriaPuntos(int puntosEntrada) // Sumamos los puntos entrantes y creamos las condiciones de victorias 
    {
        puntos += puntosEntrada;

        if (puntos >= puntajeNecesario)
        {
            SceneManager.LoadScene("You_Win");
        }
    }


}
