 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int indiceX; // xindex
    public int indiceY; // yIndex

    Board m_board; // m_board


    public void Incializar(int x, int y, Board board) // Init
    {
        indiceX = x;
        indiceY = y;
        m_board = board;
    }


    public void OnMouseEnter()
    {
        m_board.SetFinalTile(this); // DragToTile
    }
    public void OnMouseDown()
    {
        m_board.InitialTile(this); // ClickedTile
    }

    public void OnMouseUp()
    {
        m_board.ReleaseTile();
    }

}
