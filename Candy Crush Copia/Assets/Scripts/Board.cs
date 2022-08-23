using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public int alto;
    public int ancho;
    public Tile[,] board;
    public GameObject prefTile;

    public Camera camara;

    private void Start()
    {
        CrearBoard();
        
    }

    void CrearBoard()
    {
        board = new Tile[alto, ancho];

        for (int i = 0; i < alto; i++)
        {
            for (int j = 0; j < ancho; j++)
            {
                GameObject go = Instantiate(prefTile);
                go.name = "Tile(" + i + ", " + j + ")";
                go.transform.position = new Vector3(i, j, 0);
                go.transform.parent = transform;

                Tile tile = go.GetComponent<Tile>();
                board[i, j] = tile;
                tile.Incializar(i,j);
            }
        }
        camara.transform.position = new Vector3(alto, ancho,0);
    }

}
