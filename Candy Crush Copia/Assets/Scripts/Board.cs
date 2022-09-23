using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class Board : MonoBehaviour
{
    public int ancho; //width
    public int alto; // height

    public int tamanoBorde; // borderSize

    public GameObject prefTile; //tilePrefab
    public GameObject[] prefFichas; // gamepiecesPrefab
    
    public float swapTime = .3f; // swapTime
    
    public Tile[,] board; // m_allTiles
    public GamePiece[,] gamePiece; // m_allGamePieces 

    public Tile initialTile; // m_clickedTile
    public Tile finalTile; // m_TargetTile

    public bool puedeMover = true; // m_playerInputEnabled

    Transform tileParent;
    Transform gamepieceParent;

    // public Camera camara;

    private void Start()
    {
        SetParents();

        board = new Tile [ancho, alto]; // m_allTiles
        gamePiece = new GamePiece[ancho, alto]; // m_allGamePieces

        CrearBoard(); // SetupTiles
        OrganizarCamara(); // setUpCamera
        LlenarMatriz(10, .5f); // Fill Board
        
        // ResaltarCoincidencias();
    }

    void SetParents()
    {
        if (tileParent == null)
        {
            tileParent = new GameObject().transform;
            tileParent.name = "Tiles"; // Cambiar nombre
            tileParent.parent = this.transform;
        }

        if (gamepieceParent == null)
        {
            gamepieceParent = new GameObject().transform;
            gamepieceParent.name = "Gamepieces";
            gamepieceParent = this.transform;
        }
    }

    void OrganizarCamara() // setUpCamera
    {
        Camera.main.transform.position = new Vector3((float)(ancho - 1) / 2f, (float)(alto - 1) / 2f, -10f);

        float aspectRatio = (float)Screen.width / (float)Screen.height;
        float verticalSize = (float) alto / 2f + (float)tamanoBorde;
        float horizontalSize = ((float)ancho / 2f + (float)tamanoBorde) / aspectRatio;
        Camera.main.orthographicSize = verticalSize > horizontalSize ? verticalSize : horizontalSize;
    }


    void CrearBoard() // SetUpTiles
    {
        for (int i = 0; i < ancho; i++)
        {
            for (int j = 0; j < alto; j++)
            {
                GameObject tile = Instantiate(prefTile, new Vector2(i, j), Quaternion.identity);
                tile.name = $"Tile({i},{j})";
                
                if (tileParent != null)
                {
                    tile.transform.parent = tileParent;
                }

                board[i, j] = tile.GetComponent<Tile>();
                board[i, j].Incializar(i, j, this);
            }
        }
    }

    void LlenarMatriz(int falseOffset = 0, float moveTime = .1f) // FillBoard
    {
        List<GamePiece> addedPieces = new List<GamePiece>();

        for (int i = 0; i < ancho; i++)
        {
            for (int j = 0; j < alto; j++)
            {
                if (gamePiece[i,j] == null)
                {
                    if (falseOffset == 0)
                    {
                        GamePiece piece = LlenarMatrizAleatoriaEn(i, j);
                        addedPieces.Add(piece);
                    }

                    else
                    {
                       GamePiece piece = LlenarMatrizAleatoriaEn(i, j, falseOffset, moveTime);
                        addedPieces.Add(piece);
                    }
                }
            }
        }

        int interaccionesMaximas = 20; // maxIterations
        int interacciones = 0; // iterations

        
        bool estaLlena = false; // isFilled
        
        while (!estaLlena)
        {
            List<GamePiece> matches = EncontrarTodaslasCoincidencias(); // EncontrarTodasLasCoincidencias = FindAllMatcehs

            if (matches.Count == 0)
            {
                estaLlena = true;
                break;
            }
            else
            {
                matches = matches.Intersect(addedPieces).ToList();
                
                if (falseOffset == 0)
                {
                    ReemplazarConPiezaAleatoria(matches); // ReemplazarConPiezaAleatoria = ReplaceWithRandom???
                }
                else
                {
                    ReemplazarConPiezaAleatoria(matches, falseOffset, moveTime);
                }
            }

            if (interacciones > interaccionesMaximas )
            {
                estaLlena = true;
                Debug.LogWarning($"Board.FillBoard alcanzo el maximo de interacciones");
            }

            interacciones++;
           
        }
    }
    public void InitialTile(Tile tile) // ClickedTile
    {
        if (initialTile == null)
        {
            initialTile = tile;
        }
    }

    public void SetFinalTile(Tile tile) // DragToTile
    {
        if (initialTile != null && EsVecino (tile , initialTile)) // EsVecino = IsNexTo
        {
            finalTile = tile;
        }
    }

    public void ReleaseTile()
    {
        if (initialTile != null && finalTile != null)
        {
            SwitchPieces(initialTile, finalTile); // SwitchPieces = SwitchTile
        }

        initialTile = null;
        finalTile = null;
    }

    public void SwitchPieces(Tile initialTile, Tile finalTile) // SwitchTiles
    {
        StartCoroutine(SwitchTileCourutine(initialTile, finalTile));
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
                    //Score(points);
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

    /*Score
    public static int scoreValue = 0;
    public TMP_Text score;
    private string scoreEnPantalla;
    public string scoreFinal;
    public int points = 10;*/

    /* public void Score(int points)
    {
        scoreValue += points;
        scoreEnPantalla = "Score" + ":" + scoreValue;
        score.text = scoreEnPantalla;

        scoreFinal = score.text;
    }*/
}

