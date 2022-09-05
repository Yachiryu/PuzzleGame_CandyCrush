using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int indiceX;
    public int indiceY;

    Tile inicial;
    Tile final;

    public Board board;


    public void Incializar(int x, int y)
    {
        indiceX = x;
        indiceY = y;
    }

    public void OnMouseDown()
    {
        board.SetInitialTile(this);
    }

    public void OnMouseEnter()
    {
        board.SetFinalTile(this);
    }

    public void OnMouseUp()
    {
        board.ReleaseTile();
    }

}
