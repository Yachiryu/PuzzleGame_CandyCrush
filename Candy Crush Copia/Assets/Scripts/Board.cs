using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Board : MonoBehaviour
{
    public int alto;
    public int ancho;
    public int borde;

    public Tile[,] board;
    public GamePiece[,] gamePiece;
    public GameObject prefTile;

    public GameObject[] prefPuntos;
    public Camera camara;

    public Tile inicial;
    public Tile final;

    private void Start()
    {
        CrearBoard();
        OrganizarCamara();
        LlenarMatrizAleatoria();
        ResaltarCoincidencias();
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
                tile.Incializar(i, j);
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

        camara.orthographicSize = sizeY > sizeX ? sizeY : sizeX;
    }

    GameObject PiezaAleatoria()
    {
        int numeroR = Random.Range(0, prefPuntos.Length);
        GameObject go = Instantiate(prefPuntos[numeroR]);
        go.GetComponent<GamePiece>().board = this;
        return go;
    }

    public void PiezaPosicion(GamePiece gp, int x, int y)
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
                PiezaPosicion(go.GetComponent<GamePiece>(), i, j);
                go.transform.parent = transform;
                go.name = "GameP(" + i + ", " + j + ")";

                GamePiece gamePieces = go.GetComponent<GamePiece>();
                gamePieces.Coordenadas(i, j);
            }
        }
    }

    public void SetInitialTile(Tile ini)
    {
        if (inicial == null)
        {
             inicial = ini;
        }
    }

    public void SetFinalTile(Tile fin)
    {
        if (inicial != null && EsVecino(inicial, fin) == true)
        {
            final = fin;
        }
    }

    public void ReleaseTile()
    {
        if (inicial != null && final != null)
        {
            SwitchPieces(inicial, final);
        }

        inicial = null;
        final = null;
    }

    public void SwitchPieces(Tile inicial2, Tile final2)
    {
        GamePiece gpInicial = gamePiece[inicial2.indiceX, inicial2.indiceY];
        GamePiece gpFinal = gamePiece[final2.indiceX, final2.indiceY];

        gpInicial.MoverPieza(final2.indiceX, final2.indiceY, 0.25f);
        gpFinal.MoverPieza(inicial2.indiceX, inicial2.indiceY, 0.25f);

        ResaltarCoincidenciasEn(gpInicial.cordenadaX, gpInicial.cordenadaY);
        ResaltarCoincidenciasEn(gpFinal.cordenadaX, gpFinal.cordenadaY);
    }

    public bool EsVecino(Tile inicial3, Tile final3)
    {
        if (Mathf.Abs(inicial3.indiceX - final3.indiceX) == 1 && (inicial3.indiceY == final3.indiceY))
        {
            return true;
        }
     
        if (Mathf.Abs(inicial3.indiceY - final3.indiceY) == 1 && (inicial3.indiceX == final3.indiceX))
        {
          return true;
        }

        return false;
       
    }

    public bool EstaEnRango(int x, int y)
    {
        return (x < ancho && x >= 0 && y < alto && y >= 0);
    }

    List<GamePiece> EncontrarCoincidencias(int startX, int startY, Vector2 direccionDeBusqueda, int cantidadMinima = 3)
    {
        List<GamePiece> coincidencias = new List<GamePiece>();

        GamePiece piezaInicial = null;

        if (EstaEnRango(startX, startY))
        {
            piezaInicial = gamePiece[startX, startY];
        }
        if (piezaInicial != null)
        {
            coincidencias.Add(piezaInicial);
        }
        else
        {
            return null;
        }

        int siguienteX;
        int siguienteY;

        int valorMaximo = ancho > alto ? ancho : alto;

        for (int i = 1; i < valorMaximo - 1; i++)
        {
            siguienteX = startX + (int)Mathf.Clamp(direccionDeBusqueda.x, -1, 1) * i;
            siguienteY = startY + (int)Mathf.Clamp(direccionDeBusqueda.y, -1, 1) * i;

            if (!EstaEnRango(siguienteX, siguienteY))
            {
                break;
            }

            GamePiece siguientepieza = gamePiece[siguienteX, siguienteY];

            if (piezaInicial.tipoFicha == siguientepieza.tipoFicha && !coincidencias.Contains(siguientepieza))
            {
                coincidencias.Add(siguientepieza);
            }
            else
            {
                break;
            }
        }
        if (coincidencias.Count >= cantidadMinima)
        {
            return coincidencias;
        }
        return null;
    }

    List<GamePiece> BusquedaVertical(int startX, int startY, int cantidadMinima = 3)
    {
        List<GamePiece> arriba = EncontrarCoincidencias(startX, startY, Vector2.up, 2);
        List<GamePiece> Abajo = EncontrarCoincidencias(startX, startY, Vector2.down, 2);

        if (arriba == null)
        {
            arriba = new List<GamePiece>();
        }
        if (Abajo == null)
        {
            Abajo = new List<GamePiece>();
        }

        var listaCombinadas = arriba.Union(Abajo).ToList();
        return listaCombinadas.Count >= cantidadMinima ? listaCombinadas : null;
    }
    List<GamePiece> BusquedaHorizontal(int startX, int startY, int cantidadMinima = 3)
    {
        List<GamePiece> derecha = EncontrarCoincidencias(startX, startY, Vector2.right, 2);
        List<GamePiece> izquierda = EncontrarCoincidencias(startX, startY, Vector2.left, 2);


        if (derecha == null)
        {
            derecha = new List<GamePiece>();
        }
        if (izquierda == null)
        {
            izquierda = new List<GamePiece>();
        }
        
        var listaCombinadas = derecha.Union(izquierda).ToList();

        return listaCombinadas.Count >= cantidadMinima ? listaCombinadas : null;
    }

    public void ResaltarCoincidencias()
    {
        for (int i = 0; i < ancho; i++)
        {
            for (int j = 0; j < alto; j++)
            {
                ResaltarCoincidenciasEn(i, j);
            }
        }
    }

    public void ResaltarCoincidenciasEn(int _x, int _y)
    {
        var listasCombinadas = EncontrarCoincidenciasEn(_x, _y);

        if (listasCombinadas.Count > 0)
        {
            foreach (GamePiece piece in listasCombinadas)
            {
                ResaltarTile(piece.cordenadaX, piece.cordenadaY, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }
    private List<GamePiece> EncontrarCoincidenciasEn(int _x, int _y)
    {
        List<GamePiece> horizontal = BusquedaHorizontal(_x, _y, 3);
        List<GamePiece> vertical = BusquedaVertical(_x, _y, 3);

        if (horizontal == null)
        {
            horizontal = new List<GamePiece>();
        }
        if (vertical == null)
        {
            vertical = new List<GamePiece>();
        }
        var listasCombinadas = horizontal.Union(vertical).ToList();
        return listasCombinadas;
    }

    private void ResaltarTile(int x, int y , Color col)
    {
        SpriteRenderer sr = board[x, y].GetComponent<SpriteRenderer>();
        sr.color = col;
    }
    
    


   
}

