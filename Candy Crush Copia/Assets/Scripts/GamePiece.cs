using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    public int xIndex; // Indice
    public int yIndex; // Indice

    Board m_board; // Referencia al script del Board

    bool m_isMoving = false; // Sabemos si se mueve o no
    
    public TipoMovimiento tipoDeMovimiento; // Controlamos el tipo de movimiento que van a tener las fichas
    public TipoFicha tipoFicha; // El tipo de fichas para poder identificarlas

    public void SetCoord(int x , int y) // Inicializamos para dar a conocer las coordenadas
    {
        xIndex = x;
        yIndex = y;
    }

    public void Init(Board board) // Inicializamos el board
    {
        m_board = board;
    }

    public void Move(int x, int y, float moveTime) // Llamamos la corutina de moveRoutine
    {
        if (!m_isMoving)
        {
            StartCoroutine(MoveRoutine(x, y, moveTime));
        }
    }

    IEnumerator MoveRoutine(int destX, int destY, float timeToMove) // Hacemos que las fichas se puedan mover y si sabemos si se selecciona algun tipo de movimiento y hace el calculo correspondiente
    {
        Vector2 startPosition = transform.position;
        bool reacedDestination = false;
        float elapsedTime = 0f;
        m_isMoving = true;

        while (!reacedDestination)
        {
            if (Vector2.Distance(transform.position, new Vector2(destX, destY)) < 0.01f)
            {
                reacedDestination = true;

                if (m_board != null)
                {
                    m_board.PlaceGamePiece(this, destX, destY);
                }
                break;
            }

            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp(elapsedTime / timeToMove, 0f, 1f);

            switch (tipoDeMovimiento) // Interpolation
            {
                case TipoMovimiento.Linear:

                    break;

                case TipoMovimiento.EaseOut:
                    t = Mathf.Sin(t * Mathf.PI * .5f);
                    break;

                case TipoMovimiento.EseIn:
                    t = 1 - Mathf.Cos(t * Mathf.PI * .5f);
                    break;

                case TipoMovimiento.SmoothStep:
                    t = t * t * (3 - 2 * t);
                    break;

                case TipoMovimiento.SmootherStep:
                    t = t * t * t * (t * (t * 6 - 15) + 10);
                    break;   
            }

            transform.position = Vector2.Lerp(startPosition, new Vector2(destX, destY), t);
            yield return null;
        }
        m_isMoving = false;

    }
    public enum TipoMovimiento // Lista para que podamos seleccionar en el editor el tipo de movimiento
    {
        Linear,
        EaseOut,
        EseIn,
        SmoothStep,
        SmootherStep
    }
    public enum TipoFicha // Lista para que podamos seleccionar en el editor el tipo de ficha
    {
        Naranja,
        Manzana,
        Pera,
        Durazno,
        Granada,
        Limon,
        Sandia,
        Aguacate,
        Morazul,
        Pina,
    }
}

