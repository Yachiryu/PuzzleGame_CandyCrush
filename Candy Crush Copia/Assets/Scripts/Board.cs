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
                        GamePiece piece = LlenarMatrizAleatoriaEn(i, j); // LlenarMatrizAleatoriaEn = FillRandomAt???
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
            List<GamePiece> matches = EncontrarTodaslasCoincidencias(); // EncontrarTodasLasCoincidencias = FindAllMatches

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
        StartCoroutine(SwitchTilesCourutine(initialTile, finalTile));
    }

    IEnumerator SwitchTilesCourutine(Tile initialTile, Tile finalTile) // SwitchTilesCourutine = SwitchTilesRoutine
    {
        if (puedeMover)
        {
            puedeMover = false;

            GamePiece gpInicial = gamePiece[initialTile.indiceX, initialTile.indiceY]; // gpInicial = clickedPiece
            GamePiece gpFinal = gamePiece[finalTile.indiceX, finalTile.indiceY]; // gpFinal = targetPiece

            if (gpInicial != null && gpFinal != null)
            {

                gpInicial.MoverPieza(finalTile.indiceX, finalTile.indiceY, swapTime);
                gpFinal.MoverPieza(initialTile.indiceX, initialTile.indiceY, swapTime);

                yield return new WaitForSeconds(swapTime);

                List<GamePiece> listaPiezaInicial = EncontrarCoincidenciasEn(gpInicial.cordenadaX, gpInicial.cordenadaY); // listaPiezaInicial = clickedPieceMatches
                List<GamePiece> listaPiezaFinal = EncontrarCoincidenciasEn(gpFinal.cordenadaX, gpFinal.cordenadaY); // listaPiezaFinal = targetPieceMatches

                if (listaPiezaInicial.Count == 0 && listaPiezaFinal.Count == 0)
                {
                    gpInicial.MoverPieza(initialTile.indiceX, initialTile.indiceY, swapTime);
                    gpFinal.MoverPieza(finalTile.indiceX, finalTile.indiceY, swapTime);
                    yield return new WaitForSeconds(swapTime);
                    
                    //puedeMover = true;
                }
                else
                {
                    yield return new WaitForSeconds(swapTime);

                    ClearAndRefillBoard(listaPiezaFinal.Union(listaPiezaFinal).ToList());
                }

            }
        }

    }

    void ClearAndRefillBoard(List<GamePiece> gamePieces)
    {
        StartCoroutine(ClearAndRefillBoardRoutine(gamePieces));
    }

    List<GamePiece> EncontrarCoincidencias(int startX, int startY, Vector2 direccionDeBusqueda, int cantidadMinima = 3) // EncontrarCoincidencias = FindMatches
    {
        List<GamePiece> matches = new List<GamePiece>();
        GamePiece piezaInicial = null; // piezaInicial = startPiece

        if (EstaEnRango(startX, startY)) // EstaEnRango = IsWithBounds????
        {
            piezaInicial = gamePiece[startX, startY];
        }
        if (piezaInicial != null)
        {
            matches.Add(piezaInicial);
        }
        else
        {
            return null;
        }

        int siguienteX;
        int siguienteY;

        int valorMaximo = ancho > alto ? ancho : alto; // valorMaximo = maxValue

        for (int i = 1; i < valorMaximo; i++)
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
                if (siguientepieza.tipoFicha == piezaInicial.tipoFicha && !matches.Contains(siguientepieza))
                {
                    matches.Add(siguientepieza);
                }
                else
                {
                    break;
                }
            }
        }

        if (matches.Count >= cantidadMinima)
        {
            return matches;
        }
        else
        {
            return null;
        }
    }

    List<GamePiece> BusquedaVertical(int startX, int startY, int cantidadMinima = 3) // BusquedaVertical = FindVerticalMatches
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

        var listaCombinadas = arriba.Union(abajo).ToList(); // listaCombinadas = combinedMatches
        return listaCombinadas.Count >= cantidadMinima ? listaCombinadas : null;
    }
    List<GamePiece> BusquedaHorizontal(int startX, int startY, int cantidadMinima = 3) // BusquedaHorizontal = FindHorizontalMatches
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

    List<GamePiece> EncontrarCoincidenciasEn(int x, int y, int minLength = 3) // EncontrarCoincidenciasEn = FindMatchesAt
    {
        List<GamePiece> horizontalmatches = BusquedaHorizontal(x, y, minLength);
        List<GamePiece> verticalMatches = BusquedaVertical(x, y, minLength);

        if (horizontalmatches == null)
        {
            horizontalmatches = new List<GamePiece>();
        }

        if (verticalMatches == null)
        {
            verticalMatches = new List<GamePiece>();
        }

        var listasCombinadas = horizontalmatches.Union(verticalMatches).ToList();
        return listasCombinadas;
    }
    List<GamePiece> EncontrarCoincidenciasEn(List<GamePiece> gamePieces, int minLenght = 3) // EncontrarCoincidenciasEn = FindMatchesAt
    {
        List<GamePiece> matches = new List<GamePiece>();

        foreach (GamePiece piece in gamePieces)
        {
            matches = matches.Union(EncontrarCoincidenciasEn(piece.cordenadaX, piece.cordenadaY, minLenght)).ToList();
        }

        return matches;
    }
    public bool EsVecino(Tile initialTile, Tile finalTile) // EsVecino = IsNexTo 
    {
        if (Mathf.Abs(initialTile.indiceX - finalTile.indiceX) == 1 && initialTile.indiceY == finalTile.indiceY)
        {
            return true;
        }
       
        if (Mathf.Abs(initialTile.indiceY - finalTile.indiceY) == 1 && initialTile.indiceX == finalTile.indiceX)
        {
            return true;
        }
        
        return false;

    }

    private List<GamePiece> EncontrarTodaslasCoincidencias() // EncontrarTodasLasCoincidencias = FindAllMatcehs
    {
        List<GamePiece> coincidenciasCombinadas = new List<GamePiece>(); // coincidenciasCombinadas = combinedMatches

        for (int i = 0; i < ancho; i++)
        {
            for (int j = 0; j < alto; j++)
            {
                var coincidencias = EncontrarCoincidenciasEn(i, j);
                coincidenciasCombinadas = coincidenciasCombinadas.Union(coincidencias).ToList();
            }
        }

        return coincidenciasCombinadas;
    }
    private void ResaltarTileOff(int x, int y) // ResaltarTile = HighlightTileOff
    {
        SpriteRenderer spriteRenderer = board[x, y].GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
    }


    private void ResaltarTileOn(int x, int y, Color col) // ResaltarTile = HighlightTileOn
    {
        SpriteRenderer spriteRenderer = board[x, y].GetComponent<SpriteRenderer>();
        spriteRenderer.color = col;
    }

    public void ResaltarCoincidenciasEn(int x, int y) // ResaltarCoincidenciasEn = HighlightMatchesAt
    {
        ResaltarTileOff(x, y);
        var combinedMatches = EncontrarCoincidenciasEn(x, y);

        if (combinedMatches.Count > 0)
        {
            foreach (GamePiece piece in combinedMatches)
            {
                ResaltarTileOn(piece.cordenadaX, piece.cordenadaY, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }
    public void ResaltarCoincidencias() // ResaltarCoincidencias = HighlightMatches
    {
        for (int i = 0; i < ancho; i++)
        {
            for (int j = 0; j < alto; j++)
            {
                ResaltarCoincidenciasEn(i, j);
            }
        }
    }

    void ResaltarPiezas(List<GamePiece> gamePieces) // ResaltarPiezas = HighlightPieces
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                ResaltarTileOn(piece.cordenadaX, piece.cordenadaY, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    private void ClearPieceAt(int x, int y)
    {
        GamePiece pieceToClear = gamePiece[x, y];
        if (pieceToClear != null)
        {
            gamePiece[x, y] = null;
            Destroy(pieceToClear.gameObject);
        }

        ResaltarTileOff(x, y);
    }
    void ClearPiecesAt(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                ClearPieceAt(piece.cordenadaX, piece.cordenadaY);

            }
        }
    }

    void ClearBoard()
    {
        for (int i = 0; i < ancho; i++)
        {
            for (int j = 0; j < alto; j++)
            {
                ClearPieceAt(i, j);
            }
        }
    }
    GameObject PiezaAleatoria() // PiezaAleatoria = GetRandomPiece
    {
        int randomInx = Random.Range(0, prefFichas.Length);
        GameObject go = Instantiate(prefFichas[randomInx]);
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
            ClearPieceAt(gamePiece.cordenadaX, gamePiece.cordenadaY);
            LlenarMatrizAleatoriaEn(gamePiece.cordenadaX, gamePiece.cordenadaY);
        }
    }

    GamePiece LlenarMatrizAleatoriaEn(int x, int y)
    {
        GameObject go = PiezaAleatoria();
        PiezaPosicion(go.GetComponent<GamePiece>(), x, y);
        return go.GetComponent<GamePiece>();
    }

  

    

   

    public bool EstaEnRango(int x, int y) //EstaEnRango = IsWithBounds????
    {
        return (x < ancho && x >= 0 && y < alto && y >= 0);
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

