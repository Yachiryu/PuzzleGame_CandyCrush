using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public int alto;
    public int ancho;
    public int borde;

    public Tile[,] board;
    public GamePiece[,] gamePiece; 
    public GameObject prefTile;

    public GameObject [] prefPuntos;
    public Camera camara;

    public Tile inicial;
    public Tile final;

    


    private void Start()
    {
        CrearBoard();
        OrganizarCamara();
        LlenarMatrizAleatoria();
    }

    Vector3 startPos;
    Vector3 endPos;
    [Range(0f, 1f)] float time;
    
    void Update()
    {
        transform.position = Vector3.Lerp(startPos, endPos, time); ;
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
                tile.board = this;
                tile.Incializar(i,j);
                board[i, j] = tile;
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
    }

    GameObject PiezaAleatoria()
    {
        int numeroR = Random.Range(0, prefPuntos.Length);
        GameObject go = Instantiate(prefPuntos[numeroR]);
        go.GetComponent<GamePiece>().board = this;
        return go;
    }

    public void PiezaPosicion(GamePiece gp , int x, int y )
    {
        gp.transform.position = new Vector3(x, y, 0f);
        gp.Coordenadas(x, y);
        gamePiece[x, y] = gp;
    }

    void LlenarMatrizAleatoria()
    {
        gamePiece = new GamePiece[ancho, alto];

        for (int i = 0; i < ancho; i++)
        {
            for (int j = 0; j < alto; j++)
            {
                GameObject go = PiezaAleatoria();
                PiezaPosicion(go.GetComponent<GamePiece>(),i,j);
                go.transform.parent = transform;
                go.name = "GameP(" + i + ", " + j + ")";

                GamePiece gamePieces = go.GetComponent<GamePiece>();
                gamePieces.Coordenadas(i, j);
            }
        }
    }

    public void SetInitialTile(Tile ini)
    {
        inicial = null;
        inicial = ini;
    }

    public void SetFinalTile(Tile fin)
    {
        if (inicial != null)
        {
            final = fin;
        }
    }

    public void ReleaseTile()
    {
        if (inicial !=null && final != null)
        {
            SwitchPieces(inicial, final);
        }

        inicial = null;
        final = null;
    } 

    public void SwitchPieces(Tile inicial2, Tile final2)
    {
        GamePiece gpInicial= gamePiece[inicial2.indiceX, inicial2.indiceY];
        GamePiece gpFinal = gamePiece[final2.indiceX,final2.indiceY];

        gpInicial.MoverPieza(final2.indiceX, final2.indiceY, 2f);
        gpFinal.MoverPieza(inicial2.indiceX, inicial2.indiceY, 2f);
    }

    /*bool EsVecino(Tile inicial3, Tile final3)
    {
        if ()
        {
            return true;
        }

        return false;
    }*/

}


