using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int xIndex;
    public int yIndex;

    Board m_board; // Referencia del script del Board 

    public void Init(int x, int y, Board board) // Inicializamos los tiles
    {
        xIndex = x;
        yIndex = y;
        m_board = board;
    }


    public void OnMouseEnter() // Sabemos si el mouse entro al area para arrastrar y en que posicion
    {
        m_board.DragToTile(this);
    }
    public void OnMouseDown() // Damos click
    {
        m_board.ClickedTile(this);
    }

    public void OnMouseUp() // Soltamos el click
    {
        m_board.ReleaseTile();
    }

}
