using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Puntaje : MonoBehaviour
{
    private int puntos;
    private int multiplicador;
    private int puntajeAlmacenado;
    private TextMeshProUGUI textMesh;

    private void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        textMesh.text = puntos.ToString("Score : 0");
    }

    public void SumatoriaPuntos(int puntosEntrada)
    {
        puntos += puntosEntrada;
    }
}
