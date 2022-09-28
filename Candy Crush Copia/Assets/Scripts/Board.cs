using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;

public class Board : MonoBehaviour
{
    public int width; // Aqui controlamos el ancho para crear la matriz
    public int height; // Aqui controlamos el alto para crear la matriz
    public AudioSource audioSource; // Sonido

    public int borderSize; // Aqui controlamos el tamaño del borde que queramos

    public GameObject tilePrefab; // Aqui podemos seleccionar el prefab para los tiles de las fichas
    public GameObject[] gamePiecesPrefabs; // Podemos aumentar o disminuir los arrays de las fichas o cambiar su estilo

    public float swapTime = .3f; // Tiempo para que se ejecuten diferentes animaciones o eventos

    public Puntaje m_puntaje; // Referencia del script del puntaje
    int myCount = 0; // Vamos sumando el multiplicador de combos

    Tile[,] m_allTiles; // Referencia del script Tile
    GamePiece[,] m_allGamePieces; // Referencia del script GamePieces


    [SerializeField] Tile m_clickedTile; // Tile que seleccionamos con el mouse 
    [SerializeField] Tile m_targetTile; // Tile que se selecciona el mouse cuando lo arrastramos

    bool m_playerInputEnabled = true; // Booleano para controlar algunos eventos

    Transform tileParent; // Tile padre para saber la ubicacion
    Transform gamePieceParent; // Gamepiece padre para saber la ubicacion

    public int cantidadMovimientos; // Cantidad de movimiento limite que tiene el jugador
    public TextMeshProUGUI moveText; // Texto para que se mueste en pantalla los movimientos restantes

    private void Start()
    {
        SetParents();

        moveText.text = "Moves :" + cantidadMovimientos.ToString();

        m_allTiles = new Tile[width, height];
        m_allGamePieces = new GamePiece[width, height]; 

        SetupTiles(); 
        SetupCamera();
        FillBoard(10, .5f);
    }

    void SetParents() // Volvemos padres los GamePieces y los Tiles para organizar mejor en la herarquia
    {
        if (tileParent == null) 
        {
            tileParent = new GameObject().transform; 
            tileParent.name = "Tiles"; // Cambiar nombre 
            tileParent.parent = this.transform;
        }

        if (gamePieceParent == null) 
        {
            gamePieceParent = new GameObject().transform; 
            gamePieceParent.name = "GamePieces"; 
            gamePieceParent = this.transform;
        }
    }

    void SetupCamera() // Organiza la camara segun el tamaño de la pantalla y el tamaño de las fichas
    {
        Camera.main.transform.position = new Vector3((float)(width - 1) / 2f, (float)(height - 1) / 2f, -10f); 

        float aspectRatio = (float)Screen.width / (float)Screen.height; 
        float verticalSize = (float)height / 2f + (float)borderSize; 
        float horizontalSize = ((float)width / 2f + (float)borderSize) / aspectRatio; 
        Camera.main.orthographicSize = verticalSize > horizontalSize ? verticalSize : horizontalSize; 
    }


    void SetupTiles() // Instanciamos los prefabs de los tiles para crear la matriz
    {
        for (int i = 0; i < width; i++) 
        {
            for (int j = 0; j < height; j++) 
            {
                GameObject tile = Instantiate(tilePrefab, new Vector2(i, j), Quaternion.identity); 
                tile.name = $"Tile({i},{j})"; 

                if (tileParent != null) 
                {
                    tile.transform.parent = tileParent; 
                }

                m_allTiles[i, j] = tile.GetComponent<Tile>();
                m_allTiles[i, j].Init(i, j, this); 
            }
        }
    }

    void FillBoard(int falseOffset = 0, float moveTime = .1f) // Rellenamos la matriz creada con las fichas aleatorias 
    {
        List<GamePiece> addedPieces = new List<GamePiece>(); 

        for (int i = 0; i < width; i++) 
        {
            for (int j = 0; j < height; j++)
            {
                if (m_allGamePieces[i, j] == null)
                {
                    if (falseOffset == 0)
                    {
                        GamePiece piece = FillRandomAt(i, j); 
                        addedPieces.Add(piece); 
                    }
                    else
                    {
                        GamePiece piece = FillRandomAt(i, j, falseOffset, moveTime); 
                        addedPieces.Add(piece); 
                    }
                }
            }
        }

        int maxIterations = 30; // 
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
    public void ClickedTile(Tile tile) // Tile al cual le hacemos click
    {
        if (m_clickedTile == null)
        {
            m_clickedTile = tile;
        }
    }

    public void DragToTile(Tile tile) // Tile al cual queremos llegar cuando arrastramos
    {
        if (m_clickedTile != null && IsNextTo(tile, m_clickedTile)) 
        {
            m_targetTile = tile; 
        }
    }

    public void ReleaseTile() // Aqui liberamos el click y este será el tile al que queremos que se mueva la ficha
    {
        if (m_clickedTile != null && m_targetTile != null) 
        {
            SwitchTiles(m_clickedTile, m_targetTile);
        }
        m_clickedTile = null;
        m_targetTile = null; 
    }

    public void SwitchTiles(Tile m_clickedTile, Tile m_targetTile) // Llamamos a la corutina de switchTiles ademas de recibir las coordenadas de los tiles 
    {
        StartCoroutine(SwitchTilesRoutine(m_clickedTile, m_targetTile)); 
    }

    IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile) // En esta corutina efectuamos el cambio de los Tiles que queremos intercambiar para efectuar los matches
    {
        if (m_playerInputEnabled)
        {
            GamePiece clickedPiece = m_allGamePieces[clickedTile.xIndex, clickedTile.yIndex];
            GamePiece targetPiece = m_allGamePieces[targetTile.xIndex, targetTile.yIndex];

            if (clickedPiece != null && targetPiece != null)
            {

                clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
                targetPiece.Move(clickedPiece.xIndex, clickedPiece.yIndex, swapTime);

                yield return new WaitForSeconds(swapTime);

                List<GamePiece> clickedPieceMatches = FindMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
                List<GamePiece> targetPieceMatches = FindMatchesAt(targetTile.xIndex, targetTile.yIndex);

                if (clickedPieceMatches.Count == 0 && targetPieceMatches.Count == 0)
                {
                    clickedPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);
                    targetPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
                    yield return new WaitForSeconds(swapTime);
                }
                else
                {
                    yield return new WaitForSeconds(swapTime);

                    ClearAndRefillBoard(clickedPieceMatches.Union(targetPieceMatches).ToList());
                }
                cantidadMovimientos--;
                moveText.text = "Moves :" + cantidadMovimientos.ToString();


            }
        }
        if (cantidadMovimientos <= 0)
        {
            SceneManager.LoadScene("Game_Over");
        }

    }

    void ClearAndRefillBoard(List<GamePiece> gamePieces) // Llamamos a la corutina de ClearAndRefillRoutine para rellenar de nuevo la matriz
    {
        myCount = 0;
        StartCoroutine(ClearAndRefillRoutine(gamePieces)); 
    }

    List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 3) // Encontramos los matches efectuados entre las fichas para determinar si coinciden o no
    {
        List<GamePiece> matches = new List<GamePiece>(); 
        GamePiece startPiece = null;

        if (IsWithBounds(startX, startY))
        {
            startPiece = m_allGamePieces[startX, startY];
        }
        if (startPiece != null) 
        {
            matches.Add(startPiece); 
        }
        else
        {
            return null;
        }

        int nextX;
        int nextY; 

        int maxValue = width > height ? width : height; 

        for (int i = 1; i < maxValue; i++)
        {
            nextX = startX + (int)Mathf.Clamp(searchDirection.x, -1, 1) * i;
            nextY = startY + (int)Mathf.Clamp(searchDirection.y, -1, 1) * i;

            if (!IsWithBounds(nextX, nextY)) 
            {
                break;
            }

            GamePiece nextPiece = m_allGamePieces[nextX, nextY];

            if (nextPiece == null)
            {
                break; 
            }
            else
            {
                if (nextPiece.tipoFicha == startPiece.tipoFicha && !matches.Contains(nextPiece))
                {
                    matches.Add(nextPiece);
                }
                else
                {
                    break;
                }
            }
        }

        if (matches.Count >= minLength)
        {
            return matches;
        }
        else
        {
            return null;
        }
    }

    List<GamePiece> FindVerticalMatches(int startX, int startY, int minLenght = 3) // Encontramos las coincidencias solo en vertical con las coordenadas X,Y y las convertimos en listas combinadas
    {
        List<GamePiece> upwardMatches = FindMatches(startX, startY, Vector2.up, 2);
        List<GamePiece> downwardMatches = FindMatches(startX, startY, Vector2.down, 2);

        if (upwardMatches == null) 
        {
            upwardMatches = new List<GamePiece>(); 
        }

        if (downwardMatches == null)
        {
            downwardMatches = new List<GamePiece>();
        }

        var combinedMatches = upwardMatches.Union(downwardMatches).ToList();
        return combinedMatches.Count >= minLenght ? combinedMatches : null;
    }
    List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLenght = 3) // Encontramos las coincidencias solo en horizontal con las coordenadas X,Y y las convertimos en listas combinadas
    {
        List<GamePiece> rightMatches = FindMatches(startX, startY, Vector2.right, 2);
        List<GamePiece> leftMatches = FindMatches(startX, startY, Vector2.left, 2);

        if (rightMatches == null) 
        {
            rightMatches = new List<GamePiece>();
        }

        if (leftMatches == null)
        {
            leftMatches = new List<GamePiece>();
        }

        var combinedMatches = rightMatches.Union(leftMatches).ToList();
        return combinedMatches.Count >= minLenght ? combinedMatches : null; 
    }

    List<GamePiece> FindMatchesAt(int x, int y, int minLength = 3) // Encontramos las coincidencias combinadas de las dos listas anteriores
    {
        List<GamePiece> horizontalMatches = FindHorizontalMatches(x, y, minLength);
        List<GamePiece> verticalMatches = FindVerticalMatches(x, y, minLength);

        if (horizontalMatches == null)
        {
            horizontalMatches = new List<GamePiece>();
        }

        if (verticalMatches == null)
        {
            verticalMatches = new List<GamePiece>();
        }

        var combinedMatches = horizontalMatches.Union(verticalMatches).ToList();

        if (horizontalMatches.Count != 0 && verticalMatches.Count !=0) // Encontramos la T y la L 
        {
            int cantidadPuntos = 150;
            m_puntaje.SumatoriaPuntos(cantidadPuntos);
            Debug.Log("Combo T o L");
        }
        return combinedMatches;
    }
    List<GamePiece> FindMatchesAt(List<GamePiece> gamePieces, int minLenght = 3) // Encontramos las coincidencias efectuadas de cada una de las piezas
    {
        List<GamePiece> matches = new List<GamePiece>();

        foreach (GamePiece piece in gamePieces)
        {
            matches = matches.Union(FindMatchesAt(piece.xIndex, piece.yIndex, minLenght)).ToList();
        }
        return matches;
    }
    private bool IsNextTo(Tile start, Tile end) // Aqui sabemos si la ficha esta proxima a la otra o no, sabemos si las fichas son vecinas o no
    {
        if (Mathf.Abs(start.xIndex - end.xIndex) == 1 && start.yIndex == end.yIndex) 
        {
            return true;
        }

        if (Mathf.Abs(start.yIndex - end.yIndex) == 1 && start.xIndex == end.xIndex) 
        {
            return true;
        }

        return false;
    }

    private List<GamePiece> FindAllMatches() // Encontramos todas las posibles concidencias en toda la lista de GamePieces
    {
        List<GamePiece> combinedMatches = new List<GamePiece>();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                var matches = FindMatchesAt(i, j);
                combinedMatches = combinedMatches.Union(matches).ToList(); 
            }
        }
        return combinedMatches;
    }
    void HihglightTileOff(int x, int y) // Apagamos los tiles para que se puedan "encender" cuando hagamos un match
    {
        SpriteRenderer spriteRenderer = m_allTiles[x, y].GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b,0);
    }


    private void HihglightTileOn(int x, int y, Color col) // Prendemos los tiles de un color mas fuerte que el de por defecto para dar una guia visual
    {
        SpriteRenderer spriteRenderer = m_allTiles[x, y].GetComponent<SpriteRenderer>();
        spriteRenderer.color = col;
    }

    public void HighlightMatchesAt(int x, int y) // Encontramos las coincidencias para poder encender los matches de las fichas en las posiciones correctas
    {
        HihglightTileOff(x, y);
        var combinedMatches = FindMatchesAt(x, y);

        if (combinedMatches.Count > 0) 
        {
            foreach (GamePiece piece in combinedMatches)
            {
                HihglightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }
    public void HighLightMatches() // Encontramos la ubicacion de las coincidencias para poder saber su posicion y asi encenderlas
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                HighlightMatchesAt(i, j);
            }
        }
    }

    void HighLightPieces(List<GamePiece> gamePieces) // Accedemos al componente del spriterenderer y asi cambiar el color del Tile
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                HihglightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    private void ClearPieceAt(int x, int y) // Destruimos las piezas con las que hagamos match
    {
        GamePiece pieceToClear = m_allGamePieces[x, y];
        if (pieceToClear != null)
        {
            m_allGamePieces[x, y] = null;
            audioSource.Play();
            Destroy(pieceToClear.gameObject); 
        }
        HihglightTileOff(x, y); 
    }
    void ClearPiecesAt(List<GamePiece> gamePieces) // Encontramos la posicion de la pieza a la que queremos borrar
    {
        foreach (GamePiece piece in gamePieces) 
        {
            if (piece != null)
            {
                ClearPieceAt(piece.xIndex, piece.yIndex);
            }
        }
    }

    void ClearBoard() // Recorremos el board en su totalidad para saber que pieza y en que posicion la eliminamos
    {
        for (int i = 0; i < width; i++) 
        {
            for (int j = 0; j < height; j++) 
            {
                ClearPieceAt(i, j); 
            }
        }
    }
    GameObject GetRandomPiece() // Creamos una pieza aleatoria
    {
        int randomInx = Random.Range(0, gamePiecesPrefabs.Length);
        if (gamePiecesPrefabs[randomInx] == null)
        {
            Debug.LogWarning($"La clase Board en el array de prefabs en la posicion{randomInx} no contiene una pieza valida");
        }
        return gamePiecesPrefabs[randomInx];
    }

    public void PlaceGamePiece(GamePiece gamePiece, int x, int y) // Colocamos las coordenadas en las piezas
    {
        if (gamePiece == null) 
        {
            Debug.LogWarning($"gamePiece invalida"); 
            return;
        }

        gamePiece.transform.position = new Vector2(x, y); //
        gamePiece.transform.rotation = Quaternion.identity; //

        if (IsWithBounds(x, y)) //
        {
            m_allGamePieces[x, y] = gamePiece; //
        }

        gamePiece.SetCoord(x, y); //
    }

    bool IsWithBounds(int x, int y) // Sabemos si esta en rango o no 
    {
        return (x >= 0 && x < width && y >= 0 && y < height); //
    }
    GamePiece FillRandomAt(int x, int y, int falseOffset = 0, float moveTime = .1f) // Llenamos la matriz con las piezas aleatorias
    {
        GamePiece randomPiece = Instantiate(GetRandomPiece(), Vector2.zero, Quaternion.identity).GetComponent<GamePiece>(); //
        if (randomPiece != null) 
        {
            randomPiece.Init(this);
            PlaceGamePiece(randomPiece, x, y);

            if (falseOffset != 0) 
            {
                randomPiece.transform.position = new Vector2(x, y + falseOffset);
                randomPiece.Move(x, y, moveTime);
            }
            randomPiece.transform.parent = gamePieceParent;
        }
        return randomPiece; 
    }
    void ReplaceWithRandom(List<GamePiece> gamePieces, int falseOffset = 0, float moveTime = .1f) // Reemplazamos una ficha con otra ficha aleatoriamente
    {
        foreach (GamePiece piece in gamePieces) 
        {
            ClearPieceAt(piece.xIndex, piece.yIndex);
            if (falseOffset == 0)
            {
                FillRandomAt(piece.xIndex, piece.yIndex); 
            }
            else
            {
                FillRandomAt(piece.xIndex, piece.yIndex, falseOffset, moveTime); 
            }
        }
    }

    List<GamePiece> CollapseColumn(int column, float collapseTime = .1f) // Hacemos que la columna caiga cuando destruimos los matches y no se quede el espacio vacio
    {
        List<GamePiece> movingPieces = new List<GamePiece>(); 

        for (int i = 0; i < height - 1; i++) 
        {
            if (m_allGamePieces[column, i] == null)
            {
                for (int j = i + 1; j < height; j++) 
                {
                    if (m_allGamePieces[column, j] != null) 
                    {
                        m_allGamePieces[column, j].Move(column, i, collapseTime * (j - i)); 

                        m_allGamePieces[column, i] = m_allGamePieces[column, j];
                        m_allGamePieces[column, i].SetCoord(column, i);

                        if (!movingPieces.Contains(m_allGamePieces[column, i]))
                        {
                            movingPieces.Add(m_allGamePieces[column, i]); 
                        }

                        m_allGamePieces[column, j] = null;
                        break;
                    }
                }
            }
        }
        return movingPieces;
    }

    List<GamePiece> CollapseColumn(List<GamePiece> gamePieces) // Hacemos que la columna caiga cuando destruimos los matches y no se quede el espacio vacio
    {
        List<GamePiece> movingPieces = new List<GamePiece>(); 
        List<int> collumnsToCollapse = GetCollumns(gamePieces); 

        foreach (int column in collumnsToCollapse) 
        {
            movingPieces = movingPieces.Union(CollapseColumn(column)).ToList(); 
        }

        return movingPieces;
    }
    List<int> GetCollumns(List<GamePiece> gamePieces) // Conseguimos los indices de las columnas y conocer su posicion
    {
        List<int> collumns = new List<int>();
        foreach (GamePiece piece in gamePieces)
        {
            if (!collumns.Contains(piece.xIndex))
            {
                collumns.Add(piece.xIndex);
            }
        }
        return collumns; 
    }

    IEnumerator ClearAndRefillRoutine(List<GamePiece> gamePieces) // Sumamos los puntos de los matches y hacemos que se borren las fichas y se generen nuevas
    {
        m_playerInputEnabled = true; 
        List<GamePiece> matches = gamePieces;

           foreach (GamePiece piece in matches) // Sumamos los puntos
            {
                if (matches.Count == 3)
                {
                    int cantidadPuntos = 10;
                    m_puntaje.SumatoriaPuntos(cantidadPuntos);
                }

                if (matches.Count == 4)
                {
                    int cantidadPuntos = 20;
                    m_puntaje.SumatoriaPuntos(cantidadPuntos);
                }

                if (matches.Count == 5)
                {
                    int cantidadPuntos = 30;
                    m_puntaje.SumatoriaPuntos(cantidadPuntos);
                }

                if (matches.Count >= 6)
                {
                    int cantidadPuntos = 40;
                    m_puntaje.SumatoriaPuntos(cantidadPuntos);
                }
            }
        do
        {
            yield return StartCoroutine(ClearAndCollapseRoutine(matches)); 
            yield return null; 
            yield return StartCoroutine(RefillRoutine()); 
            matches = FindAllMatches(); 
            yield return new WaitForSeconds(.5f); 
        } 
        while (matches.Count != 0);
        m_playerInputEnabled = true; 

    }
    IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces) // Hacemos que las fichas caigan a las posiciones que esten vacias por los matches
    {
        myCount++; 
        List<GamePiece> movingPieces = new List<GamePiece>(); 
        List<GamePiece> matches = new List<GamePiece>();
        HighLightPieces(gamePieces);
        yield return new WaitForSeconds(.5f); 
        bool isFinished = false;

        while (!isFinished) 
        {
            ClearPiecesAt(gamePieces); 
            yield return new WaitForSeconds(.25f);

            movingPieces = CollapseColumn(gamePieces);
            while (!IsCollapsed(gamePieces))
            {
                yield return null;
            }
            yield return new WaitForSeconds(.5f);

            matches = FindMatchesAt(movingPieces);

            if (matches.Count == 0)
            {
                isFinished = true;
                break;
            }
            else
            {
                foreach (GamePiece piece in matches) // Hacemos que se multipliquen los matches cuando las columnas colapsan
                {
                    if (matches.Count == 3)
                    {
                        int cantidadPuntos = 10 * myCount;

                        m_puntaje.SumatoriaPuntos(cantidadPuntos);

                    }
                    if (matches.Count == 4)
                    {
                        int cantidadPuntos = 20 * myCount;

                        m_puntaje.SumatoriaPuntos(cantidadPuntos);

                    }
                    if (matches.Count == 5)
                    {
                        int cantidadPuntos = 30 * myCount;

                        m_puntaje.SumatoriaPuntos(cantidadPuntos);

                    }
                    if (matches.Count >= 6)
                    {
                        int cantidadPuntos = 40 * myCount;

                        m_puntaje.SumatoriaPuntos(cantidadPuntos);

                    }
                }

                yield return StartCoroutine(ClearAndCollapseRoutine(matches)); //
            }
        }
        yield return null; //
    }

    IEnumerator RefillRoutine() // Rellenamos la matriz
    {
        FillBoard(10, .5f);
        yield return null;
    }
    bool IsCollapsed(List<GamePiece> gamePieces) // Sabemos si la ficha se puede colapsar o no
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                if (piece.transform.position.y - (float)piece.yIndex > 0.001f)
                {
                    return false;
                }
            }
        }
        return true;
    }

}


