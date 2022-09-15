using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Board : MonoBehaviour
{
    //Tablero UI
    public int alto;
    public int ancho;
    public int borde;
    public Tile[,] board;
    public Camera camara;
    public GameObject prefTile;

    //Fichas
    public GamePiece[,] gamePiece;
    public GameObject[] prefFichas;

    public float swapTime;
    [Range(0f ,.5f)]
    
    public Tile inicial;
    public Tile final;

    public bool puedeMover = true;

    private void Start()
    {
        gamePiece = new GamePiece[ancho, alto];
        CrearBoard();
        OrganizarCamara();
        LlenarMatriz();
        ResaltarCoincidencias();
        
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
                board[i, j] = tile;
                tile.Incializar(i, j);
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
        int numeroR = Random.Range(0, prefFichas.Length);
        GameObject go = Instantiate(prefFichas[numeroR]);
        go.GetComponent<GamePiece>().board = this;
        return go;
    }

    public void PiezaPosicion(GamePiece gp, int x, int y)
    {
        gp.transform.position = new Vector3(x, y, 0f);
        gp.Coordenadas(x, y);
        gamePiece[x, y] = gp;
    }

    void LlenarMatriz()
    {
        List<GamePiece> addedPieces = new List<GamePiece>();

        for (int x = 0; x < ancho; x++)
        {
            for (int y = 0; y < alto; y++)
            {
                if (gamePiece[x,y] == null)
                {
                    GamePiece gameP = LlenarMatrizAleatoriaEn(x, y);
                    addedPieces.Add(gameP);
                }
            }
        }

        bool estaLlena = false;
        int interacciones = 0;
        int interaccionesMaximas = 100;

        while (!estaLlena)
        {
            List<GamePiece> coincidencias = EncontrarTodaslasCoincidencias();

            if (coincidencias.Count == 0)
            {
                estaLlena = true;
                break;
            }
            else
            {
                coincidencias = coincidencias.Intersect(addedPieces).ToList();
                ReemplazarConPiezaAleatoria(coincidencias);
            }
            if (interacciones > interaccionesMaximas)
            {
                estaLlena = true;
                Debug.LogWarning("Se alcanzó el número máximo de interacciones");
            }
            interacciones++;
        }
    }
    private void ReemplazarConPiezaAleatoria(List<GamePiece> coincidencias)
    {
        foreach (GamePiece gamePiece in coincidencias)
        {
            ClearPiecesAt(gamePiece.cordenadaX, gamePiece.cordenadaY);
            LlenarMatrizAleatoriaEn(gamePiece.cordenadaX, gamePiece.cordenadaY);
        }
    }

    GamePiece LlenarMatrizAleatoriaEn(int x, int y)
    {
        GameObject go = PiezaAleatoria();
        PiezaPosicion(go.GetComponent<GamePiece>(), x, y);
        return go.GetComponent<GamePiece>();
    }

    public void InitialTile(Tile ini)
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
        StartCoroutine(SwitchTileCourutine(inicial2, final2));
    }

    IEnumerator SwitchTileCourutine(Tile inicial2, Tile final2)
    {
        if (puedeMover)
        {
            puedeMover = false;

            GamePiece gpInicial = gamePiece[inicial2.indiceX, inicial2.indiceY];
            GamePiece gpFinal = gamePiece[final2.indiceX, final2.indiceY];

            if (gpInicial != null && gpFinal != null)
            {

                gpInicial.MoverPieza(final2.indiceX, final2.indiceY, swapTime);
                gpFinal.MoverPieza(inicial2.indiceX, inicial2.indiceY, swapTime);

                yield return new WaitForSeconds(swapTime);

                List<GamePiece> listaPiezaInicial = EncontrarCoincidenciasEn(gpInicial.cordenadaX, gpInicial.cordenadaY);
                List<GamePiece> listaPiezaFinal = EncontrarCoincidenciasEn(gpFinal.cordenadaX, gpFinal.cordenadaY);

                if (listaPiezaInicial.Count == 0 && listaPiezaFinal.Count == 0)
                {
                    gpInicial.MoverPieza(inicial2.indiceX, inicial2.indiceY, swapTime);
                    gpFinal.MoverPieza(final2.indiceX, final2.indiceY, swapTime);
                    yield return new WaitForSeconds(swapTime);
                    puedeMover = true;
                }
                else
                {
                    listaPiezaInicial = listaPiezaInicial.Union(listaPiezaFinal).ToList();
                    ClearAndRefillBoard(listaPiezaInicial);
                }

            }
        }

    }

    public bool EsVecino(Tile inicial3, Tile final3)
    {
        if (Mathf.Abs(inicial3.indiceX - final3.indiceX) == 1 && (inicial3.indiceY == final3.indiceY))
        {
            return true;
        }
        else
        {
            if (Mathf.Abs(inicial3.indiceY - final3.indiceY) == 1 && (inicial3.indiceX == final3.indiceX))
            {
              return true;
            }
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

            if (siguientepieza == null)
            {
                break;
            }
            else
            {
                if (piezaInicial.tipoFicha == siguientepieza.tipoFicha && !coincidencias.Contains(siguientepieza))
                {
                    coincidencias.Add(siguientepieza);
                }
                else
                {
                    break;
                }
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
        List<GamePiece> abajo = EncontrarCoincidencias(startX, startY, Vector2.down, 2);

        if (arriba == null)
        {
            arriba = new List<GamePiece>();
        }
        if (abajo == null)
        {
            abajo = new List<GamePiece>();
        }

        var listaCombinadas = arriba.Union(abajo).ToList();
        return listaCombinadas.Count >= cantidadMinima ? listaCombinadas : null;
    }
    List<GamePiece> BusquedaHorizontal(int startX, int startY, int cantidadMinima = 3)
    {
        List<GamePiece> derecha = EncontrarCoincidencias(startX, startY, Vector2.right, 2);
        List<GamePiece> izquierda = EncontrarCoincidencias(startX, startY, Vector2.left, 2);

        if (izquierda == null)
        {
            izquierda = new List<GamePiece>();
        }

        if (derecha == null)
        {
            derecha = new List<GamePiece>();
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
    private void ResaltarTile(int x, int y, Color col)
    {
        SpriteRenderer sr = board[x, y].GetComponent<SpriteRenderer>();
        sr.color = col;
    }

    List<GamePiece> EncontrarCoincidenciasEn(int _x, int _y)
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
    List<GamePiece> EncontrarCoincidenciasEn(List<GamePiece> gamePieces, int minLenght = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();

        foreach (GamePiece gp in gamePieces)
        {
            matches = matches.Union(EncontrarCoincidenciasEn(gp.cordenadaX, gp.cordenadaY)).ToList();
        }
        return matches;
    }
    private List<GamePiece> EncontrarTodaslasCoincidencias()
    {
        List<GamePiece> todasLasCoincidencias = new List<GamePiece>();

        for (int i = 0; i < ancho; i++)
        {
            for (int j = 0; j < alto; j++)
            {
                var coincidencias = EncontrarCoincidenciasEn(i, j);
                todasLasCoincidencias = todasLasCoincidencias.Union(coincidencias).ToList();
            }
        }

        return todasLasCoincidencias;
    }
    void ClearBoard()
    {
        for (int i = 0; i < ancho; i++)
        {
            for (int j = 0; j < alto; j++)
            {
                ClearPiecesAt(i,j);
            }
        }
    }

    private void ClearPiecesAt(int x, int y)
    {
        GamePiece pieceToClear = gamePiece[x, y];
        if (pieceToClear != null)
        {
            gamePiece[x, y] = null;
            Destroy(pieceToClear.gameObject);
        }
    }

    void ClearPiecesAt(List<GamePiece> gamePieces)
    {
        foreach (GamePiece gp in gamePieces)
        {
            if (gp != null)
            {
                ClearPiecesAt(gp.cordenadaX, gp.cordenadaY);

            }
        }
    }

    List<GamePiece> CollapseColumn(int column, float collapseTime = 0.1f)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();

        for (int i = 0; i < alto -1; i++)
        {
            if (gamePiece[column, i] == null)
            {
                for (int j = i + 1 ; j < alto; j++)
                {
                    if (gamePiece[column, j] != null)
                    {
                        gamePiece[column, j].MoverPieza(column,i,collapseTime);
                        gamePiece[column, i] = gamePiece[column, j];
                        gamePiece[column, j].Coordenadas(column, i);

                        if (!movingPieces.Contains(gamePiece[column,i]))
                        {
                            movingPieces.Add(gamePiece[column, i]);
                        }

                        gamePiece[column, j] = null;
                        break;
                    }
                }
            }
        }
        return movingPieces;
    }

    List<GamePiece> CollapseColumn(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<int> collumnToCollapse = GetCollumns(gamePieces);

        foreach (int collumn in collumnToCollapse)
        {
            movingPieces = movingPieces.Union(CollapseColumn(collumn)).ToList();
        }
        return movingPieces;
    }

    List<int> GetCollumns(List<GamePiece> gamePieces)
    {
        List<int> collumnIndex = new List<int>();

        foreach (GamePiece gP in gamePieces)
        {
            if (!collumnIndex.Contains(gP.cordenadaX))
            {
                collumnIndex.Add(gP.cordenadaX);
            }
        }
        return collumnIndex;
    }

    void ClearAndRefillBoard(List<GamePiece> gamePieces)
    {
        StartCoroutine(ClearAndRefillBoardRoutine(gamePieces));
    }

    IEnumerator ClearAndRefillBoardRoutine(List<GamePiece> gamePieces)
    {
        yield return StartCoroutine(ClearAndCollapseColumn(gamePieces));
        yield return null;
        yield return StartCoroutine(RefillRoutine());
        puedeMover = true;
    }
    IEnumerator ClearAndCollapseColumn(List<GamePiece> gamePieces)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<GamePiece> matches = new List<GamePiece>();

        bool isFinished = false;

        while (!isFinished)
        {
            ClearPiecesAt(gamePieces);
            yield return new WaitForSeconds(swapTime);
            movingPieces = CollapseColumn(gamePieces);
            matches = EncontrarCoincidenciasEn(movingPieces);

            while (!IsColapse(gamePieces))
            {
                yield return new WaitForEndOfFrame();
            }

            matches = EncontrarCoincidenciasEn(movingPieces);

            if (matches.Count == 0)
            {
                isFinished = true;
                break;
            }
            else
            {
                yield return StartCoroutine(ClearAndCollapseColumn(matches));
            }
            
        }
    
    }

    IEnumerator RefillRoutine()
    {
        LlenarMatriz();
        yield return null;
    }
     bool IsColapse(List<GamePiece> gamePieces)
    {
        foreach (GamePiece gp in gamePieces)
        {
            if (gp != null)
            {
                if (gp.transform.position.y - (float) gp.cordenadaY > 0.001f)
                {
                    return false;
                }
            }
        }
        return true;
    }
}

