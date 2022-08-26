using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public int alto;
    public int ancho;
    public int borde;
    public Tile[,] board;
    public GameObject prefTile;

    public GameObject [] prefPuntos;

    public Camera camara;

    

    private void Start()
    {
        CrearBoard();
        OrganizarCamara();
        //PiezaAleatoria();
        LlenarMatrizAleatoria();
    }

    void CrearBoard()
    {
        board = new Tile[ancho, alto];
        

        for (int i = 0; i < ancho; i++)
        {
            for (int j = 0; j < alto; j++)
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

    }

    void OrganizarCamara()
    {
        camara.transform.position = new Vector3(((float)ancho / 2) - .5f, (((float)alto / 2)) - .5f, -10);

        float aspectRatio = (float)Screen.width / (float)Screen.height;
        float sizeY = ((float)alto / 2f) + borde;
        float sizeX = (((float)ancho / 2) + borde) / aspectRatio;

        camara.orthographicSize = sizeY > sizeX ? sizeY: sizeX;

        //camara.orthographicSize = ((float)ancho / 2);
        //Screen.height;
        //Screen.width;
        //camara.aspect = ((float)ancho /(float)alto);
    }

    GameObject PiezaAleatoria()
    {
        int numeroR = Random.Range(0, prefPuntos.Length);
        GameObject go = Instantiate(prefPuntos[numeroR]);
        return go;
    }

    void PiezaPosicion(GamePiece gp , int x, int y )
    {
        gp.transform.position = new Vector3(x, y, 0f);
    }

    void LlenarMatrizAleatoria()
    {
        for (int i = 0; i < ancho; i++)
        {
            for (int j = 0; j < alto; j++)
            {
                GameObject go = PiezaAleatoria();
                PiezaPosicion(go.GetComponent<GamePiece>(),i,j);

                //GamePiece gamePiece = go.GetComponent<GamePiece>();

                
            }
        }
    }
}
