using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    public int xIndex;
    public int yIndex;

    Board m_board;

    public bool m_isMoving = false; // Devolverlo a private
    
    public TipoMovimiento tipoDeMovimiento; // Interpolation
    public TipoFicha tipoFicha; // MatchValue

    public void SetCoord(int x , int y)
    {
        xIndex = x;
        yIndex = y;
    }

    public void Init(Board board)
    {
        m_board = board;
    }

    public void Move(int x, int y, float moveTime)
    {
        if (!m_isMoving)
        {
            StartCoroutine(MoveRoutine(x, y, moveTime));
        }
    }

    IEnumerator MoveRoutine(int destX, int destY, float timeToMove)
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
    public enum TipoMovimiento // InterType
    {
        Linear,
        EaseOut,
        EseIn,
        SmoothStep,
        SmootherStep
    }
    public enum TipoFicha // MatchValue
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

