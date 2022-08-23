using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int indiceX;
    public int indiceY;

    public void Incializar(int x, int y)
    {
        indiceX = x;
        indiceY = y;
    }
}
