using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class Board : MonoBehaviour
{
    public int width; //
    public int height; //

    public int borderSize; //

    public GameObject tilePrefab; //
    public GameObject[] gamePiecesPrefabs; //

    public float swapTime = .3f; //

    Tile[,] m_allTiles; //
    GamePiece[,] m_allGamePieces; //


    [SerializeField] Tile m_clickedTile; //
    [SerializeField] Tile m_targetTile; //

    bool m_playerInputEnabled = true; //

    Transform tileParent; //
    Transform gamePieceParent; // 

    private void Start()
    {
        SetParents(); //

        m_allTiles = new Tile[width, height];//
        m_allGamePieces = new GamePiece[width, height]; //

        SetupTiles(); // 
        SetupCamera(); // 
        FillBoard(10, .5f); // Fill Board ///
    }

    void SetParents() //
    {
        if (tileParent == null) // 
        {
            tileParent = new GameObject().transform; // 
            tileParent.name = "Tiles"; // Cambiar nombre //
            tileParent.parent = this.transform;
        }

        if (gamePieceParent == null) //
        {
            gamePieceParent = new GameObject().transform; //
            gamePieceParent.name = "GamePieces"; //
            gamePieceParent = this.transform;
        }
    }

    void SetupCamera() // 
    {
        Camera.main.transform.position = new Vector3((float)(width - 1) / 2f, (float)(height - 1) / 2f, -10f); //

        float aspectRatio = (float)Screen.width / (float)Screen.height; //
        float verticalSize = (float)height / 2f + (float)borderSize; //
        float horizontalSize = ((float)width / 2f + (float)borderSize) / aspectRatio; //
        Camera.main.orthographicSize = verticalSize > horizontalSize ? verticalSize : horizontalSize; //
    }


    void SetupTiles() //
    {
        for (int i = 0; i < width; i++) //
        {
            for (int j = 0; j < height; j++) //
            {
                GameObject tile = Instantiate(tilePrefab, new Vector2(i, j), Quaternion.identity); //
                tile.name = $"Tile({i},{j})"; //

                if (tileParent != null) //
                {
                    tile.transform.parent = tileParent; // 
                }

                m_allTiles[i, j] = tile.GetComponent<Tile>(); //
                m_allTiles[i, j].Init(i, j, this); //
            }
        }
    }

    void FillBoard(int falseOffset = 0, float moveTime = .1f) // 
    {
        List<GamePiece> addedPieces = new List<GamePiece>(); //

        for (int i = 0; i < width; i++) // 
        {
            for (int j = 0; j < height; j++)
            {
                if (m_allGamePieces[i, j] == null)
                {
                    if (falseOffset == 0)
                    {
                        GamePiece piece = FillRandomAt(i, j); // FillRandomAt = LlenarMatrizAleatoriaEn ///
                        addedPieces.Add(piece); //
                    }
                    else
                    {
                        GamePiece piece = FillRandomAt(i, j, falseOffset, moveTime); //
                        addedPieces.Add(piece); //
                    }
                }
            }
        }

        int maxIterations = 20; // 
        int iterations = 0; //


        bool isFilled = false; //

        while (!isFilled) //
        {
            List<GamePiece> matches = FindAllMatches(); // FindAllMatches = EncontrarTodasLasCoincidencias ///

            if (matches.Count == 0) //
            {
                isFilled = true; //
                break; //
            }
            else
            {
                matches = matches.Intersect(addedPieces).ToList(); //

                if (falseOffset == 0)
                {
                    ReplaceWithRandom(matches); // ReplaceWithRandom = ReemplazarConPiezaAleatoria ///
                }
                else
                {
                    ReplaceWithRandom(matches, falseOffset, moveTime); ///
                }
            }

            if (iterations > maxIterations) //
            {
                isFilled = true; //
                Debug.LogWarning($"Board.FillBoard alcanzo el maximo de interacciones"); //
            }
            iterations++;
        }
    }
    public void ClickedTile(Tile tile) // 
    {
        if (m_clickedTile == null)
        {
            m_clickedTile = tile;
        }
    }

    public void DragToTile(Tile tile) // 
    {
        if (m_clickedTile != null && IsNextTo(tile, m_clickedTile)) // IsNexTo = EsVecino ///
        {
            m_targetTile = tile; //
        }
    }

    public void ReleaseTile() //
    {
        if (m_clickedTile != null && m_targetTile != null) //
        {
            SwitchTiles(m_clickedTile, m_targetTile); // SwitchTiles = SwitchPieces /// 
        }
        m_clickedTile = null; //
        m_targetTile = null; //
    }

    public void SwitchTiles(Tile m_clickedTile, Tile m_targetTile) // 
    {
        StartCoroutine(SwitchTilesRoutine(m_clickedTile, m_targetTile)); ///
    }

    IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile) // SwitchTilesRoutine = SwitchTilesCourutine ///
    {
        if (m_playerInputEnabled) //
        {
            GamePiece clickedPiece = m_allGamePieces[clickedTile.xIndex, clickedTile.yIndex]; //
            GamePiece targetPiece = m_allGamePieces[targetTile.xIndex, targetTile.yIndex]; //

            if (clickedPiece != null && targetPiece != null) //
            {

                clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime); //
                targetPiece.Move(clickedPiece.xIndex, clickedPiece.yIndex, swapTime); //

                yield return new WaitForSeconds(swapTime); //

                List<GamePiece> clickedPieceMatches = FindMatchesAt(clickedTile.xIndex, clickedTile.yIndex); // FindMatchesAt = EncontrarCoincidenciasEn ///
                List<GamePiece> targetPieceMatches = FindMatchesAt(targetTile.xIndex, targetTile.yIndex); // 

                if (clickedPieceMatches.Count == 0 && targetPieceMatches.Count == 0) //
                {
                    clickedPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime); //
                    targetPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime); //
                    yield return new WaitForSeconds(swapTime); //
                }
                else
                {
                    yield return new WaitForSeconds(swapTime); //

                    ClearAndRefillBoard(clickedPieceMatches.Union(targetPieceMatches).ToList()); //
                }

            }
        }

    }

    void ClearAndRefillBoard(List<GamePiece> gamePieces) //
    {
        StartCoroutine(ClearAndRefillRoutine(gamePieces)); ///
    }

    List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 3) // FindMatches = EncontrarCoincidencias ///
    {
        List<GamePiece> matches = new List<GamePiece>(); //
        GamePiece startPiece = null; // 

        if (IsWithBounds(startX, startY)) // IsWithBounds = EstaEnRango ///
        {
            startPiece = m_allGamePieces[startX, startY]; //
        }
        if (startPiece != null) //
        {
            matches.Add(startPiece); //
        }
        else
        {
            return null; //
        }

        int nextX; //
        int nextY; //

        int maxValue = width > height ? width : height; //

        for (int i = 1; i < maxValue; i++) //
        {
            nextX = startX + (int)Mathf.Clamp(searchDirection.x, -1, 1) * i; //
            nextY = startY + (int)Mathf.Clamp(searchDirection.y, -1, 1) * i; //

            if (!IsWithBounds(nextX, nextY)) //
            {
                break; //
            }

            GamePiece nextPiece = m_allGamePieces[nextX, nextY]; //

            if (nextPiece == null) //
            {
                break; //
            }
            else
            {
                if (nextPiece.tipoFicha == startPiece.tipoFicha && !matches.Contains(nextPiece)) //
                {
                    matches.Add(nextPiece); //
                }
                else
                {
                    break; //
                }
            }
        }

        if (matches.Count >= minLength) //
        {
            return matches; //
        }
        else
        {
            return null; //
        }
    }

    List<GamePiece> FindVerticalMatches(int startX, int startY, int minLenght = 3) // FindVerticalMatches = BusquedaVertical ///
    {
        List<GamePiece> upwardMatches = FindMatches(startX, startY, Vector2.up, 2); //
        List<GamePiece> downwardMatches = FindMatches(startX, startY, Vector2.down, 2); //

        if (upwardMatches == null) //
        {
            upwardMatches = new List<GamePiece>(); //
        }

        if (downwardMatches == null)
        {
            downwardMatches = new List<GamePiece>();
        }

        var combinedMatches = upwardMatches.Union(downwardMatches).ToList(); //
        return combinedMatches.Count >= minLenght ? combinedMatches : null; //
    }
    List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLenght = 3) // FindHorizontalMatches = BusquedaHorizontal /// 
    {
        List<GamePiece> rightMatches = FindMatches(startX, startY, Vector2.right, 2); // 
        List<GamePiece> leftMatches = FindMatches(startX, startY, Vector2.left, 2); //

        if (rightMatches == null) //
        {
            rightMatches = new List<GamePiece>(); //
        }

        if (leftMatches == null) //
        {
            leftMatches = new List<GamePiece>(); //
        }

        var combinedMatches = rightMatches.Union(leftMatches).ToList(); //
        return combinedMatches.Count >= minLenght ? combinedMatches : null; //
    }

    List<GamePiece> FindMatchesAt(int x, int y, int minLength = 3) // FindMatchesAt = EncontrarCoincidenciasEn ///
    {
        List<GamePiece> horizontalMatches = FindHorizontalMatches(x, y, minLength); //
        List<GamePiece> verticalMatches = FindVerticalMatches(x, y, minLength); //

        if (horizontalMatches == null) //
        {
            horizontalMatches = new List<GamePiece>(); //
        }

        if (verticalMatches == null) //
        {
            verticalMatches = new List<GamePiece>(); //
        }

        var combinedMatches = horizontalMatches.Union(verticalMatches).ToList();
        return combinedMatches;
    }
    List<GamePiece> FindMatchesAt(List<GamePiece> gamePieces, int minLenght = 3) // FindMatchesAt = EncontrarCoincidenciasEn ///
    {
        List<GamePiece> matches = new List<GamePiece>(); //

        foreach (GamePiece piece in gamePieces) //
        {
            matches = matches.Union(FindMatchesAt(piece.xIndex, piece.yIndex, minLenght)).ToList(); //
        }
        return matches; //
    }
    private bool IsNextTo(Tile start, Tile end) // IsNexTo = EsVecino
    {
        if (Mathf.Abs(start.xIndex - end.xIndex) == 1 && start.yIndex == end.yIndex) //
        {
            return true; //
        }

        if (Mathf.Abs(start.yIndex - end.yIndex) == 1 && start.xIndex == end.xIndex) //
        {
            return true; //
        }

        return false; //
    }

    private List<GamePiece> FindAllMatches() // FindAllMatches = EncontrarTodasLasCoincidencias
    {
        List<GamePiece> combinedMatches = new List<GamePiece>(); //

        for (int i = 0; i < width; i++) //
        {
            for (int j = 0; j < height; j++) //
            {
                var matches = FindMatchesAt(i, j); //
                combinedMatches = combinedMatches.Union(matches).ToList(); //
            }
        }
        return combinedMatches; //
    }
    void HihglightTileOff(int x, int y) // HighlightTileOff = ResaltarTile ///
    {
        SpriteRenderer spriteRenderer = m_allTiles[x, y].GetComponent<SpriteRenderer>(); //
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b,0); //
    }


    private void HihglightTileOn(int x, int y, Color col) // HighlightTileOn = ResaltarTile /// 
    {
        SpriteRenderer spriteRenderer = m_allTiles[x, y].GetComponent<SpriteRenderer>(); //
        spriteRenderer.color = col; //
    }

    public void HighlightMatchesAt(int x, int y) // HighlightMatchesAt = ResaltarCoincidenciasEn ///
    {
        HihglightTileOff(x, y); //
        var combinedMatches = FindMatchesAt(x, y); //

        if (combinedMatches.Count > 0) //
        {
            foreach (GamePiece piece in combinedMatches) //
            {
                HihglightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color); //
            }
        }
        
    }
    public void HighLightMatches() // HighlightMatches = ResaltarCoincidencias  
    {
        for (int i = 0; i < width; i++) //
        {
            for (int j = 0; j < height; j++) //
            {
                HighlightMatchesAt(i, j); //
            }
        }

    }

    void HighLightPieces(List<GamePiece> gamePieces) // HighlightPieces = ResaltarPiezas ///
    {
        foreach (GamePiece piece in gamePieces) //
        {
            if (piece != null) //
            {
                HihglightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    private void ClearPieceAt(int x, int y) //
    {
        GamePiece pieceToClear = m_allGamePieces[x, y]; //
        if (pieceToClear != null) //
        {
            m_allGamePieces[x, y] = null; //
            Destroy(pieceToClear.gameObject); //
        }
        HihglightTileOff(x, y); //
    }
    void ClearPiecesAt(List<GamePiece> gamePieces) //
    {
        foreach (GamePiece piece in gamePieces) //
        {
            if (piece != null) //
            {
                ClearPieceAt(piece.xIndex, piece.yIndex); //
            }
        }
    }

    void ClearBoard() //
    {
        for (int i = 0; i < width; i++) //
        {
            for (int j = 0; j < height; j++) //
            {
                ClearPieceAt(i, j); //
            }
        }
    }
    GameObject GetRandomPiece() // GetRandomPiece = PiezaAleatoria ///
    {
        int randomInx = Random.Range(0, gamePiecesPrefabs.Length); //
        if (gamePiecesPrefabs[randomInx] == null) //
        {
            Debug.LogWarning($"La clase Board en el array de prefabs en la posicion{randomInx} no contiene una pieza valida"); //
        }
        return gamePiecesPrefabs[randomInx]; //
    }

    public void PlaceGamePiece(GamePiece gamePiece, int x, int y) //
    {
        if (gamePiece == null) //
        {
            Debug.LogWarning($"gamePiece invalida"); //
            return; //
        }

        gamePiece.transform.position = new Vector2(x, y); //
        gamePiece.transform.rotation = Quaternion.identity; //

        if (IsWithBounds(x, y)) //
        {
            m_allGamePieces[x, y] = gamePiece; //
        }

        gamePiece.SetCoord(x, y); //
    }

    bool IsWithBounds(int x, int y) // IsWithBounds = EstaEnRango 
    {
        return (x >= 0 && x < width && y >= 0 && y < height); //
    }
    GamePiece FillRandomAt(int x, int y, int falseOffset = 0, float moveTime = .1f) // FillRandomAt = LlenarMatrizAleatoriaEn ///
    {
        GamePiece randomPiece = Instantiate(GetRandomPiece(), Vector2.zero, Quaternion.identity).GetComponent<GamePiece>(); //
        if (randomPiece != null) //
        {
            randomPiece.Init(this); //
            PlaceGamePiece(randomPiece, x, y); //

            if (falseOffset != 0) //
            {
                randomPiece.transform.position = new Vector2(x, y + falseOffset); //
                randomPiece.Move(x, y, moveTime); //
            }
            randomPiece.transform.parent = gamePieceParent; //
        }
        return randomPiece; //
    }
    void ReplaceWithRandom(List<GamePiece> gamePieces, int falseOffset = 0, float moveTime = .1f) // ReplacedWithRandom = ReemplazarConPiezaAleatoria ///
    {
        foreach (GamePiece piece in gamePieces) //
        {
            ClearPieceAt(piece.xIndex, piece.yIndex); //
            if (falseOffset == 0) //
            {
                FillRandomAt(piece.xIndex, piece.yIndex); //
            }
            else
            {
                FillRandomAt(piece.xIndex, piece.yIndex, falseOffset, moveTime); //
            }
        }
    }

    List<GamePiece> CollapseColumn(int column, float collapseTime = .1f) //
    {
        List<GamePiece> movingPieces = new List<GamePiece>(); //

        for (int i = 0; i < height - 1; i++) //
        {
            if (m_allGamePieces[column, i] == null) //
            {
                for (int j = i + 1; j < height; j++) //
                {
                    if (m_allGamePieces[column, j] != null) //
                    {
                        m_allGamePieces[column, j].Move(column, i, collapseTime * (j - i)); //

                        m_allGamePieces[column, i] = m_allGamePieces[column, j]; //
                        m_allGamePieces[column, i].SetCoord(column, i); //

                        if (!movingPieces.Contains(m_allGamePieces[column, i])) //
                        {
                            movingPieces.Add(m_allGamePieces[column, i]); //
                        }

                        m_allGamePieces[column, j] = null; //
                        break; //
                    }
                }
            }
        }
        return movingPieces; //
    }

    List<GamePiece> CollapseColumn(List<GamePiece> gamePieces) //
    {
        List<GamePiece> movingPieces = new List<GamePiece>(); //
        List<int> collumnsToCollapse = GetCollumns(gamePieces); //

        foreach (int column in collumnsToCollapse) //
        {
            movingPieces = movingPieces.Union(CollapseColumn(column)).ToList(); //
        }

        return movingPieces; //
    }
    List<int> GetCollumns(List<GamePiece> gamePieces) //
    {
        List<int> collumns = new List<int>(); //
        foreach (GamePiece piece in gamePieces) //
        {
            if (!collumns.Contains(piece.xIndex)) //
            {
                collumns.Add(piece.xIndex); //
            }
        }
        return collumns; //
    }

    /*void ClearAndRefillBoard(List<GamePiece> gamePieces)
    {
            REVISAR FOTO 11 !!!!!!
    }*/
    IEnumerator ClearAndRefillRoutine(List<GamePiece> gamePieces) //
    {
        m_playerInputEnabled = true; //
        List<GamePiece> matches = gamePieces; //

        do
        {
            yield return StartCoroutine(ClearAndCollapseRoutine(matches)); //
            yield return null; //
            yield return StartCoroutine(RefillRoutine()); //
            matches = FindAllMatches(); //
            yield return new WaitForSeconds(.5f); // 

        } 
        while (matches.Count != 0); //
        m_playerInputEnabled = true; //

    }
    IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces) //
    {
        List<GamePiece> movingPieces = new List<GamePiece>(); //
        List<GamePiece> matches = new List<GamePiece>(); //
        HighLightPieces(gamePieces); //
        yield return new WaitForSeconds(.5f); //
        bool isFinished = false; //

        while (!isFinished) //
        {
            ClearPiecesAt(gamePieces); //
            yield return new WaitForSeconds(.25f); // Cambiar por SwapTime // 

            movingPieces = CollapseColumn(gamePieces); //
            while (!IsCollapsed(gamePieces)) //
            {
                yield return null; //
            }
            yield return new WaitForSeconds(.5f); //

            matches = FindMatchesAt(movingPieces); //

            if (matches.Count == 0) //
            {
                isFinished = true; //
                break; //
            }
            else
            {
                yield return StartCoroutine(ClearAndCollapseRoutine(matches)); //
            }
        }
        yield return null; //
    }

    IEnumerator RefillRoutine() //
    {
        FillBoard(10, .5f); //
        yield return null; //
    }
    bool IsCollapsed(List<GamePiece> gamePieces) //
    {
        foreach (GamePiece piece in gamePieces) //
        {
            if (piece != null) //
            {
                if (piece.transform.position.y - (float)piece.yIndex > 0.001f) //
                {
                    return false; //
                }
            }
        }
        return true; //
    }

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


